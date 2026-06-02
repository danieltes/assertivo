# Feature Specification: Contain(predicate) Overload for Collection Assertions

**Feature Branch**: `00029-contain-predicate`  
**Created**: 2026-06-02  
**Status**: Draft  
**Input**: User description: "Add Contain(predicate) overload to GenericCollectionAssertions"

## Clarifications

### Session 2026-06-02

- Q: How should `Contain(predicate)` behave when `predicate` is null? → A: Throw `ArgumentNullException` immediately.
- Q: What is the canonical null-subject failure contract for this feature? → A: Use the existing collection null-guard contract (`expected = "a collection"`, `actual = "<null>"`).

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Predicate-Based Collection Containment (Priority: P1)

A developer writing tests against a collection wants to assert that at least one element satisfies a given condition, without manually reducing the collection to a boolean. They write a single assertion and, on failure, receive a message that clearly indicates no matching element was found.

**Why this priority**: This is the core feature. All other user stories depend on this capability being available and producing meaningful output.

**Independent Test**: Can be fully tested by exercising the new assertion against a collection where one or more elements satisfy the predicate, verifying the assertion passes, and then exercising it against a collection where no element satisfies the predicate, verifying a descriptive failure message is produced.

**Acceptance Scenarios**:

1. **Given** a non-empty collection where at least one element satisfies the predicate, **When** `.Should().Contain(predicate)` is called, **Then** the assertion passes without error.
2. **Given** a non-empty collection where no element satisfies the predicate, **When** `.Should().Contain(predicate)` is called, **Then** the assertion fails with a message indicating no matching element was found.
3. **Given** a collection where multiple elements satisfy the predicate, **When** `.Should().Contain(predicate)` is called, **Then** the assertion passes (any match is sufficient).

---

### User Story 2 - Informative Failure Messages with `because` (Priority: P2)

A developer wants to provide a reason string to the assertion so that, when the assertion fails, the failure message explains not just what happened but why the assertion was expected to hold.

**Why this priority**: Informative failure messages are central to the value proposition of a fluent assertion library. The `because` parameter is part of the standard API contract and must work consistently.

**Independent Test**: Can be fully tested by calling `.Should().Contain(predicate, "because {0}", reason)` on a failing collection and confirming the reason string appears in the failure message.

**Acceptance Scenarios**:

1. **Given** a failing assertion with a `because` argument, **When** the failure message is inspected, **Then** it includes the formatted reason string.
2. **Given** a passing assertion with a `because` argument, **When** the assertion completes, **Then** no failure is raised and the reason is not relevant.

---

### User Story 3 - Assertion Chaining (Priority: P3)

A developer wants to chain additional assertions after a successful predicate-based containment check, using the fluent `.And` continuation, without needing to restate the subject.

**Why this priority**: Chainability is a standard expectation of the fluent API. Without it, developers cannot compose compound assertions in a single readable expression.

**Independent Test**: Can be fully tested by calling `.Should().Contain(predicate).And.HaveCount(n)` and confirming the expression compiles and both assertions are evaluated.

**Acceptance Scenarios**:

1. **Given** a passing assertion, **When** `.Should().Contain(predicate).And.HaveCount(n)` is chained, **Then** the chain compiles and both assertions are evaluated correctly.

---

### Edge Cases

