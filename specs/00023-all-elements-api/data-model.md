# Data Model: First-Class All-Elements Assertions

**Feature**: 00023-all-elements-api  
**Date**: 2026-05-08

---

## Overview

This feature adds no persistent storage entities. The effective data model is runtime assertion-evaluation state used to aggregate and render all-elements failures.

---

## Entities

### 1. ElementInspectionContext<T>

Represents one inspection invocation during enumeration.

| Field | Type | Description |
|-------|------|-------------|
| `Index` | `int` | Zero-based element position in enumeration order. |
| `Element` | `T` | Current element passed to inspector callback. |
| `InspectorShape` | `enum` | `ElementOnly` or `ElementWithIndex`. |

Validation rules:
- `Index >= 0`

---

### 2. ElementFailureRecord

Represents one element-level failure captured from inspector execution.

| Field | Type | Description |
|-------|------|-------------|
| `Index` | `int` | Element index where failure occurred. |
| `ExceptionType` | `string` | Runtime exception type name. |
| `Message` | `string` | Captured exception message. |
| `IsAssertionFailure` | `bool` | True when captured exception is assertion-related. |

Validation rules:
- `Index >= 0`
- Records are stored in ascending index order.

---

### 3. AllElementsFailureSummary

Represents the aggregate result used to build final failure output.

| Field | Type | Description |
|-------|------|-------------|
| `FailedCount` | `int` | Total number of failing elements. |
| `FailingIndices` | `IReadOnlyList<int>` | Complete set of failing indices in ascending order. |
| `DetailedFailures` | `IReadOnlyList<ElementFailureRecord>` | First 50 failures with full details. |
| `DetailsTruncated` | `bool` | True when `FailedCount > 50`. |
| `IndexFormat` | `enum` | `ExplicitList` for <=100 indices, `RangeCompressed` for >100. |

Validation rules:
- `FailedCount == FailingIndices.Count`
- `DetailedFailures.Count == min(50, FailedCount)`
- `FailingIndices` contains all failures, no loss under compression mode.

---

### 4. IndexRangeSegment

Represents one compressed contiguous index segment for large failure sets.

| Field | Type | Description |
|-------|------|-------------|
| `Start` | `int` | Inclusive range start. |
| `End` | `int` | Inclusive range end. |

Validation rules:
- `Start >= 0`
- `End >= Start`
- Segments are sorted and non-overlapping.

---

## State Transitions

```text
Start
  -> EnumeratingElements
    -> CompletedNoFailures
         -> Return AndConstraint<GenericCollectionAssertions<T>>
    -> CompletedWithInspectorFailures
         -> Build AllElementsFailureSummary
         -> Throw AssertionFailedException via standard pipeline
    -> EnumerationFault
         -> Rethrow original enumeration exception immediately
```

Notes:
- Inspector exceptions are collectable failures.
- Enumeration exceptions are terminal source faults, not aggregate assertion failures.
