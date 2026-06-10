# Research: Fix BeEmpty Double Enumeration

**Feature**: `00045-fix-be-empty-double-enumeration`  
**Date**: 2026-06-09

## Summary

No external research required. All design decisions are resolved from reading the existing source and test suite.

---

## Decision 1: Single Count() vs ToList() materialisation

**Decision**: Use `var count = Subject!.Count()` to enumerate once and branch on the result.

**Rationale**: `Count()` is the minimal operation needed — it returns the count without materialising the sequence into a new allocation. `ToList()` would also enumerate once but allocates a `List<T>` that is immediately discarded. `ContainSingle` uses `ToList()` because it needs element access; `BeEmpty` does not.

**Alternatives considered**:
- `ToList()` — rejected: unnecessary allocation, constitution §VI.2 allocates "no more than a single enumerator and the result object"
- Keep `Any()` + materialise-on-failure — rejected: still enumerates the sequence once for `Any()` before we know we need the count

---

## Decision 2: Failure message format

**Decision**: Keep "a collection with N item(s)" unchanged.

**Rationale**: The existing `BeEmpty_WithNonEmptyCollection_Fails` test asserts on the exception type only (not the message text), so the format is not under a hard test contract — but consistency with all other collection assertion messages is preferred. The user description explicitly confirms the format must stay the same.

---

## Decision 3: Null guard position

**Decision**: `GuardNull()` is called before the `Count()` call, unchanged from the current implementation.

**Rationale**: `GuardNull()` throws when `Subject` is null, so the `Subject!.Count()` call is always reached with a non-null reference. The `!` null-forgiving operator on the second call in the original code (`Subject!.Count()`) becomes the only occurrence in the fixed code.

---

## Allocation Verification

**Decision**: Single `var count = Subject!.Count()` introduces zero additional heap allocations on the happy path.

**Analysis**:
- `var count` is a stack-allocated `int` — no boxing, no closure capture, no heap allocation.
- On the happy path (empty sequence): `Count()` returns 0; the `if (count > 0)` branch is not entered; no string interpolation or `AssertionFailedException` is constructed.
- On the failure path: one `string` interpolation (`$"a collection with {count} item(s)"`) allocates the `Actual` message — identical to the pre-fix behaviour. Constitution §VI.2 permits this as "the result object."
- The fix reduces allocations vs. the original: two enumerator allocations (`.Any()` + `.Count()`) become one.

**Conclusion**: Constitution §VI.2 compliance confirmed by IL inspection (no `box`, `newobj`, or `ldftn` instructions added).

## Resolved Unknowns

None — all technical questions answered from source inspection.