- What happens when the collection subject is `null`? The assertion must fail with the standard null-subject failure message rather than throwing a `NullReferenceException`.
- What happens when the collection is empty? The assertion must fail because no element can satisfy the predicate.
- What happens when the predicate itself is `null`? The API throws `ArgumentNullException` immediately.
- What happens when the predicate throws an exception for some elements? The exception propagates; no special handling is required beyond what the runtime provides.
- What happens with deferred or single-use enumerables? The assertion performs at most one forward pass and must not require re-enumeration.
- What happens with side-effectful predicates? Predicate side effects are not suppressed; the predicate may run until the first match or sequence end.
- What happens when `because` is empty or whitespace? Empty/whitespace reasons are treated as not supplied and must not add a `Because:` line.
- What happens when `because` uses placeholders? Format placeholders are resolved using `becauseArgs` and included in failure output.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The assertion library MUST expose a `Contain(predicate)` assertion that accepts a predicate function and passes when at least one element in the collection satisfies the predicate.
- **FR-002**: The assertion MUST accept an optional reason string and reason arguments, formatted into the failure message when the assertion fails.
- **FR-003**: The assertion MUST fail with a clear, human-readable message when no element in the collection matches the predicate, stating that no matching element was found.
- **FR-004**: The assertion MUST fail when the collection subject is null, producing the standard null-subject failure message.
- **FR-005**: The assertion MUST fail when the collection is empty, as no element can satisfy the predicate.
- **FR-006**: The assertion MUST return a continuation object allowing further assertions to be chained via `.And`.
- **FR-007**: The assertion MUST NOT require exactly one match; one or more matching elements constitutes a passing assertion.
- **FR-008**: The public API contract documentation MUST be updated to reflect the new overload.
- **FR-008**: The public API documentation updates MUST include `specs/00029-contain-predicate/contracts/public-api.md`, `specs/00029-contain-predicate/quickstart.md`, and XML documentation comments on the public overload.
- **FR-009**: Automated tests covering all acceptance scenarios MUST be provided alongside the implementation.
- **FR-010**: If the predicate argument is null, the API MUST throw `ArgumentNullException` before evaluating the subject.
- **FR-011**: The null-subject assertion failure for this feature MUST use the canonical collection null-guard contract (`expected = "a collection"`, `actual = "<null>"`).
- **FR-012**: The implementation MUST evaluate the subject in a single pass and stop at the first matching element.
- **FR-013**: Failure diagnostics for no-match MUST be machine-verifiable by asserting required output components: expected phrase, actual phrase, optional expression line, and optional because line.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 7 acceptance scenarios in the inventory below pass as automated tests.
- **SC-002**: A developer can write a predicate-based containment assertion in a single expression with no workarounds.
- **SC-003**: A failing assertion produces a failure message that unambiguously identifies the cause (no matching element, null subject, or empty collection) — verifiable by reading the assertion output without inspecting source code.
- **SC-004**: The chaining expression `.Should().Contain(predicate).And.HaveCount(n)` compiles and evaluates both assertions without compiler errors, confirmed by a passing automated test.
- **SC-005**: The existing `Contain(T expected, comparer)` overload continues to pass all its existing tests after the predicate overload is introduced (no regression).

### Acceptance Scenario Inventory (for SC-001)

1. **AS-001**: At least one element matches predicate -> pass.
2. **AS-002**: No elements match predicate -> fail with no-match diagnostics.
3. **AS-003**: Multiple elements match predicate -> pass.
4. **AS-004**: Empty collection -> fail.
5. **AS-005**: Null collection subject -> fail with canonical null-subject contract.
6. **AS-006**: Chaining `.Contain(predicate).And.HaveCount(n)` compiles and evaluates.
7. **AS-007**: Failing assertion with `because` includes formatted reason text.

### Requirement-to-Scenario Traceability

| Requirement | Acceptance Scenarios | Success Criteria |
|---|---|---|
| FR-001 | AS-001, AS-002, AS-003 | SC-001, SC-002 |
| FR-002 | AS-007 | SC-001, SC-003 |
| FR-003 | AS-002 | SC-001, SC-003 |
| FR-004 | AS-005 | SC-001, SC-003 |
| FR-005 | AS-004 | SC-001, SC-003 |
| FR-006 | AS-006 | SC-001, SC-004 |
| FR-007 | AS-003 | SC-001 |
| FR-008 | AS-007 (doc and API usability verification) | SC-002 |
| FR-009 | AS-001..AS-007 | SC-001..SC-005 |
| FR-010 | AS-005 plus null-predicate guard ordering test | SC-001 |
| FR-011 | AS-005 | SC-003 |
| FR-012 | AS-001, AS-002, AS-003 | SC-002 |
| FR-013 | AS-002, AS-007 | SC-003 |

## Assumptions

- The predicate overload is distinct from `ContainSingle(predicate)`, which requires exactly one match; this feature requires only at least one match.
- The `.Which` drill-down to the matched element is out of scope; `ContainSingle(predicate)` already serves that use case.
- The null-subject guard reuses the existing `GuardNull()` helper already present in the assertion class.
- The `because` / `becauseArgs` parameters follow the same formatting convention as all other assertions in the library.
- Enumeration is single-pass only; no buffering or second pass is required.
