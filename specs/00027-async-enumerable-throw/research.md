# Research: ThrowAsync Support for IAsyncEnumerable<T> Subjects

**Feature**: 00027-async-enumerable-throw  
**Date**: 2026-05-27  
**Status**: Complete — all NEEDS CLARIFICATION resolved

---

## R-001: `await foreach` vs. Manual Enumerator — DisposeAsync Exception Handling

### Decision

Use a **manual enumerator pattern** (`GetAsyncEnumerator()` / `MoveNextAsync()` /
`DisposeAsync()`) instead of `await foreach` to implement `ThrowAsync`. This is
required to correctly preserve the original enumeration exception when `DisposeAsync`
also throws (clarification §2026-05-27 Q1).

### Rationale

The C# compiler desugars `await foreach` into a `try/finally` block that calls
`DisposeAsync()` in the `finally` clause. Standard `finally` semantics in C# cause
an exception thrown inside a `finally` block to replace any in-flight exception from
the `try` block. Therefore, if an iterator throws during `MoveNextAsync()` *and*
`DisposeAsync()` also throws, `await foreach` would surface the `DisposeAsync`
exception and silently lose the original iteration exception — the one the test
author actually cares about.

The clarified requirement (Q1) mandates preserving the original enumeration
exception and discarding the `DisposeAsync` exception. To achieve this, manual
enumeration is used with a `catch when (caught is not null)` exception filter on
the `DisposeAsync` call:

```csharp
var enumerator = subject.GetAsyncEnumerator();
Exception? caught = null;
try
{
    while (true)
    {
        bool hasNext;
        try { hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false); }
        catch (Exception ex) { caught = ex; break; }
        if (!hasNext) break;
    }
}
finally
{
    try { await enumerator.DisposeAsync().ConfigureAwait(false); }
    catch when (caught is not null) { /* discard — original iteration exception takes precedence */ }
}
```

The `catch when (caught is not null)` exception filter ensures:
- If only `DisposeAsync` throws (no prior iteration exception): the disposal
  exception propagates normally.
- If both iteration and `DisposeAsync` throw: the disposal exception is swallowed;
  the original iteration exception is re-thrown after the `finally` block completes.

### `ConfigureAwait(false)` correctness

Both `MoveNextAsync()` and `DisposeAsync()` calls use `.ConfigureAwait(false)`.
This matches the intent of FR-003 ("use `ConfigureAwait(false)`") and is correct
for library code that should not marshal back to any synchronization context.

### AOT compatibility

`GetAsyncEnumerator()` is a regular interface method call. No reflection, no
`Type.MakeGenericType`, no runtime code generation. AOT-safe. ✅

### Alternatives Considered

- **`await foreach` with `ConfigureAwait(false)`**: Rejected because it does not
  allow catching and discarding `DisposeAsync` exceptions independently of
  iteration exceptions. The compiler-generated `finally` block semantics make
  this impossible without wrapping the entire `await foreach` in a separate
  try/catch, which is equivalent to the manual pattern but less clear.
- **Catch all exceptions including `DisposeAsync` into one variable**: Rejected
  because it conflates two distinct exception sources. The spec requires preserving
  the *iteration* exception, not whichever was thrown last.

---

## R-002: Overload Resolution — `IAsyncEnumerable<T>` Dispatch

### Decision

Add `Should<T>(this IAsyncEnumerable<T>? subject, ...)` as a new overload in
`ShouldExtensions`. No conflict arises with any existing overload.

### Rationale

`IAsyncEnumerable<T>` does not extend `IEnumerable<T>`, `Task`, `Func<Task>`,
`Action`, or any other type targeted by existing overloads. C# overload resolution
will match `IAsyncEnumerable<T>` to the new overload as an exact interface match,
which is more specific than the `ObjectAssertions<T>` generic fallback. There is
no ambiguity.

The boundary case of types that implement both `IAsyncEnumerable<T>` and
`IEnumerable<T>` could theoretically produce an overload resolution ambiguity,
but in practice no BCL type does this, and the spec's scope covers only
`IAsyncEnumerable<T>` subjects.

### Alternatives Considered

- **Accepting `object` and downcasting at runtime**: Rejected — violates Pit of
  Success; would lose compile-time type safety and return `ObjectAssertions<T>`
  for unexpected types.

---

## R-003: Message Templates — Parity with `TaskAssertions.ThrowAsync`

### Decision

Use the **same `MessageFormatter.Fail` call patterns** as `TaskAssertions.ThrowAsync`,
substituting "source" for "task" in all wording (clarification §2026-05-27 Q3).

### Rationale

This produces consistent, predictable messages across all `ThrowAsync` entry points.
Library test assertions can use shared substring checks (`.Should().Contain(...)`)
rather than entry-point-specific strings. The substitution is minimal and
domain-appropriate: "source" is the canonical term for an `IAsyncEnumerable<T>`
in streaming-pipeline terminology.

### Concrete message templates

| Failure path | `expected` arg | `actual` arg |
|---|---|---|
| Null subject | `"source to be non-null"` | `"source was null"` |
| No exception thrown | `$"source to throw {typeof(TException).Name}"` | `"no exception was thrown"` |
| Wrong exception type | `typeof(TException).Name` | `$"{target.GetType().Name}: {target.Message}"` |

Where `target` is the possibly-unwrapped exception (after single-inner
`AggregateException` extraction per R-004).

---

## R-004: AggregateException Unwrapping — Mirror `TaskAssertions`

### Decision

Apply the identical single-inner-exception unwrapping logic used in
`TaskAssertions.ThrowAsync` and `AsyncFunctionAssertions.ThrowAsync`.

### Rationale

Parity with existing entry points is mandated by FR-004 and the spec's
acceptance scenario 5 (US-1). The logic is: if `caught` is an
`AggregateException` with exactly one inner exception, replace `caught` with
`caught.InnerExceptions[0]` for type-matching and result construction. A
zero-inner or multi-inner `AggregateException` is not unwrapped and is treated
as the caught exception itself.

```csharp
var target = caught;
if (caught is AggregateException { InnerExceptions.Count: 1 } ae)
    target = ae.InnerExceptions[0];
```

---

## R-005: `ExceptionAssertions<TException>` Reuse

### Decision

`ThrowAsync<TException>` returns `new ExceptionAssertions<TException>(typed, Expression)`
— the existing internal constructor, unchanged.

### Rationale

Per FR-008 and spec clarification §2026-05-27 Q3: reuse the same return type as
`TaskAssertions.ThrowAsync` and `AsyncFunctionAssertions.ThrowAsync`. This ensures
`.Which`, `.And`, and `.WithMessage(...)` chaining work identically after any
`ThrowAsync` call, regardless of entry point. No changes to `ExceptionAssertions.cs`.

---

## R-006: Null Subject Guard Location

### Decision

The null check is deferred to `ThrowAsync`, not performed at the `.Should()` call
site. A null `IAsyncEnumerable<T>` subject is accepted into `AsyncEnumerableAssertions<T>`
and produces an `AssertionFailedException` when `ThrowAsync` is awaited.

### Rationale

Mirrors the decision in feature 00026 (`TaskAssertions`) for the same reasons: a
null source returned from code under test is a *testable condition* (an assertion
failure), not a programming error (a guard violation). The developer is asserting
something about the source; null is a meaningful failing state.

This differs from `Func<T>.Should()`, which throws `ArgumentNullException`
immediately, because a null delegate cannot be invoked at all — it is a
programming error, not a testable assertion subject.
