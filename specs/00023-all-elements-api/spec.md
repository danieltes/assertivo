# Feature Specification: First-Class All-Elements Assertions

**Feature Branch**: `00023-all-elements-api`  
**Created**: 2026-05-08  
**Status**: Draft  
**Input**: User description: "Elevate AllSatisfy to a first-class all-elements assertion API with stronger contracts and diagnostics, centered on assertion bodies rather than predicates."

## Constitution Structured Format *(required)*

Feature: First-Class All-Elements Assertions
As a .NET test author
I want all-elements assertions that evaluate every element and aggregate failures
So that I can diagnose all failing elements with deterministic, bounded diagnostics.

Scenario: Aggregated failure uses framework-compatible assertion exception
	Given a collection where one or more elements fail the inspector
	When AllSatisfy finishes aggregation
	Then the failure is surfaced through the active test-framework adapter exception type, or `AssertionFailedException` when no framework adapter is active.

## Clarifications

### Session 2026-05-08

- Q: How should AllSatisfy format diagnostics when many elements fail in one run? â†’ A: Include full error details for first 50 failing elements, include total failed count, and include all failing indices.
- Q: What ordering should AllSatisfy use when reporting aggregated failures (detailed entries and index list)? â†’ A: Preserve original enumeration order (ascending index).
- Q: When AllSatisfy finishes aggregation with at least one failure, what should be the final thrown type? â†’ A: Surface a framework-compatible assertion exception via the existing failure pipeline (`MessageFormatter`/`AssertionConfiguration`), falling back to `AssertionFailedException` when no framework adapter is active.
- Q: If the source enumerable itself throws during iteration (not inside the inspector callback), how should AllSatisfy behave? â†’ A: Stop immediately and rethrow the original enumeration exception.
- Q: How should AllSatisfy represent the all-failing-indices section when there are many failures? â†’ A: Adaptive format: explicit list for up to 100 indices, then range compression above 100.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert Every Element with Assertion Bodies (Priority: P1)

A developer writing tests for a collection wants to express multiple assertions per element inside one callback, instead of writing manual loops or reducing checks to single boolean predicates.

**Why this priority**: This is the core value of the feature. Without a first-class assertion-body API for all elements, users lose fluent expressiveness and readable diagnostics.

**Independent Test**: Can be fully tested by running an all-elements assertion on a collection of domain objects where each callback performs multiple fluent assertions and passes without requiring manual iteration.

**Acceptance Scenarios**:

1. **Given** a non-empty collection, **When** `AllSatisfy` is called with an assertion body that passes for all elements, **Then** the assertion passes.
2. **Given** a collection where one element violates one assertion in the body, **When** `AllSatisfy` runs, **Then** a single failure is thrown after full enumeration.
3. **Given** a collection where multiple elements fail, **When** `AllSatisfy` runs, **Then** all failing element indices are reported in one failure.
4. **Given** an empty collection, **When** `AllSatisfy` runs, **Then** the assertion passes vacuously.

---

### User Story 2 - Diagnose All-Elements Failures Precisely (Priority: P1)

A developer receiving a failed all-elements assertion wants actionable diagnostics that identify exactly which elements failed and why, including details from inner assertion messages.

**Why this priority**: Clear diagnostics are required for usability. All-elements assertions lose practical value if users cannot identify failures quickly.

**Independent Test**: Can be fully tested by intentionally causing multiple element failures and verifying that output includes index tags and per-element error details.

**Acceptance Scenarios**:

1. **Given** one element failure at index 2, **When** `AllSatisfy` fails, **Then** the failure output includes `[2]`.
2. **Given** mixed failure causes across elements, **When** `AllSatisfy` fails, **Then** each failing element entry includes index and error details.
3. **Given** a custom reason string, **When** `AllSatisfy` fails, **Then** the final failure message includes that reason.
4. **Given** more than 50 failing elements, **When** `AllSatisfy` fails, **Then** the output includes full error details for the first 50 failures, includes the total failure count, and includes all failing indices.
5. **Given** multiple failing elements, **When** `AllSatisfy` fails, **Then** detailed failure entries and the failing index list preserve original enumeration order (ascending index).
6. **Given** one or more element failures, **When** `AllSatisfy` completes, **Then** the final thrown exception is surfaced as the active framework assertion exception type from the standard failure pipeline, or `AssertionFailedException` when no framework adapter is active.
7. **Given** the enumerable throws during iteration before completion, **When** `AllSatisfy` is running, **Then** it stops immediately and rethrows the original enumeration exception.
8. **Given** 100 or fewer failing indices, **When** `AllSatisfy` fails, **Then** failing indices are shown explicitly in order; **Given** more than 100 failing indices, **Then** failing indices are shown using range-compression while preserving full index coverage.

---

### User Story 3 - Use Index-Aware Assertion Bodies (Priority: P2)

A developer wants to assert position-sensitive rules using an index-aware callback without manually managing a counter.

**Why this priority**: This improves expressiveness for ordering and position rules while keeping the API body-centric and fluent.

**Independent Test**: Can be fully tested with an index-aware all-elements callback that validates both element value rules and index-based rules in one run.

**Acceptance Scenarios**:

1. **Given** a collection, **When** `AllSatisfy(Action<T, int>)` is used, **Then** the callback receives zero-based index values in enumeration order.
2. **Given** an index-based rule violation, **When** `AllSatisfy` fails, **Then** the reported index matches the callback index.
3. **Given** all index-based rules pass, **When** `AllSatisfy` completes, **Then** the assertion passes.

### Edge Cases

