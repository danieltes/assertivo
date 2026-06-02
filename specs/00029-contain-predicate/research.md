# Research: Predicate-Based Collection Containment

**Feature**: `00029-contain-predicate`  
**Phase**: 0 - Pre-design research  
**Date**: 2026-06-02

## Decision 1: Null predicate handling

**Decision**: `Contain(Func<T, bool> predicate, ...)` throws `ArgumentNullException` when `predicate` is `null`.

**Rationale**: A null predicate is API misuse, not an assertion outcome. Throwing immediately matches .NET argument-guard conventions and keeps assertion failures focused on subject/value mismatches.

**Alternatives considered**:
- Treat null predicate as an assertion failure. Rejected because it conflates invalid API usage with assertion semantics.
- Let a downstream null dereference occur. Rejected because failure mode is unclear and non-deterministic.

## Decision 2: Match semantics and evaluation strategy

**Decision**: The assertion passes when at least one element matches; implementation uses a single pass with early exit (equivalent to `Any(predicate)` behavior).

**Rationale**: The feature intent is existential matching (`there exists an element`). Single-pass early exit minimizes work and allocations while preserving behavior on large sequences.

**Alternatives considered**:
- Count all matches first. Rejected because exact cardinality is not required and full traversal adds cost.
- Reuse `ContainSingle(predicate)`. Rejected because it enforces exactly-one semantics and is behaviorally different.

## Decision 3: Failure contract when no match exists

**Decision**: On zero matches, fail with expected = `a collection containing a matching element` and actual = `no element matched the predicate`.

**Rationale**: This message is explicit, user-facing, and directly answers why the assertion failed without requiring manual debugging.

**Alternatives considered**:
- Generic mismatch text (`collection did not contain it`). Rejected because it implies value-based containment, not predicate matching.
- Include predicate source text. Rejected because expression text for lambdas is not reliably available and would add complexity.

## Decision 4: Null subject and because behavior

**Decision**: Null subject handling remains delegated to existing `GuardNull()`. `because` and `becauseArgs` flow through `MessageFormatter.Fail` unchanged.

**Rationale**: Reusing existing guard behavior keeps consistency across collection assertions. Existing message formatting already supports reason inclusion and structured output.

**Alternatives considered**:
- Inline duplicate null-subject check per method. Rejected to avoid behavior drift and duplication.
- Custom message formatting path. Rejected because existing formatter already meets requirement.

## Decision 5: API compatibility and overload strategy

**Decision**: Add a new overload on existing `Contain` in `GenericCollectionAssertions<T>` and update public API contract docs for this feature.

**Rationale**: This aligns with constitution API rule "overloads over new names" and preserves discoverability.

**Alternatives considered**:
- Introduce `ContainAny(...)` or similar new method name. Rejected as unnecessary API surface expansion.

## Resolved Clarifications

All known clarifications are resolved:
- Null predicate -> `ArgumentNullException`.
- Null subject -> standard guard failure.
- Matching semantics -> at least one match required.
- Failure message -> explicit predicate-match wording.
