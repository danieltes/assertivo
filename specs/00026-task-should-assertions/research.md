# Research: Should() Entry Point for Task Subjects

**Feature**: 00026-task-should-assertions  
**Date**: 2026-05-17  
**Status**: Complete — all NEEDS CLARIFICATION resolved

---

## R-001: Overload Resolution — `Task` vs. `Func<Task>` Coexistence

### Decision

Add `Should(this Task? subject, ...)` as a new overload alongside the existing
`Should(this Func<Task> subject, ...)`. No conflict arises; the two bind to
distinct compile-time types.

### Rationale

C# overload resolution selects the candidate whose parameter type requires the
least conversion from the argument type. A variable declared as `Task` is an
exact match for `Should(this Task? subject)` and has no conversion to
`Func<Task>`, so resolution is unambiguous. Conversely, a variable declared as
`Func<Task>` has no conversion to `Task`, so it continues to bind to the
existing overload.

The boundary case `service.DoAsync().Should()` also resolves correctly: the
return type of any `async Task` method is `Task`, so the call-site expression
has compile-time type `Task` and binds to the new overload.

### Alternatives Considered

- **Single overload accepting `object`**: Rejected — violates the Pit of Success
  principle; callers could not predict the returned assertion type at compile time.
- **Adapt `Task` to `Func<Task>` inside `Should()`**: Rejected — semantically
  incorrect. Wrapping an already-started task as `() => subject` would not
  re-execute the task; it would return the same completed/faulted task on each
  call, but the intent of `AsyncFunctionAssertions` is deferred execution. The
  two types have different semantics and should remain separate.

---

## R-002: `Task<T>` Coverage via Inheritance

### Decision

No separate `Task<T>` overload is introduced. `Task<T>` subjects bind to
`Should(this Task? subject)` via the standard `Task<T> : Task` inheritance chain.

### Rationale

C# implicit reference conversion allows a `Task<string>` to be passed where
`Task` is expected. Since the new overload targets `Task?`, a `Task<T>` variable
declared as `Task<T>` will be implicitly converted to `Task` and bind to the new
overload. The return value of type `T` is unreachable from a `Task` reference, so
it is correctly discarded; only the fault/completion state is relevant for
`ThrowAsync`.

Verified against the spec's Assumptions section: "Task<T> subjects are in scope
via inheritance… The return value T is not surfaced."

### Alternatives Considered

- **Separate `Task<T>` overload**: Rejected as out of scope per spec (§Out of Scope).
  Would be an additive non-breaking change in a future feature if needed.

---

## R-003: Null Task Subject — Guard Location

### Decision

The null check is deferred to `ThrowAsync`, not performed at the `.Should()`
call site. A null `Task` subject is accepted into the `TaskAssertions` struct
and produces an `AssertionFailedException` when `ThrowAsync` is awaited.

### Rationale

This aligns with the spec (FR-010 and edge case definition). The design rationale
is that null task detection is an *assertion failure*, not a guard violation: the
developer is asserting something about the task, and a null task is a failing
condition that should be expressed via the normal assertion failure mechanism.

This differs intentionally from `Func<T>.Should()`, which throws
`ArgumentNullException` immediately, because a null delegate cannot be
meaningfully invoked at all — it is a programming error rather than a testable
condition. A null `Task` returned from a method under test *is* a testable
condition.

The `actual` message for the null case is `"task was null"`, following the
`"no exception was thrown"` pattern established by `AsyncFunctionAssertions`.

### Alternatives Considered

- **`ArgumentNullException` at `.Should()`**: Rejected — inconsistent with spec,
  and prevents developers from writing `nullTask.Should().ThrowAsync<X>()` as a
  negative assertion on code that incorrectly returns null.

---

## R-004: Wrong-Type Failure Message Format

### Decision

When `ThrowAsync<TException>` fails because the task faulted with the wrong type,
the `actual` argument to `MessageFormatter.Fail` is:

```text
<{target.GetType().FullName}>: {target.Message}
```

where `target` is the possibly-unwrapped exception (after single-inner
`AggregateException` extraction).

### Rationale

Per clarification session 2026-05-17 (Q2 answer A): type names + actual exception
`.Message` text. This satisfies SC-004 ("enough information to identify the
problem without running a debugger") by surfacing the exception message alongside
the type — the most actionable diagnostic detail available.

The `target` (unwrapped) exception is used rather than `caught` (raw) so that when
`AggregateException` was unwrapped, the reported type and message match the actual
exception that was inspected, not its wrapper.

Example output:
```
Expected <System.ArgumentNullException> but found
<System.InvalidOperationException>: Value cannot be null. (Parameter 'key')
Expression: service.LookupAsync(null)
```

### Alternatives Considered

- **Type names only**: Rejected per Q2 answer.
- **Include stack trace of actual exception**: Rejected — too verbose for a
  summary failure message; the exception is available via `.Which` if needed.

---

## R-005: `ThrowAsync` Implementation Pattern — Mirror `AsyncFunctionAssertions`

### Decision

`TaskAssertions.ThrowAsync<TException>` mirrors the structure of
`AsyncFunctionAssertions.ThrowAsync<TException>` exactly, with two differences:
(1) it awaits `Subject` directly (not `Subject()`), and (2) it prefixes a null
guard that produces `AssertionFailedException("task was null")`.

### Rationale

Identical `AggregateException` unwrapping logic, identical `MessageFormatter.Fail`
call signatures, identical `ExceptionAssertions<TException>` return type, and
identical `[StackTraceHidden]` annotation. Developers reading either type will find
the pattern immediately familiar. No new abstraction is needed; the two structs
remain independent (no shared base class or helper).

### Code sketch

```csharp
[StackTraceHidden]
public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(
    string because = "", params object[] becauseArgs)
    where TException : Exception
{
    if (Subject is null)
        MessageFormatter.Fail("task to be non-null", "task was null",
            Expression, because, becauseArgs);

    Exception? caught = null;
    try { await Subject!.ConfigureAwait(false); }
    catch (Exception ex) { caught = ex; }

    if (caught is null)
        MessageFormatter.Fail($"<{typeof(TException).FullName}> to be thrown",
            "no exception was thrown", Expression, because, becauseArgs);

    var target = caught;
    if (target is AggregateException agg && agg.InnerExceptions.Count == 1)
        target = agg.InnerExceptions[0];

    if (target is TException typed)
        return new ExceptionAssertions<TException>(typed, Expression);

    MessageFormatter.Fail(
        $"<{typeof(TException).FullName}>",
        $"<{target!.GetType().FullName}>: {target.Message}",
        Expression, because, becauseArgs);
    return default!;
}
```
