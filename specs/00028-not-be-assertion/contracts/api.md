# Public API Contract: NotBe Inequality Assertion

**Feature**: 00028-not-be-assertion  
**Phase**: 1 — Design  
**Date**: 2026-05-31

This document defines the exact public method signatures introduced by this feature. These are the authoritative contracts against which tests and implementation are validated.

---

## `ObjectAssertions<T>.NotBe`

```csharp
namespace Assertivo;

public readonly struct ObjectAssertions<T>
{
    /// <summary>
    /// Asserts that the subject does not equal the <paramref name="unexpected"/> value.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<ObjectAssertions<T>> NotBe(
        T unexpected,
        IEqualityComparer<T>? comparer = null,
        string because = "",
        params object[] becauseArgs);
}
```

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `unexpected` | `T` | required | The value the subject must NOT equal |
| `comparer` | `IEqualityComparer<T>?` | `null` → `EqualityComparer<T>.Default` | Optional equality rule override |
| `because` | `string` | `""` | Human-readable reason appended to failure message when non-empty |
| `becauseArgs` | `object[]` | `[]` | Format arguments for `because` (via `string.Format`) |

**Returns**: `AndConstraint<ObjectAssertions<T>>` — the same assertion object wrapped for fluent chaining via `.And`.

**Throws**: `AssertionFailedException` (via `AssertionConfiguration.ReportFailure`) when `comparer.Equals(Subject, unexpected)` returns `true`.

**Failure message shape**:
```
Expected not {unexpected} but found {actual}.
[Expression: {expression}]
[Because: {reason}]
```

---

## `NumericAssertions<T>.NotBe`

```csharp
namespace Assertivo.Numeric;

public readonly struct NumericAssertions<T> where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Asserts that the subject does not equal the <paramref name="unexpected"/> value.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<NumericAssertions<T>> NotBe(
        T unexpected,
        IEqualityComparer<T>? comparer = null,
        string because = "",
        params object[] becauseArgs);
}
```

**Parameters**: Same semantics as `ObjectAssertions<T>.NotBe`. `T` is constrained to non-nullable value types — `unexpected` is never null.

**Returns**: `AndConstraint<NumericAssertions<T>>`.

**Throws**: `AssertionFailedException` when `comparer.Equals(Subject, unexpected)` returns `true`.

**Failure message shape**: Same template as `ObjectAssertions<T>.NotBe`.

---

## `StringAssertions.NotBe`

```csharp
namespace Assertivo;

public readonly struct StringAssertions
{
    /// <summary>
    /// Asserts that the subject does not equal the <paramref name="unexpected"/> string.
    /// Uses ordinal (case-sensitive) comparison.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<StringAssertions> NotBe(
        string? unexpected,
        string because = "",
        params object[] becauseArgs);
}
```

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `unexpected` | `string?` | required | The string value the subject must NOT equal (may be `null`) |
| `because` | `string` | `""` | Reason appended to failure message when non-empty |
| `becauseArgs` | `object[]` | `[]` | Format arguments for `because` |

**Comparison**: `string.Equals(Subject, unexpected, StringComparison.Ordinal)` — case-sensitive, culture-insensitive. No `StringComparison` parameter; case-insensitive variants are deferred.

**Returns**: `AndConstraint<StringAssertions>`.

**Throws**: `AssertionFailedException` when `string.Equals(Subject, unexpected, Ordinal)` returns `true`.

**Failure message shape**: Same template as above.

---

## Entry Points

These methods are reached via the existing `.Should()` extension methods on `Should.cs`:

| Subject type | Extension method | Returns |
|---|---|---|
| `int` | `42.Should()` | `NumericAssertions<int>` |
| `long` | `100L.Should()` | `NumericAssertions<long>` |
| `string` | `"hello".Should()` | `StringAssertions` |
| `T` (any) | `value.Should<T>()` | `ObjectAssertions<T>` |

No changes to `Should.cs` are required.
