# Quickstart: First-Class All-Elements Assertions

**Feature**: 00023-all-elements-api  
**Date**: 2026-05-08

This guide shows how to use `AllSatisfy` as a first-class all-elements assertion API with assertion bodies and index-aware checks.

---

## 1. Basic assertion-body usage

```csharp
using Assertivo;

IReadOnlyList<int> values = [2, 4, 6, 8];

values.Should().AllSatisfy(v =>
{
    v.Should().BeGreaterThanOrEqualTo(2);
    (v % 2).Should().Be(0);
});
```

Use this when each element needs one or more fluent assertions, not just a boolean predicate.

---

## 2. Index-aware overload

```csharp
using Assertivo;

var values = new[] { 10, 20, 30 };

values.Should().AllSatisfy((v, i) =>
{
    // Position-sensitive check
    v.Should().Be((i + 1) * 10);
});
```

`i` is zero-based and follows enumeration order.

---

## 3. Bounded, deterministic diagnostics

When failures occur:
- A single aggregated assertion failure is surfaced through the active framework adapter exception type (or `AssertionFailedException` fallback).
- Detailed entries are in ascending index order.
- Full details are included for first 50 failing elements.
- Total failed count is always included.
- All failing indices are always represented.

Example shape:

```text
Expected all elements to satisfy the inspector but found 165 element(s) failed.
Expression: values
Because: every score must be valid
Details: [0]: ...; [2]: ...; ... (showing first 50 failure(s))
Failing indices: [0-120, 123, 125-167]
```

---

## 4. Index rendering rules

- If failing index count <= 100: explicit ordered list
- If failing index count > 100: range-compressed ordered list
- Range-compressed grammar: comma-separated ascending segments where each segment is either `N` (singleton) or `Start-End` (inclusive contiguous range)
- Singleton values are rendered as `N`, not `N-N`

Examples:

```text
[1, 4, 7, 9]
[0-12, 18, 21-30, 44]
```

---

## 5. Exception-source behavior

Inspector exceptions are aggregated by element index.

```csharp
values.Should().AllSatisfy(v =>
{
    if (v < 0)
    {
        throw new InvalidOperationException("negative values are not allowed");
    }
});
```

If the source enumerable itself throws during iteration, `AllSatisfy` rethrows that original exception immediately.

---

## 6. Dispatch compatibility examples

All collection entry points that resolve to `GenericCollectionAssertions<T>` support this API:

```csharp
IEnumerable<int> a = [1, 2, 3];
IReadOnlyList<int> b = [1, 2, 3];
IReadOnlyCollection<int> c = [1, 2, 3];
List<int> d = [1, 2, 3];
int[] e = [1, 2, 3];

a.Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(1));
b.Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(1));
c.Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(1));
d.Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(1));
e.Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(1));
```
