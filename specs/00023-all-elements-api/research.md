# Research: First-Class All-Elements Assertions

**Feature**: 00023-all-elements-api  
**Date**: 2026-05-08  
**Status**: Complete - all clarifications resolved

---

## R-001: Canonical API Shape for All-Elements Assertions

### Decision
Use `AllSatisfy` as the canonical public API and add an index-aware overload:
- `AllSatisfy(Action<T> inspector, ...)`
- `AllSatisfy(Action<T, int> inspector, ...)`

No `OnlyContain(Func<T, bool>)` predicate-only API is introduced in this feature.

### Rationale
The spec explicitly centers on assertion bodies (multiple fluent assertions per element), not predicate-only semantics. Keeping one canonical name avoids discoverability fragmentation and aligns with the existing shipped method.

### Alternatives considered
- Add `OnlyContain(Func<T, bool>)` now: Rejected (explicitly out of scope).
- Rename to `OnlyContain` now: Rejected (creates migration risk and ambiguity).

---

## R-002: Aggregation and Ordering Semantics

### Decision
`AllSatisfy` aggregates inspector failures and reports them in original enumeration order (ascending index).

### Rationale
Deterministic ordering improves debuggability and test stability. Developers naturally map failures to the source collection by index order.

### Alternatives considered
- Error-type ordering: Rejected (harder to map to source collection).
- Unspecified ordering: Rejected (nondeterministic diagnostics).

---

## R-003: Failure Type Boundary (Inspector vs. Source Enumeration)

### Decision
Treat exception sources differently:
- Exceptions thrown by the inspector body are collected as element failures.
- Exceptions thrown by source enumeration are rethrown immediately and are not converted into aggregated assertion failures.

### Rationale
Enumeration exceptions represent source-data failure, not assertion mismatch. Rethrowing preserves root-cause fidelity and avoids masking runtime errors.

### Alternatives considered
- Convert enumeration exceptions into aggregate assertion failures: Rejected (masks source faults).
- Treat enumeration exceptions as synthetic element failures: Rejected (incorrect semantics).

---

## R-004: Final Thrown Type for Aggregated Assertion Failures

### Decision
When one or more element failures are collected, throw `AssertionFailedException` through the existing failure pipeline (`MessageFormatter` + `AssertionConfiguration`).

### Rationale
This preserves existing framework integration and consistent failure behavior across the library.

### Alternatives considered
- `AggregateException`: Rejected (inconsistent with assertion pipeline).
- New custom all-elements exception type: Rejected (adds API surface and compatibility burden).

---

## R-005: Diagnostic Detail Budget

### Decision
For aggregated failures:
- Include full per-failure details for the first 50 failing elements.
- Include total failed count.
- Include all failing indices.

### Rationale
This bounds message size while preserving complete failure coverage via indices.

### Alternatives considered
- Unbounded detail output: Rejected (large-message risk).
- Hard truncation without full index coverage: Rejected (loses traceability).

---

## R-006: Failing-Index Rendering Strategy

### Decision
Use adaptive rendering for all failing indices:
- Up to 100 indices: explicit ordered list.
- Above 100 indices: range-compressed ordered output.

Compression format uses inclusive ranges (for example: `[0-12, 18, 21-30]`).

### Rationale
Adaptive output keeps small failures immediately readable and large failures compact while preserving full index coverage.

### Alternatives considered
- Always explicit: Rejected (too verbose for large failures).
- Always compressed: Rejected (less readable for small failures).

---

## R-007: Implementation Constraints for Maintainability

### Decision
Keep line-count and complexity limits by isolating heavy diagnostic formatting logic from core assertion flow when necessary (for example, internal helper methods/file), while preserving public API location in `GenericCollectionAssertions<T>`.

### Rationale
`GenericCollectionAssertions.cs` is already near the 300-line constitution guideline. Separation keeps code readable and testable.

### Alternatives considered
- Keep all logic inline in one method/file: Rejected if it breaches maintainability limits.
