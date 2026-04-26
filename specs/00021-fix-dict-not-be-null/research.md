# Research: Collection and Dictionary Null-Guard Assertions

**Branch**: `00021-fix-dict-not-be-null`  
**Date**: 2026-04-26  
**Status**: Complete — no NEEDS CLARIFICATION items remain

---

## Research Item 1: Exact method signature already used for `NotBeNull` in ObjectAssertions

**Question**: What exact signature does the existing `NotBeNull` use, so the new methods can be consistent?

**Finding** (from `src/Assertivo/ObjectAssertions.cs`):

```csharp
[StackTraceHidden]
public AndConstraint<ObjectAssertions<T>> NotBeNull(string because = "", params object[] becauseArgs)
{
    if (Subject is null)
    {
        MessageFormatter.Fail(
            "not <null>",
            "<null>",
            Expression, because, becauseArgs);
    }
    return new AndConstraint<ObjectAssertions<T>>(this);
}

[StackTraceHidden]
public AndConstraint<ObjectAssertions<T>> BeNull(string because = "", params object[] becauseArgs)
{
    if (Subject is not null)
    {
        MessageFormatter.Fail(
            "<null>",
            MessageFormatter.FormatValue(Subject),
            Expression, because, becauseArgs);
    }
    return new AndConstraint<ObjectAssertions<T>>(this);
}
```

**Decision**: Mirror this signature verbatim, substituting the concrete assertion type in the return position.

**Alternatives considered**: Adding a shared interface or base type that both `ObjectAssertions<T>` and the collection/dictionary types could inherit. Rejected — the constitution disallows restructuring the inheritance hierarchy for this change, and the struct constraint makes inheritance impossible anyway (both types are `readonly struct`).

---

## Research Item 2: Null failure message format for GuardNull vs NotBeNull

**Question**: `GenericDictionaryAssertions` already has a private `GuardNull` that fires when other methods (e.g., `ContainKey`) are called on a null subject. Does `NotBeNull` need to use the same message, or a different one?

**Finding**: `GuardNull` uses `"a non-null dictionary"` / `"<null>"`. This is appropriate when the caller invokes a method that *requires* a non-null subject as a precondition. `NotBeNull()` is itself the assertion — its message should read exactly like `ObjectAssertions<T>`:

```
Expected not <null> but found <null>.
```

i.e., `expected = "not <null>"`, `actual = "<null>"`.

`BeNull()` on a non-null subject should report:

```
Expected <null> but found <value>.
```

i.e., `expected = "<null>"`, `actual = MessageFormatter.FormatValue(Subject)`.

**Decision**: Use the `ObjectAssertions<T>` message pattern directly. `GuardNull` is unchanged and remains the precondition check for all other methods.

---

## Research Item 3: Impact of `readonly struct` on implementation

**Question**: Both `GenericDictionaryAssertions<TKey, TValue>` and `GenericCollectionAssertions<T>` are `readonly struct`. Does this impose any constraints on the implementation?

**Finding**: No. `readonly struct` means no field can be assigned after construction, but `NotBeNull` and `BeNull` do not mutate state — they only read `Subject` and `Expression`, then return `new AndConstraint<T>(this)`. The `this` copy is valid (struct copy semantics). The existing `ContainKey` and `HaveCount` methods work identically.

**Decision**: No special handling required. Implementation is identical to a class-based assertions type.

---

## Research Item 4: Zero-allocation on passing path

**Question**: Does `return new AndConstraint<T>(this)` allocate?

**Finding**: `AndConstraint<T>` is also a `readonly struct`:

```csharp
public readonly struct AndConstraint<TAssertions>
{
    public AndConstraint(TAssertions parent) { And = parent; }
    public TAssertions And { get; }
}
```

**Decision**: Zero heap allocation on the passing path (struct-on-stack). Satisfies constitution §6.2. No allocations benchmark needed for this change (follows the same pattern as all existing passing-path methods).

---

## Research Item 5: `[StackTraceHidden]` requirement

**Question**: Is `[StackTraceHidden]` required on the new methods?

**Finding**: Every assertion method in the codebase that calls `MessageFormatter.Fail` or returns an `AndConstraint` is decorated with `[StackTraceHidden]` (e.g., `ContainKey`, `HaveCount`, `BeNull`, `NotBeNull` on `ObjectAssertions<T>`). This trims internal library frames from test failure stack traces, per constitution §5.3.

**Decision**: Apply `[StackTraceHidden]` to both new methods in both types.

---

## Research Item 6: XML documentation format

**Question**: What XML doc pattern should the new methods follow?

**Finding** (from existing methods, e.g., `ContainKey`):

```xml
/// <summary>
/// Asserts that the dictionary contains the specified key.
/// </summary>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
```

**Decision**: Follow the same pattern. Exact doc text for the new methods:

For `NotBeNull()`:
> Asserts that the subject is not `null`.

For `BeNull()`:
> Asserts that the subject is `null`.

---

## Summary of Decisions

| Decision | Chosen Approach | Rationale |
|----------|----------------|-----------|
| Signature shape | `(string because = "", params object[] becauseArgs)` → `AndConstraint<T>` | Exact mirror of `ObjectAssertions<T>`; zero surprise |
| Failure message (NotBeNull) | `expected="not <null>", actual="<null>"` | Matches `ObjectAssertions<T>` exactly |
| Failure message (BeNull) | `expected="<null>", actual=FormatValue(Subject)` | Matches `ObjectAssertions<T>` exactly |
| Inheritance / restructuring | None | `readonly struct` cannot inherit; additive-only per constitution |
| Allocation | Zero on passing path | `AndConstraint<T>` is a struct; constitution §6.2 |
| `[StackTraceHidden]` | Applied to both new methods | Matches all other assertion methods; constitution §5.3 |
| XML docs | Full `<summary>`, `<param>`, `<returns>` | Constitution §3.2 |
