# Research: NotBeSameAs — Object Reference Inequality Assertion

**Date**: 2026-06-08
**Branch**: `00031-not-be-same-as`

## Summary

No external research required. The implementation pattern is fully established by the existing `BeSameAs` method in `ObjectAssertions.cs`. All decisions below are derived from reading the codebase directly.

---

## Decision 1: Reference equality mechanism

**Decision**: Use `ReferenceEquals(Subject, unexpected)`
**Rationale**: `ReferenceEquals` is the canonical .NET BCL method for object identity comparison. It handles all cases correctly including `null`/`null` (returns `true`), `null`/non-null (returns `false`), and boxed value types (two separately boxed values return `false` even if the underlying value is equal). It cannot be overridden, making it immune to user-defined equality semantics.
**Alternatives considered**:
- `object.Equals(Subject, unexpected)` — rejected; delegates to virtual `Equals`, which can be overridden to return `true` for logically equal but distinct instances
- `(object)Subject == (object?)unexpected` — rejected; uses overloaded `==` operator which can differ from reference identity for some types

---

## Decision 2: Failure message content

**Decision**:
```csharp
MessageFormatter.Fail(
    "not the same reference",
    "same reference",
    Expression, because, becauseArgs);
```
**Rationale**: Mirrors the pattern used in `BeSameAs` (`"same reference"` / `"different reference"`) and `NotBe` (`"not {value}"` / `"{value}"`). The `expected` slot describes what was desired ("not the same reference"); the `actual` slot describes what was found ("same reference"). This is consistent with the established `MessageFormatter.Fail(expected, actual, ...)` convention across the entire library.
**Alternatives considered**:
- Including object hash codes or `ToString()` output — rejected; reference identity failures are not about value representation; adding potentially long strings adds noise without diagnostic value
- `"different reference"` / `"same reference"` (swapping `expected`) — rejected; the `expected` parameter in `MessageFormatter.Fail` should describe the desired condition

---

## Decision 3: Value-type guard message

**Decision**: `$"NotBeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality."`
**Rationale**: Directly specified in the feature description. Mirrors the guard message in `BeSameAs` (only the method name differs). Provides the correct alternative (`Be()`) inline with the error.
**Alternatives considered**: None — exact message is prescribed by spec FR-004.

---

## Decision 4: Guard ordering

**Decision**: Check `typeof(T).IsValueType` before `ReferenceEquals`
**Rationale**: Consistent with `BeSameAs`. Value types are always boxed when passed as `object?`, so `ReferenceEquals` would never return `true` for two separately-boxed value-type subjects — a guard-free `NotBeSameAs(42)` on an `int` subject would pass silently and incorrectly. The guard makes misuse immediately visible rather than producing a green test that tests nothing.
**Alternatives considered**: None — guard-first is the established library pattern.

---

## Decision 5: No comparer parameter

**Decision**: No `IEqualityComparer<T>` overload
**Rationale**: Reference identity is not configurable — `ReferenceEquals` by definition cannot be substituted. Adding a comparer parameter would imply value-equality semantics, which is already covered by `Be()` / `NotBe()`. Spec explicitly states "no new comparer parameter is needed."
**Alternatives considered**: None — excluded by spec.

---

## Decision 6: Return type

**Decision**: `AndConstraint<ObjectAssertions<T>>`
**Rationale**: Standard return type for all terminal assertion methods in the library. Enables `.And` chaining: `a.Should().NotBeSameAs(b).And.NotBeNull()`.
**Alternatives considered**: `void` — rejected; breaks fluent chaining, violating constitution §VII.7.

---

## Existing infrastructure confirmed available

| Piece | Location | Used for |
|-------|----------|---------|
| `MessageFormatter.Fail(expected, actual, expr, because, becauseArgs)` | `src/Assertivo/MessageFormatter.cs` | Failure reporting and `because` formatting |
| `AndConstraint<TAssertions>` | `src/Assertivo/Primitives/AndConstraint.cs` | Return type for chaining |
| `[StackTraceHidden]` attribute | All assertion methods | Trims library frames from test stack traces |
| `typeof(T).IsValueType` | `ObjectAssertions.cs` — `BeSameAs` | Value-type guard |
| xUnit `Assert.Throws<T>` | `ObjectAssertionsTests.cs` | Verifying exception throwing |
| `AssertionFailedException.Expected`, `.Actual`, `.Reason`, `.Message` | `AssertionFailedException.cs` | Asserting failure message structure in tests |
