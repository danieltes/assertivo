# Research: Float/Double/Decimal Should Dispatch

**Feature**: `00049-float-double-decimal-should`  
**Date**: 2026-06-10

## Decision 1: NumericAssertions<T> Constraint Compatibility

**Decision**: No changes to `NumericAssertions<T>` are needed.

**Rationale**: The class is declared as `public readonly struct NumericAssertions<T> where T : struct, IComparable<T>, IEquatable<T>`. All three target types satisfy this constraint:
- `System.Single` (`float`): implements `IComparable<float>`, `IEquatable<float>`
- `System.Double` (`double`): implements `IComparable<double>`, `IEquatable<double>`
- `System.Decimal` (`decimal`): implements `IComparable<decimal>`, `IEquatable<decimal>`

**Alternatives considered**: Separate assertion classes per type (e.g., `FloatAssertions`). Rejected — the generic approach is already in place and correct. Per-type classes would duplicate code without benefit.

---

## Decision 2: Placement of New Overloads

**Decision**: Add the three new overloads in `src/Assertivo/Should.cs`, immediately after the existing `long` overload and before the `string?` overload. No new files needed.

**Rationale**: All `Should()` extension methods are co-located in `Should.cs`. Placing the new overloads adjacent to the existing numeric overloads keeps related dispatch logic grouped and matches the established convention.

**Alternatives considered**: A separate file for floating-point overloads (e.g., `ShouldFloatingPoint.cs`). Rejected — the project uses a single `Should.cs` file for all dispatch overloads; splitting for 3 overloads adds fragmentation without benefit.

---

## Decision 3: CallerArgumentExpression Parameter

**Decision**: Include `[CallerArgumentExpression(nameof(subject))] string? caller = null` on each new overload, identical to the `int` and `long` overloads.

**Rationale**: Failure messages must include the caller expression (e.g., `"result"` when the calling code is `result.Should().BeLessThan(0.0)`). This is required by FR-008 and constitution §III.4. The attribute requires C# 10+ which the project already uses.

**Alternatives considered**: Removing the parameter for simplicity. Rejected — `int` and `long` overloads already use it; omitting it from the new overloads would produce inconsistent failure messages.

---

## Decision 4: Test Location and Naming

**Decision**: New tests go in `tests/Assertivo.Tests/NumericAssertionsTests.cs`, following the existing test class and method naming conventions.

**Rationale**: `NumericAssertionsTests.cs` already covers `int` and `long` with the `MethodName_Scenario_ExpectedOutcome` pattern (e.g., `Be_WhenValuesAreEqual_Passes`). Adding `float`, `double`, and `decimal` tests to the same file maintains the single-class-per-assertion-class convention.

**Additional**: `ShouldDispatchTests.cs` should receive dispatch-type verification tests confirming that `.Should()` on each new type returns `NumericAssertions<T>`, not `ObjectAssertions<T>`.

**Alternatives considered**: Separate test files per numeric type. Rejected — would fragment coverage for a single assertion class; the project groups by assertion class, not by subject type.

---

## Decision 5: Allocation Profile

**Decision**: The new overloads are zero-allocation on the happy path, matching the existing `int`/`long` overloads.

**Rationale**: `new NumericAssertions<T>(subject, caller)` constructs a `readonly struct` which lives on the stack. No heap allocation occurs. This satisfies constitution §VI.2.

**Verification**: No BenchmarkDotNet changes required for this patch (adding overloads does not change the allocation profile of existing benchmarks). Benchmarks for floating-point numeric assertions could be added in a follow-up.
