# Research: NotBe Inequality Assertion

**Feature**: 00028-not-be-assertion  
**Phase**: 0 — Pre-design research  
**Date**: 2026-05-31

All decisions below resolve `NEEDS CLARIFICATION` items identified during Technical Context analysis. Each is backed by direct code inspection or BCL documentation.

---

## Decision 1: `NumericAssertions<T>.Be` comparer signature — confirmed

**Decision**: `NumericAssertions<T>.NotBe` WILL accept `IEqualityComparer<T>? comparer = null`, matching the same parameter on `Be`.

**Rationale**: `NumericAssertions<T>.Be` (line 27, `src/Assertivo/Numeric/NumericAssertions.cs`) already has the signature:

```csharp
public AndConstraint<NumericAssertions<T>> Be(
    T expected,
    IEqualityComparer<T>? comparer = null,
    string because = "",
    params object[] becauseArgs)
```

Adding the same optional comparer to `NotBe` is symmetric and consistent. Omitting it would break symmetry with both `Be` and `ObjectAssertions<T>.NotBe`.

**Alternatives considered**: Omit comparer on `NumericAssertions<T>.NotBe` because numeric equality rarely needs custom comparers. Rejected — constitution principle VII.8 ("Custom comparers MUST be injectable at the assertion call site") and existing `Be` shape take precedence.

---

## Decision 2: Failure message format — `not {formatted}` prefix

**Decision**: The `expected` parameter passed to `MessageFormatter.Fail` will be `$"not {MessageFormatter.FormatValue(unexpected)}"`, producing messages of the form:

```
Expected not "hello" but found "hello".
Expression: name
```

For integers: `Expected not 42 but found 42.`  
For null subjects/unexpecteds: `Expected not <null> but found <null>.`

**Rationale**: `MessageFormatter.BuildMessage` produces `Expected {expected} but found {actual}.` — the `expected` argument is a free-form string. The existing `NotContain` method uses a similar descriptive prefix pattern (`$"a string not containing {value}"`). The feature proposal explicitly shows the `not "hello"` prefix. The `FormatValue` helper already handles `null → <null>`, `string → "quoted"`, and numeric → `ToString()`.

**Alternatives considered**: "not equal to {value}" as prefix. Rejected — the proposal shows the shorter `not {value}` form, consistent with how assertion libraries conventionally express negation.

---

## Decision 3: `StringAssertions.NotBe` null semantics — ordinal, no special null path

**Decision**: `StringAssertions.NotBe` will use `string.Equals(Subject, unexpected, StringComparison.Ordinal)` for the equality check, identical to `Be`. No separate null-guarding branch is needed.

**Rationale**: `string.Equals(null, null, StringComparison.Ordinal)` returns `true` (documented BCL behaviour). `string.Equals(null, "x", StringComparison.Ordinal)` and `string.Equals("x", null, StringComparison.Ordinal)` both return `false`. This means all null combinations are handled correctly by the single `string.Equals` call — the assertion fails when both are null (correctly, since null == null) and passes in all mixed-null cases (correctly, since null ≠ non-null). No special-casing needed.

**Alternatives considered**: Use `Subject is null && unexpected is null` fast path for clarity. Rejected — the single `string.Equals` path is more concise and already the pattern used by `Be` in the same file.

---

## Decision 4: `EqualityComparer<T>.Default` and AOT-safety

**Decision**: `EqualityComparer<T>.Default` is safe to use in AOT-compiled scenarios for both `ObjectAssertions<T>` and `NumericAssertions<T>`.

**Rationale**: The .NET 10 runtime trimmer preserves `EqualityComparer<T>.Default` implementations for all value types referenced in code, and for reference types the default comparer falls back to `object.Equals` / `GetHashCode` which is always available. No `[DynamicDependency]` annotation is needed. This is the same pattern already used by `Be` on both types — no regression is introduced.

**Alternatives considered**: Use `IEquatable<T>` directly. Rejected — `ObjectAssertions<T>` has no `IEquatable<T>` constraint and the comparer pattern is already established.

---

## Decision 5: `AndConstraint<T>` allocation on happy path — zero

**Decision**: `NotBe` has the same zero-allocation profile as `Be` on the passing path.

**Rationale**: `AndConstraint<T>` is declared as `readonly struct` in `src/Assertivo/Primitives/AndConstraint.cs`. Returning a struct by value allocates on the stack. `EqualityComparer<T>.Default` is a cached singleton — no allocation. `string.Equals` with `StringComparison.Ordinal` returns a `bool` — no allocation. The only allocation on the failure path is inside `MessageFormatter.Fail` (string interpolation, `List<string>`, `AssertionFailure` object) — this is expected and acceptable since a failure is an exceptional event.

**Alternatives considered**: N/A — this is purely a confirmation of the existing struct-based design.

---

## Decision 6: Test file placement — add to existing test files

**Decision**: `NotBe` tests will be added to the three existing test files (`ObjectAssertionsTests.cs`, `NumericAssertionsTests.cs`, `StringAssertionsTests.cs`) rather than in a new dedicated file.

**Rationale**: The existing pattern in the repository groups tests by the type under test (e.g., all `ObjectAssertions<T>` tests in `ObjectAssertionsTests.cs`). Creating a separate `NotBeAssertionsTests.cs` would scatter related tests across files and make discoverability harder. Constitution IV.2 ("Test class names MUST mirror the class under test") confirms the per-type grouping.

**Alternatives considered**: A single new `NotBeAssertionsTests.cs` file. Rejected — violates the per-type grouping convention established by existing test files.

---

## Summary of resolved unknowns

| ID | Topic | Resolution |
|----|-------|------------|
| R-1 | `NumericAssertions<T>` comparer | Confirmed — `Be` already has it; `NotBe` adds the same parameter |
| R-2 | Failure message prefix | `not {FormatValue(unexpected)}` → `Expected not 42 but found 42.` |
| R-3 | String null semantics | `string.Equals(..., Ordinal)` handles all null cases correctly |
| R-4 | AOT safety | `EqualityComparer<T>.Default` is AOT-safe; same pattern as `Be` |
| R-5 | Happy-path allocation | Zero — `AndConstraint<T>` is a `readonly struct` |
| R-6 | Test placement | Append to existing per-type test files |

All NEEDS CLARIFICATION items resolved. Phase 1 may proceed.
