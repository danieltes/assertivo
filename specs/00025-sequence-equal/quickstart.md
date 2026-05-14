# Quickstart: Ordered Sequence Equality Assertion

**Feature**: `00025-sequence-equal`  
**Date**: 2026-05-13

## Overview

`Equal` asserts that two sequences contain the same elements in the same order. It is the ordered counterpart to `BeEquivalentTo` (which ignores order).

---

## Basic Usage

```csharp
// Passes — same elements, same order
new[] { 1, 2, 3 }.Should().Equal(new[] { 1, 2, 3 });

// Fails — same elements, wrong order
new[] { 3, 1, 2 }.Should().Equal(new[] { 1, 2, 3 });
// → Expected 1 at index 0 but found 3.

// Fails — different counts
new[] { 1, 2 }.Should().Equal(new[] { 1, 2, 3 });
// → Expected collection with 3 element(s) but found 2 element(s).
```

---

## Inline Values (params overload)

```csharp
var result = GetSortedIds();
result.Should().Equal(10, 20, 30);
```

Use the `IEnumerable<T>` overload when you also need `because` or a custom comparer.

---

## With a Reason

```csharp
var output = pipeline.Process(input);
output.Should().Equal(expectedOutput, because: "pipeline output must be deterministic");
// → Expected "bar" at index 1 but found "baz".
//   Because: pipeline output must be deterministic
```

---

## With a Custom Comparer

```csharp
var tags = GetTags();
var expected = new[] { "Alpha", "Beta", "Gamma" };

// Case-insensitive element comparison
tags.Should().Equal(expected, comparer: StringComparer.OrdinalIgnoreCase);
```

---

## Chaining

```csharp
var rows = ReadCsvRows();
rows.Should()
    .Equal("header", "row1", "row2")
    .And.HaveCount(3);
```

---

## `Equal` vs `BeEquivalentTo`

| Assertion | Order-sensitive | Frequency-aware |
|-----------|:--------------:|:---------------:|
| `Equal` | Yes | Yes |
| `BeEquivalentTo` | No | Yes |

```csharp
// These two are DIFFERENT:
new[] { 2, 1 }.Should().BeEquivalentTo(new[] { 1, 2 }); // passes (order ignored)
new[] { 2, 1 }.Should().Equal(new[] { 1, 2 });           // fails (order matters)
```

---

## Null and Edge Cases

```csharp
// Both empty — passes
Array.Empty<int>().Should().Equal(Array.Empty<int>());

// Null subject — fails with standard null guard message
IEnumerable<int>? subject = null;
subject.Should().Equal(new[] { 1 });
// → Expected a collection but found <null>.

// Null element rendered as <null> in failure message
new string?[] { null, "x" }.Should().Equal(new string?[] { "y", "x" });
// → Expected "y" at index 0 but found <null>.
```

> **Note**: Passing `null` as the `expected` argument throws `ArgumentNullException` immediately — it is a programming error, not an assertion condition.
