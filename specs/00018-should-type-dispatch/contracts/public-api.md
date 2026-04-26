# Public API Contract: Should() Extension — Type-Aware Dispatch

**Feature**: 00018-should-type-dispatch  
**Date**: 2026-04-25  
**Namespace**: `Assertivo`  
**File**: `src/Assertivo/Should.cs` — class `ShouldExtensions`

---

## New Overloads (Additive — No Breaking Changes)

### 1. `IReadOnlyList<T>.Should()`

```csharp
/// <summary>
/// Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified
/// read-only list subject.
/// </summary>
public static GenericCollectionAssertions<T> Should<T>(
    this IReadOnlyList<T>? subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
    => new(subject, caller);
```

| Property | Value |
|----------|-------|
| Return type | `GenericCollectionAssertions<T>` |
| Null subject | Accepted; forwarded |
| Breaking change | None |
| Overload priority | Beats `IEnumerable<T>` for `IReadOnlyList<T>` subjects |
| Ambiguity with `IReadOnlyCollection<T>` | A type implementing both raises a compiler ambiguity error; no tie-break rule is defined (Spec §Edge Cases) |

---

### 2. `IReadOnlyCollection<T>.Should()`

```csharp
/// <summary>
/// Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified
/// read-only collection subject.
/// </summary>
public static GenericCollectionAssertions<T> Should<T>(
    this IReadOnlyCollection<T>? subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
    => new(subject, caller);
```

| Property | Value |
|----------|-------|
| Return type | `GenericCollectionAssertions<T>` |
| Null subject | Accepted; forwarded |
| Breaking change | None |
| Overload priority | Beats `IEnumerable<T>` for `IReadOnlyCollection<T>` subjects |
| Ambiguity with `IReadOnlyList<T>` | A type implementing both raises a compiler ambiguity error; no tie-break rule is defined (Spec §Edge Cases) |

---

### 3. `Func<T>.Should()`

```csharp
/// <summary>
/// Returns an <see cref="ActionAssertions"/> for the specified function subject,
/// adapting it to an <see cref="Action"/> by discarding the return value.
/// </summary>
/// <exception cref="ArgumentNullException">
/// Thrown immediately if <paramref name="subject"/> is <see langword="null"/>.
/// </exception>
public static ActionAssertions Should<T>(
    this Func<T> subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
{
    ArgumentNullException.ThrowIfNull(subject);
    return new ActionAssertions(() => subject(), caller);
}
```

| Property | Value |
|----------|-------|
| Return type | `ActionAssertions` |
| Null subject | Rejected — `ArgumentNullException` thrown at call site |
| Allocation | One `Action` delegate per call |
| Breaking change | Source-incompatible — callers previously invoking `ObjectAssertions`-only methods on `Func<T>` subjects will see compile errors (intentional; see Stability Guarantees) |
| Overload priority | Below `Func<Task>`, above unconstrained `T` fallback |

> **Allocation note**: The one-delegate-per-call budget is a design constraint documented here. Benchmark enforcement is out of scope for this feature; compliance is verified by code review (the `() => subject()` lambda captures exactly one reference).

---

## Unchanged Overloads (Listed for Completeness)

The following overloads are not modified by this feature:

| Subject type | Return type | Notes |
|--------------|-------------|-------|
| `bool` | `BooleanAssertions` | |
| `int` | `NumericAssertions<int>` | |
| `long` | `NumericAssertions<long>` | |
| `string?` | `StringAssertions` | |
| `Action` | `ActionAssertions` | |
| `Func<Task>` | `AsyncFunctionAssertions` | |
| `Func<T1,…,TResult>` (multi-param) | `ObjectAssertions<T>` | Intentionally unchanged — out of scope per Spec §FR-003 |
| `T[]` | `GenericCollectionAssertions<T>` | |
| `List<T>` | `GenericCollectionAssertions<T>` | |
| `IEnumerable<T>` | `GenericCollectionAssertions<T>` | |
| `IEnumerable<KeyValuePair<K,V>>` | `GenericDictionaryAssertions<K,V>` | |
| `IDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | |
| `IReadOnlyDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | ⚠️ Pre-satisfied — overload exists in `Should.cs`; **no code change** required for this feature |
| `T` (fallback) | `ObjectAssertions<T>` | |

---

## Stability Guarantees

- All three new overloads are **additive** — no existing overload is removed or
  modified.
- The `Func<T>` overload changes the resolved type from `ObjectAssertions<Func<T>>`
  to `ActionAssertions`. Any code that was previously calling `ObjectAssertions`
  methods on a `Func<T>` subject (e.g., `func.Should().BeNull()`) will now produce
  a **compile error** because `ActionAssertions` does not expose `BeNull()`. This
  is a **source-incompatible change** for that specific misuse pattern, but is
  intentional (the correct type is now used; null guarding makes `BeNull()` on
  `Func<T>` impossible by construction).
- The `IReadOnlyList<T>` and `IReadOnlyCollection<T>` overloads similarly change
  the resolved type from `ObjectAssertions<IReadOnlyList<T>>` to
  `GenericCollectionAssertions<T>`. Any code calling `ObjectAssertions`-only methods
  on those types will produce a compile error. This is intentional.
