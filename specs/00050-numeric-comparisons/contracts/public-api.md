# Public API Contract: NumericAssertions<T> Comparison Methods

**Feature**: 00050-numeric-comparisons  
**Type**: `Assertivo.Numeric.NumericAssertions<T>`  
**File**: `src/Assertivo/Numeric/NumericAssertions.cs`

## New Methods

### BeGreaterThan

```csharp
/// <summary>
/// Asserts that the subject is greater than <paramref name="value"/>.
/// </summary>
[StackTraceHidden]
public AndConstraint<NumericAssertions<T>> BeGreaterThan(
    T value,
    IComparer<T>? comparer = null,
    string because = "",
    params object[] becauseArgs)
```

**Semantics**: Passes when `comparer.Compare(Subject, value) > 0`. Fails when subject is equal to or less than value.

**Failure message format**: `"a value greater than {value}"`

**On null comparer**: Falls back to `Comparer<T>.Default`.

---

### BeLessThanOrEqualTo

```csharp
/// <summary>
/// Asserts that the subject is less than or equal to <paramref name="value"/>.
/// </summary>
[StackTraceHidden]
public AndConstraint<NumericAssertions<T>> BeLessThanOrEqualTo(
    T value,
    IComparer<T>? comparer = null,
    string because = "",
    params object[] becauseArgs)
```

**Semantics**: Passes when `comparer.Compare(Subject, value) <= 0`. Fails when subject is strictly greater than value.

**Failure message format**: `"a value less than or equal to {value}"`

**On null comparer**: Falls back to `Comparer<T>.Default`.

---

## Complete Comparison Method Set (post-feature)

| Method | Condition | Fails when |
|---|---|---|
| `BeGreaterThan` (new) | `subject > value` | subject ≤ value |
| `BeGreaterThanOrEqualTo` | `subject >= value` | subject < value |
| `BeLessThan` | `subject < value` | subject ≥ value |
| `BeLessThanOrEqualTo` (new) | `subject <= value` | subject > value |

## Type Constraint

`T` is constrained at the class level: `where T : struct, IComparable<T>, IEquatable<T>`. No additional constraints are needed for these methods.

## Chain Compatibility

Both methods return `AndConstraint<NumericAssertions<T>>`, enabling chains such as:

```csharp
result.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(100);
```