- A null collection subject fails as an assertion failure, not a null-reference runtime failure.
- A null inspector callback throws `ArgumentNullException` immediately.
- Non-assertion exceptions thrown inside the inspector body are captured and reported per failing element.
- Deferred collection sources are enumerated exactly once per `AllSatisfy` call.
- Large failure sets include full error details for only the first 50 failures, while still including total failed count and all failing indices.
- If source enumeration itself throws, that original exception is rethrown immediately and no aggregated all-elements assertion failure is produced.
- Failing-index display uses explicit ordered indices for up to 100 entries and range-compressed output beyond 100 while still representing all failing indices.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `GenericCollectionAssertions<T>` MUST expose `AllSatisfy(Action<T> inspector, string because = "", params object[] becauseArgs)` as the primary all-elements assertion-body API.
- **FR-002**: `GenericCollectionAssertions<T>` MUST expose `AllSatisfy(Action<T, int> inspector, string because = "", params object[] becauseArgs)` for index-aware assertion bodies.
- **FR-003**: Both `AllSatisfy` overloads MUST reject a null inspector with `ArgumentNullException`.
- **FR-004**: Both `AllSatisfy` overloads MUST fail on null subject through the library's assertion failure mechanism.
- **FR-005**: `AllSatisfy` MUST execute the inspector for every element and MUST NOT fail fast on the first failure.
- **FR-006**: `AllSatisfy` MUST aggregate all element failures collected during successful enumeration and surface one final assertion failure via the standard failure pipeline using the active framework assertion exception type, or `AssertionFailedException` when no framework adapter is active.
- **FR-007**: Aggregated failure output MUST include each failing element index in square brackets.
- **FR-008**: Aggregated failure output MUST include inner assertion messages for assertion-related exceptions and exception type plus message for non-assertion exceptions for each failure entry that includes full details.
- **FR-009**: `AllSatisfy` MUST pass for empty collections.
- **FR-010**: `because` and `becauseArgs` MUST be applied to the final failure message.
- **FR-011**: Both `AllSatisfy` overloads MUST return `AndConstraint<GenericCollectionAssertions<T>>` for fluent chaining.
- **FR-012**: Existing dispatch paths that resolve to `GenericCollectionAssertions<T>` MUST support this API consistently across `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `List<T>`, and arrays.
- **FR-013**: Predicate-only all-elements semantics such as `OnlyContain(Func<T, bool>)` are out of scope for this feature.
- **FR-014**: Async per-element callback support is out of scope for this feature.
- **FR-015**: Global message formatter behavior outside all-elements diagnostics is out of scope for this feature.
- **FR-016**: When more than 50 elements fail, aggregated output MUST include full per-failure details for the first 50 failing elements, MUST include the total failed element count, and MUST include all failing indices.
- **FR-017**: Aggregated failure details and aggregated failing index lists MUST preserve original enumeration order (ascending index).
- **FR-018**: This feature MUST NOT introduce a new public aggregated-failure exception type; aggregated failures MUST use existing framework-adapter assertion exception surfaces with `AssertionFailedException` fallback as defined by FR-006.
- **FR-019**: If the source enumerable throws during iteration (outside inspector execution), `AllSatisfy` MUST stop immediately and rethrow that original exception.
- **FR-020**: The failing-index section in aggregated diagnostics MUST use adaptive formatting: explicit ordered indices for up to 100 failing indices and range-compressed ordered indices when there are more than 100 failing indices.
- **FR-021**: For this feature, assertion-related exception means `AssertionFailedException` (or subtype) produced by Assertivo assertion APIs.
- **FR-022**: Range-compressed failing-index format MUST use ascending comma-separated segments where each segment is either `N` (singleton) or `Start-End` (inclusive contiguous range), with non-overlapping segments and no index loss.

### Key Entities *(include if feature involves data)*

- **Element Inspection Context**: Represents the current element and its zero-based index as evaluated by an all-elements assertion.
- **Element Failure Record**: Represents one element-level failure with index and error detail used for aggregate diagnostics.
- **All-Elements Failure Summary**: Represents the combined failure output produced when one or more element inspections fail.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: New and existing `AllSatisfy` tests pass for pass, fail, and empty scenarios.
- **SC-002**: Tests verify null inspector throws `ArgumentNullException` for both overloads.
- **SC-003**: Tests verify multi-failure aggregation reports all failing indices in one failure.
- **SC-004**: Tests verify index-aware overload receives and enforces correct zero-based indices.
- **SC-005**: Existing collection and dispatch test suites run with zero regression failures.
- **SC-006**: Documentation includes at least one assertion-body all-elements example in collection guidance.
- **SC-007**: Tests verify the large-failure diagnostics policy: first 50 failures include full details, total failed count is reported, and all failing indices are present.
- **SC-008**: Tests verify aggregated failure detail ordering and failing index ordering match original enumeration order.
- **SC-009**: Tests verify aggregated all-elements failures are surfaced through the active framework assertion exception type, or `AssertionFailedException` when no framework adapter is active.
- **SC-010**: Tests verify enumeration-time source exceptions are rethrown unchanged and are not converted into aggregated assertion failures.
- **SC-011**: Tests verify adaptive failing-index formatting: explicit ordered indices for up to 100 failures and range-compressed ordered indices above 100, with no index loss.

## Assumptions

- The canonical user-facing name remains `AllSatisfy`; `OnlyContain` naming may be introduced later only as an alias.
- Existing baseline behavior for current `AllSatisfy(Action<T>)` remains compatible, while diagnostics and contracts are strengthened.
- Current collection assertion dispatch continues to be the entry path, so no new top-level entry-point pattern is required.
- The feature targets synchronous assertion bodies and does not introduce asynchronous callback execution semantics.
