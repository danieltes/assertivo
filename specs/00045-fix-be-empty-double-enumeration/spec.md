# Feature Specification: Fix BeEmpty Double Enumeration

**Feature Branch**: `00045-fix-be-empty-double-enumeration`  
**Created**: 2026-06-09  
**Status**: Draft  
**Input**: User description: "Fix GenericCollectionAssertions<T>.BeEmpty double enumeration bug"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Correct Failure Message on Non-Replayable Sequence (Priority: P1)

A developer calls `BeEmpty()` on a non-replayable `IEnumerable<T>` — for example, a LINQ-to-database query, a `Channel<T>` reader, or a custom iterator with side effects — that yields one or more elements. The assertion should fail with a message that accurately states how many elements were found.

**Why this priority**: This is the core defect. Without this fix, the failure message may report "a collection with 0 item(s)" or crash entirely, violating the Zero Surprise principle and making failures harder to diagnose.

**Independent Test**: Can be tested by passing a custom `IEnumerable<T>` implementation that throws on a second enumeration and verifying that `BeEmpty()` both fails and produces a message containing the correct count.

**Acceptance Scenarios**:

1. **Given** an `IEnumerable<T>` that yields 3 elements and cannot be enumerated more than once, **When** `BeEmpty()` is called, **Then** the assertion fails with a message containing "a collection with 3 item(s)".
2. **Given** an `IEnumerable<T>` that yields 1 element and throws on a second enumeration, **When** `BeEmpty()` is called, **Then** the assertion fails without throwing a secondary exception.

---

### User Story 2 - Existing Behavior Preserved for Replayable Collections (Priority: P2)

A developer calls `BeEmpty()` on standard in-memory collections such as `List<T>`, arrays, or `IEnumerable<T>` from `Enumerable.Empty<T>()`. All existing test contracts must continue to pass without modification.

**Why this priority**: Correctness of the fix must not regress existing behavior. Developers relying on `BeEmpty()` for everyday assertions should see no change.

**Independent Test**: The existing `BeEmpty` test suite passes without any test modifications.

**Acceptance Scenarios**:

1. **Given** an empty `List<int>`, **When** `BeEmpty()` is called, **Then** the assertion passes.
2. **Given** a `List<string>` with 2 elements, **When** `BeEmpty()` is called, **Then** the assertion fails with a message containing "a collection with 2 item(s)".

---

### Edge Cases

- What happens when the subject is an empty non-replayable sequence? The assertion should pass without any enumeration issues.
- What happens when the subject itself is `null`? Existing null-handling behavior is preserved (no change in this fix).
- What happens when `Count()` on the sequence throws? The exception propagates naturally — this fix does not add catch blocks.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `BeEmpty` MUST enumerate the subject exactly once, regardless of whether the subject is a replayable or non-replayable `IEnumerable<T>`.
- **FR-002**: When `BeEmpty` fails, the failure message MUST accurately report the number of elements found during that single enumeration.
- **FR-003**: The failure message format MUST remain "a collection with N item(s)" — the same format used before the fix.
- **FR-004**: The fix MUST NOT require modification of any existing test file. No test method, assertion, or helper that existed before this change may be altered, renamed, or removed.
- **FR-005**: When the assertion passes (subject is empty and no exception is thrown), the fix MUST NOT introduce any additional allocations or enumerations. On the failure path, allocations are bounded by the constitution §VI.2 limit of a single enumerator and the result object.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `BeEmpty` on a non-replayable `IEnumerable<T>` with N elements produces a failure message that contains the string "a collection with N item(s)", where N matches the actual element count.
- **SC-002**: `BeEmpty` on a non-replayable `IEnumerable<T>` does not throw a secondary exception during failure-message construction.
- **SC-003**: All existing `BeEmpty` tests pass without modification after the fix.
- **SC-004**: `BeEmpty` on an empty non-replayable sequence passes the assertion without error.

## Assumptions

- The fix is a targeted one-line change to `GenericCollectionAssertions<T>.BeEmpty`; no other assertion methods are in scope.
- Non-null subject handling remains unchanged; the fix addresses only the double-enumeration pattern.
- The existing failure message format ("a collection with N item(s)") is the accepted format and requires no change.
- Tests for `BeEmpty` already exist and cover the happy path, the failure path, and edge cases for in-memory collections.
