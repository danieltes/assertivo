# Research: Should() Type-Aware Dispatch

**Feature**: 00018-should-type-dispatch  
**Date**: 2026-04-25  
**Status**: Complete — all NEEDS CLARIFICATION resolved

---

## R-001: C# Overload Resolution — Why `IReadOnlyList<T>` Falls Back to `ObjectAssertions<T>`

### Decision
Add explicit extension method overloads for `IReadOnlyList<T>` and `IReadOnlyCollection<T>`.

### Rationale

C# overload resolution ranks candidate methods by the quality of the implicit conversion
required for each argument. For a subject declared as `IReadOnlyList<string>`, two
candidates exist in the current `ShouldExtensions`:

| Candidate | Type parameter binding | Conversion required |
|-----------|----------------------|---------------------|
| `Should<T>(this IEnumerable<T>? subject)` | T = string | Implicit reference conversion (IReadOnlyList→IEnumerable) |
| `Should<T>(this T subject)` | T = IReadOnlyList\<string\> | Identity conversion (exact match) |

C# §11.6.4.3 (Better conversion target) states that an identity conversion is always
better than any other conversion. The unconstrained `T` fallback therefore wins, and
`.Should()` on an `IReadOnlyList<string>` returns `ObjectAssertions<IReadOnlyList<string>>`
rather than `GenericCollectionAssertions<string>`.

Adding an explicit `IReadOnlyList<T>` overload creates a third candidate that is an
exact match *and* has a more specific parameter type than the unconstrained `T` overload.
C# §11.6.4.2 (Better function member) prefers the more specific type, resolving the
ambiguity deterministically.

The same analysis applies identically to `IReadOnlyCollection<T>`.

### Alternatives Considered
- **Runtime type-check inside the generic `T` overload**: Rejected — violates the
  Pit of Success principle (dispatch becomes runtime, not compile-time) and the
  Zero Surprise principle (callers cannot predict the returned type at compile time).
- **Remove the generic `T` fallback**: Rejected — it is needed for `ObjectAssertions<T>`
  on arbitrary reference types; removing it would break users who rely on null-checking
  any arbitrary object.

---

## R-002: `IReadOnlyDictionary<TKey,TValue>` Dispatch — Pre-Satisfied

### Decision
No code change required for `IReadOnlyDictionary<TKey,TValue>`. User Story 3 (P3)
from the spec is already satisfied by the existing codebase.

### Rationale

`Should.cs` already contains:

```csharp
public static GenericDictionaryAssertions<TKey, TValue> Should<TKey, TValue>(
    this IReadOnlyDictionary<TKey, TValue>? subject, ...)
    where TKey : notnull
    => new(subject, caller);
```

This overload was introduced before this feature was specified. Code inspection at
commit HEAD on branch `00018-should-type-dispatch` confirms its presence at line ~58.
The `NotBeNull()`, `ContainKey()`, and `HaveCount()` acceptance scenarios in User
Story 3 are therefore already passing in the existing test suite.

### Alternatives Considered
- **Delete and re-add**: No value; the existing overload is correct.
- **Add a test anyway**: A test that verifies `IReadOnlyDictionary` dispatch is correct
  is valuable as a regression guard; it can be added to `ShouldDispatchTests.cs`
  alongside the other dispatch regression tests.

---

## R-003: `Func<T>` Adaptation Strategy

### Decision
Adapt `Func<T>` to `Action` inline within the `.Should()` extension method overload
via `() => subject()`. Pass the resulting `Action` directly to the existing
`ActionAssertions(Action, string?)` constructor. No new class is introduced.

### Rationale

The `ActionAssertions` constructor signature is:

```csharp
internal ActionAssertions(Action subject, string? expression)
```

`Func<T>` (where T is any non-void type) is covariant with `Action` in terms of
invocation behavior: both are zero-argument delegates. The return value of `Func<T>`
is explicitly out of scope (spec FR-003, clarification Q2). The adaptation:

