# Data Model: Ordered Sequence Equality Assertion

**Feature**: `00025-sequence-equal`  
**Phase**: 1 — Design  
**Date**: 2026-05-13

## Overview

This feature introduces no new types. `Equal` is a pair of methods added to the existing `GenericCollectionAssertions<T>` readonly struct. All types referenced below already exist in the codebase.

---

## Existing Types Used

### `GenericCollectionAssertions<T>` *(modified — receives new methods)*

**Namespace**: `Assertivo.Collections`  
**Kind**: `readonly partial struct`  
**Note**: The `partial` modifier is added to the existing declaration to support file splitting (see research D-001).

| Member | Kind | Description |
|--------|------|-------------|
| `Subject` | property (`IEnumerable<T>?`) | The collection under test. Accessed by `Equal` after `GuardNull()`. |
| `Expression` | property (`string?`) | Caller expression string captured by compiler attributes; threaded through `MessageFormatter.Fail`. |
| `GuardNull()` | private method | Fails the assertion pipeline if `Subject` is null. Reused as-is by `Equal`. |
| `Equal(IEnumerable<T>, IEqualityComparer<T>?, string, object[])` | **new public method** | Primary ordered-equality assertion (see Contracts). |
| `Equal(params T[])` | **new public method** | Convenience overload; delegates to primary via `(IEnumerable<T>)` cast. |

---

### `AndConstraint<TAssertions>` *(unchanged — return type)*

**Namespace**: `Assertivo.Primitives`  
**Kind**: `readonly struct`  
**Role**: Wraps the assertion context returned by both `Equal` overloads to enable fluent `.And.*` chaining.

---

### `MessageFormatter` *(unchanged — used for failure reporting)*

**Namespace**: `Assertivo`  
**Kind**: `internal static class`  
**Members used by `Equal`**:

| Member | Signature | Usage |
|--------|-----------|-------|
| `Fail` | `(string expected, string actual, string? expression, string because, object[] becauseArgs)` | Called for count mismatch and element mismatch failures. |
| `FormatValue` | `(object? value) → string` | Renders element values in element-mismatch messages; returns `"<null>"` for null, quoted string for `string`, `ToString()` for others. |

---

## Algorithms

### `Equal` — primary overload

```
Input: expected (IEnumerable<T>), comparer (nullable), because, becauseArgs
Pre-conditions:
  - ArgumentNullException if expected is null  (before any enumeration)
  - GuardNull() fails pipeline if Subject is null

Steps:
  1. comparer ??= EqualityComparer<T>.Default
  2. actualList  = Subject.ToList()
  3. expectedList = expected.ToList()
  4. If actualList.Count ≠ expectedList.Count →
       Fail("collection with {expectedList.Count} element(s)",
            "{actualList.Count} element(s)", ...)
  5. For i = 0 to actualList.Count - 1:
       If NOT comparer.Equals(actualList[i], expectedList[i]) →
         Fail("{FormatValue(expectedList[i])} at index {i}",
              "{FormatValue(actualList[i])}", ...)
         (loop does not continue after first mismatch — Fail does not return)
  6. Return new AndConstraint<GenericCollectionAssertions<T>>(this)
```

**Complexity**: O(n) time, O(n) space (two lists). Cyclomatic complexity = 4 (null-guard branch, count-mismatch branch, loop, element-mismatch branch). ≤ 10 constitution limit.

### `Equal` — params convenience overload

```
Equal(params T[] expected)
  → Equal((IEnumerable<T>)expected)
```

No independent logic. The explicit cast prevents infinite self-call (see research D-004).

---

## State Transitions

`Equal` is a pure assertion method with no side effects on the subject. It either:
1. Returns `AndConstraint<GenericCollectionAssertions<T>>(this)` — assertion passed.
2. Throws (via `MessageFormatter.Fail` → `AssertionConfiguration.ReportFailure`) — assertion failed.

No mutable state is introduced.
