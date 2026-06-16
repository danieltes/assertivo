# Research: Add BeGreaterThan and BeLessThanOrEqualTo to NumericAssertions

**Branch**: `00050-numeric-comparisons` | **Date**: 2026-06-15

## Summary

No external unknowns to resolve. All implementation decisions are fully determined by the existing code in `src/Assertivo/Numeric/NumericAssertions.cs`. This document records the pattern audit performed before Phase 1 design.

---

## Decision 1: Implementation Pattern

**Decision**: Mirror the exact pattern used by `BeGreaterThanOrEqualTo` and `BeLessThan`.

**Rationale**: Both existing methods share a uniform four-part structure — null-coalesce the comparer, invoke `comparer.Compare`, call `MessageFormatter.Fail` on violation, return `AndConstraint`. Deviating from this pattern for the two new methods would introduce inconsistency without benefit. The constitution's Zero Surprise and Readability First principles reinforce this choice.

Existing pattern (from `NumericAssertions.cs`):
```
[StackTraceHidden]
public AndConstraint<NumericAssertions<T>> <Method>(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
{
    comparer ??= Comparer<T>.Default;
    if (comparer.Compare(Subject, value) <CONDITION>)
    {
        MessageFormatter.Fail(
            $"a value <DESCRIPTION> {MessageFormatter.FormatValue(value)}",
            MessageFormatter.FormatValue(Subject),
            Expression, because, becauseArgs);
    }
    return new AndConstraint<NumericAssertions<T>>(this);
}
```

Comparison conditions for each method:

| Method | Fail condition | Message description |
|---|---|---|
| `BeGreaterThanOrEqualTo` | `< 0` (subject is less than value) | `"greater than or equal to"` |
| `BeLessThan` | `>= 0` (subject is equal or greater) | `"less than"` |
| **`BeGreaterThan`** (new) | `<= 0` (subject is equal or less) | `"greater than"` |
| **`BeLessThanOrEqualTo`** (new) | `> 0` (subject is strictly greater) | `"less than or equal to"` |

**Alternatives considered**: Using `IComparable<T>` directly (calling `Subject.CompareTo(value)`) rather than `IComparer<T>`. Rejected because the existing methods use `IComparer<T>` to support custom comparer injection, and the class constraint already satisfies `IComparable<T>` anyway.

---

## Decision 2: Generic Constraint

**Decision**: No new constraints needed on `NumericAssertions<T>`.

**Rationale**: The class already declares `where T : struct, IComparable<T>, IEquatable<T>`. `Comparer<T>.Default` is always resolvable for any type satisfying `IComparable<T>`. No arithmetic operators or additional numeric constraints (`INumber<T>`) are required because the implementation uses the comparer, not arithmetic.

**Alternatives considered**: Adding `where T : INumber<T>` to tighten the constraint. Rejected because it is unnecessary for comparison-only logic and would restrict the usable type set without benefit.

---

## Decision 3: Test Coverage Structure

**Decision**: Add tests to the existing `NumericAssertionsTests.cs` file, following the `MethodName_Scenario_ExpectedOutcome` naming convention and the Arrange-Act-Assert pattern already used throughout.

**Rationale**: All existing numeric tests live in one file. Adding a new class or file would split coverage for the same type across multiple locations. The file remains manageable in size with the additions.

Required test cases per method (per constitution §4.1 and spec SC-003):
- Passing case (subject satisfies condition)
- Boundary case (subject equals threshold — pass for `BeLessThanOrEqualTo`, fail for `BeGreaterThan`)
- Failing case (subject violates condition)
- Custom comparer case
- `because` phrase included in failure message

**Alternatives considered**: Separate test class per method. Rejected because the file is not large and the type is cohesive.