```csharp
public static ActionAssertions Should<T>(
    this Func<T> subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
{
    ArgumentNullException.ThrowIfNull(subject);
    return new ActionAssertions(() => subject(), caller);
}
```

allocates exactly one `Action` delegate on the heap. This is acceptable because:
1. Exception-assertion paths are inherently not zero-allocation (they involve
   try/catch and exception object construction).
2. The constitution's zero-allocation budget applies to the happy path (assertion
   passes), which for `Throw<T>()` involves the delegate being invoked, catching
   an exception, and constructing an `ExceptionAssertions<T>` — allocations are
   unavoidable regardless of the adaptation approach.

The lambda `() => subject()` captures `subject` (one closure slot); this is the
minimum possible allocation for any adaptation strategy that wraps `Func<T>`.

### Alternatives Considered
- **New `FuncAssertions<T>` class**: Rejected per spec clarification Q1 (answered
  "A — adapt internally"). Would duplicate ~80 LOC from `ActionAssertions` with
  zero new behavior.
- **Extend `ActionAssertions` to accept `Func<T>`**: Rejected — `ActionAssertions`
  is a `readonly struct` and its `Subject` property is typed as `Action`; adding
  a `Func<T>` constructor would require either a new overload of `Subject` or
  storing a `Func<T>` separately, both of which break the single-responsibility
  design.

---

## R-004: Null Handling Asymmetry

### Decision
Collection overloads (`IReadOnlyList<T>?`, `IReadOnlyCollection<T>?`) accept null and
forward it to `GenericCollectionAssertions<T>`. The `Func<T>` overload rejects null
immediately with `ArgumentNullException.ThrowIfNull(subject)`.

### Rationale

This asymmetry is intentional and matches existing patterns in `Should.cs`:
- All collection and string overloads accept nullable subjects (the `?` suffix on
  the parameter type) and forward the null into the assertion object, allowing
  `subject.Should().BeNull()` to pass.
- The `Action` and `Func<Task>` overloads do not use nullable subjects — they
  accept non-nullable delegates.

`Func<T>` must be non-nullable for the same reason as `Action`: a null delegate
cannot be invoked to test throwing or non-throwing behavior, so accepting null
would produce a `NullReferenceException` inside the assertion rather than a
meaningful assertion failure.

`ArgumentNullException.ThrowIfNull` is used (BCL-only, AOT-safe, available on
.NET 6+) rather than a manual null check to stay consistent with modern .NET
patterns and minimize verbosity.

### Alternatives Considered
- **Nullable `Func<T>?` with `BeNull()` support**: Rejected per spec clarification
  Q3 (answered "A — throw ArgumentNullException at call site").

---

## R-005: `IEnumerable<T>` Regression — Overload Shadowing Safety

### Decision
Add an explicit regression test in `ShouldDispatchTests.cs` that asserts a subject
declared as `IEnumerable<T>` resolves to `GenericCollectionAssertions<T>`.

### Rationale

Adding `IReadOnlyList<T>` and `IReadOnlyCollection<T>` overloads does not change
the resolution path for `IEnumerable<T>` — the existing `IEnumerable<T>` overload
remains the best match for a variable explicitly typed as `IEnumerable<T>` (identity
conversion). However, a future refactor (e.g., reordering overloads or introducing
a new `IReadOnlyEnumerable<T>` overload) could silently break this. An explicit
compile-time dispatch test locks in the contract.

The test uses `Assert.IsType<GenericCollectionAssertions<string>>(...)` (or equivalent
pattern) to verify the static return type at runtime. This is the standard pattern for
verifying dispatch in C# extension method test suites.

### Alternatives Considered
- **Rely on existing tests**: Rejected per spec clarification Q5 (answered "A —
  add explicit regression test"). Existing tests assert *behavior* (HaveCount, Contain,
  etc.); none assert *dispatch type*.
