# Public API Contract: Float/Double/Decimal Should Dispatch

**Feature**: `00049-float-double-decimal-should`  
**Date**: 2026-06-10  
**File**: `src/Assertivo/Should.cs`

## New Extension Methods

Three overloads added to the existing `ShouldExtensions` static class:

```csharp
/// <summary>
/// Returns a <see cref="NumericAssertions{T}"/> object for asserting on the <see cref="float"/> value.
/// </summary>
public static NumericAssertions<float> Should(
    this float subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
    => new(subject, caller);

/// <summary>
/// Returns a <see cref="NumericAssertions{T}"/> object for asserting on the <see cref="double"/> value.
/// </summary>
public static NumericAssertions<double> Should(
    this double subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
    => new(subject, caller);

/// <summary>
/// Returns a <see cref="NumericAssertions{T}"/> object for asserting on the <see cref="decimal"/> value.
/// </summary>
public static NumericAssertions<decimal> Should(
    this decimal subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
    => new(subject, caller);
```

## Inherited Assertion Methods

All methods already defined on `NumericAssertions<T>` are automatically available for `float`, `double`, and `decimal` with no additional code:

| Method | Signature |
|--------|-----------|
| `Be` | `AndConstraint<NumericAssertions<T>> Be(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` |
| `NotBe` | `AndConstraint<NumericAssertions<T>> NotBe(T unexpected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` |
| `BeGreaterThanOrEqualTo` | `AndConstraint<NumericAssertions<T>> BeGreaterThanOrEqualTo(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` |
| `BeLessThan` | `AndConstraint<NumericAssertions<T>> BeLessThan(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` |

## Failure Message Format

When an assertion fails, the message follows the established format:

```
Expected <condition description> but found <actual>.
Expression: <caller_expression>
Because: <because_reason>
```

**Example** — `(5.0).Should().BeLessThan(1.0)` fails with:

```
Expected a value less than 1.0 but found 5.0.
Expression: 5.0
```

## Breaking Changes

None. This is a purely additive change. Existing consumers using `float`, `double`, or `decimal` with `.Should()` were previously receiving `ObjectAssertions<T>` — a weaker type. The new overloads give them `NumericAssertions<T>`. Code that only used `Be`/`NotBe` on these types will continue to compile; code that used range methods will now compile where it previously failed.

## Out of Scope

- `float?`, `double?`, `decimal?` (nullable variants) — deferred
- `short`, `ushort`, `byte`, `sbyte`, `uint`, `ulong` — deferred
- NaN / infinity edge case behaviour — deferred
