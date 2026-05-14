# Public API Contract: Ordered Sequence Equality Assertion

**Feature**: `00025-sequence-equal`  
**Phase**: 1 — Design  
**Date**: 2026-05-13  
**Namespace**: `Assertivo.Collections`

---

## New Methods on `GenericCollectionAssertions<T>`

### Primary overload

```csharp
/// <summary>
/// Asserts that the collection contains the same elements in the same order as
/// <paramref name="expected"/>.
/// </summary>
/// <param name="expected">The expected sequence. Must not be <see langword="null"/>.</param>
/// <param name="comparer">
/// An optional equality comparer for elements. When <see langword="null"/>,
/// <see cref="EqualityComparer{T}.Default"/> is used.
/// </param>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="expected"/> is <see langword="null"/>.
/// </exception>
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> Equal(
    IEnumerable<T> expected,
    IEqualityComparer<T>? comparer = null,
    string because = "",
    params object[] becauseArgs);
```

### Convenience overload

```csharp
/// <summary>
/// Asserts that the collection contains exactly the provided elements in the given order.
/// </summary>
/// <param name="expected">The expected elements.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> Equal(params T[] expected);
```

---

## Failure Message Contracts

### Count mismatch

Produced when `actual.Count != expected.Count`.

```
Expected collection with {expectedCount} element(s) but found {actualCount} element(s).
[Expression: {expression}]
[Because: {reason}]
```

- `Expression` line is present only when a caller expression was captured.
- `Because` line is present only when a non-empty `because` argument is provided.
- No element-level comparison is performed when counts differ.

**Example**:
```
Expected collection with 3 element(s) but found 2 element(s).
Expression: items
```

### Element mismatch (first differing index)

Produced when counts match but `actual[i] != expected[i]` at the smallest `i`.

```
Expected {FormatValue(expected[i])} at index {i} but found {FormatValue(actual[i])}.
[Expression: {expression}]
[Because: {reason}]
```

**Value rendering**:
| Value | Rendered as |
|-------|-------------|
| `null` | `<null>` |
| `string` | `"value"` (double-quoted) |
| other | `ToString()` output, or `<null>` if `ToString()` returns null |

**Example — string mismatch**:
```
Expected "bar" at index 1 but found "baz".
```

**Example — null vs value**:
```
Expected <null> at index 0 but found "x".
```

**Example — with because**:
```
Expected 42 at index 2 but found 99.
Expression: pipeline
Because: pipeline output must be deterministic
```

---

## Behaviour Contract

| Scenario | Outcome |
|----------|---------|
| Same elements, same order | Passes; returns `AndConstraint` |
| Same elements, different order | Fails with element-mismatch message at first differing index |
| Different counts | Fails with count-mismatch message; no element comparison |
| Both empty | Passes |
| Null subject | Fails via `GuardNull()` standard null guard message |
| Null `expected` argument | Throws `ArgumentNullException` immediately |
| Null element in either sequence | Compared via comparer; rendered as `<null>` in failure message |
| Null comparer | Treated as `EqualityComparer<T>.Default` |
| Custom comparer | Element comparison delegates to `comparer.Equals(actual[i], expected[i])` |
| `because` supplied | `Because: {reason}` appended to failure message |

---

## Chaining

Both overloads return `AndConstraint<GenericCollectionAssertions<T>>`, enabling:

```csharp
collection.Should().Equal(1, 2, 3).And.HaveCount(3);
```

---

## Breaking Change Assessment

None. Both methods are additions to an existing type. No existing signatures are modified. Adding `partial` to the struct declaration is source-compatible and binary-compatible.
