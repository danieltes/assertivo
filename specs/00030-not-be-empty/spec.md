# Feature Specification: NotBeEmpty Assertion

**Feature Branch**: `00030-not-be-empty`
**Created**: 2026-06-07
**Status**: Draft
**Input**: User description: "Add NotBeEmpty assertion for strings and collections"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert String Is Not Empty (Priority: P1)

A developer writing a test to verify that a string value (such as a response body, user input, or configuration value) contains content wants to express this intent directly and readably in a single assertion call. The library already offers `BeEmpty()` for strings, so the absence of its complement forces developers to write workarounds or less expressive code.

**Why this priority**: Strings are among the most frequently asserted values in unit tests. The missing `NotBeEmpty` is an API asymmetry that affects every test suite that needs to confirm a string has content.

**Independent Test**: Fully testable by calling `NotBeEmpty()` on various string values and verifying pass/fail behavior; no dependency on collection assertion behavior.

**Acceptance Scenarios**:

1. **Given** a non-empty string (e.g., `"hello"`), **When** the developer asserts it is not empty, **Then** the assertion passes.
2. **Given** an empty string, **When** the developer asserts it is not empty, **Then** the assertion fails with a message that identifies the subject expression and clearly describes expected vs. actual.
3. **Given** an empty string and a supplied reason, **When** the assertion fails, **Then** the failure message includes the developer-supplied reason.
4. **Given** a null string, **When** the developer asserts it is not empty, **Then** the assertion passes (null is not the empty string and is therefore "not empty" under the strict logical complement of `BeEmpty`).

---

### User Story 2 - Assert Collection Has at Least One Element (Priority: P1)

A developer writing a test to verify that a collection produced by some operation is non-empty — for example, that a query returned results, a pipeline produced output, or a batch operation processed items — wants to express this directly. The library provides `BeEmpty()` for collections but not its complement, creating the same asymmetry as with strings.

**Why this priority**: Collection non-emptiness checks are equally fundamental to string non-emptiness. Both are first-class assertions that belong in a symmetric API.

**Independent Test**: Fully testable by calling `NotBeEmpty()` on various collections; no dependency on string assertion behavior.

**Acceptance Scenarios**:

1. **Given** a collection with one or more elements, **When** the developer asserts it is not empty, **Then** the assertion passes.
2. **Given** an empty collection, **When** the developer asserts it is not empty, **Then** the assertion fails with a message indicating expected "a non-empty collection" and actual "an empty collection."
3. **Given** an empty collection and a supplied reason, **When** the assertion fails, **Then** the failure message includes the developer-supplied reason.
4. **Given** a null collection reference, **When** the developer asserts it is not empty, **Then** the assertion fails immediately, producing a clear null-subject failure message before any element-count evaluation.

---

### Edge Cases

- What happens when the string is whitespace-only (e.g., `"   "`)? → The assertion passes — whitespace-only strings are not the empty string. Asserting against whitespace content is a separate concern outside this feature.
- What happens when the null string is asserted? → Passes; null is logically distinct from `""` under the strict complement of `BeEmpty`.
- What happens when the null collection is asserted? → Fails with a null-subject failure message, consistent with how `BeEmpty` handles a null collection.
- What happens when the collection is a lazy sequence (e.g., an iterator)? → The assertion evaluates the sequence only as far as needed to determine whether at least one element exists.
- Does the assertion compose with further chained assertions? → Yes; both string and collection forms return a chainable constraint that permits additional assertions in the same fluent chain.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST expose a `NotBeEmpty` assertion on string assertion contexts that returns a chainable constraint for further assertions.
- **FR-002**: The `NotBeEmpty` string assertion MUST pass when the subject is any value other than the empty string, including `null`.
- **FR-003**: The `NotBeEmpty` string assertion MUST fail when the subject is the empty string, producing a failure message that includes the subject expression, an expected description of "a non-empty string," and the actual value.
- **FR-004**: The library MUST expose a `NotBeEmpty` assertion on generic collection assertion contexts that returns a chainable constraint for further assertions.
- **FR-005**: The `NotBeEmpty` collection assertion MUST fail immediately when the subject collection reference is null, producing a null-subject failure message without evaluating element count.
- **FR-006**: The `NotBeEmpty` collection assertion MUST pass when the subject collection contains at least one element.
- **FR-007**: The `NotBeEmpty` collection assertion MUST fail when the subject collection is empty (zero elements), producing a failure message with expected "a non-empty collection" and actual "an empty collection."
- **FR-008**: Both `NotBeEmpty` assertions MUST accept an optional reason parameter and optional reason-argument parameters; when provided, the failure message MUST include the formatted reason text.
- **FR-009**: Both `NotBeEmpty` assertions MUST be hidden from assertion failure stack traces so that the test-code call site is the first visible frame in any failure output.
- **FR-010**: The public API contract documentation MUST be updated to list both new assertion signatures alongside their `BeEmpty` counterparts.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every existing automated test for `BeEmpty` on strings and collections continues to pass without modification after `NotBeEmpty` is introduced.
- **SC-002**: Developers can call `value.Should().NotBeEmpty()` on both strings and collections without any additional imports or configuration beyond what is already required to use `BeEmpty`.
- **SC-003**: 100% of the acceptance scenarios defined in User Stories 1 and 2 are covered by automated tests, each completing in under 100 ms.
- **SC-004**: Assertion failure messages produced by `NotBeEmpty` include all three required elements — subject expression, expected condition, and actual value or condition — in 100% of failure scenarios.
- **SC-005**: The public API contract lists `NotBeEmpty` as a symmetric counterpart to `BeEmpty` for both string and collection assertion contexts.
- **SC-006**: The `NotBeEmpty` assertion chains correctly with `.And` for further assertions in all primary usage scenarios.

## Assumptions

- **Null-string behavior**: a null string subject **passes** `NotBeEmpty`. This follows the strict logical complement of `BeEmpty` (which only passes for `""`). Developers who need both null and empty to fail should use a separate `NotBeNullOrEmpty` assertion, which is out of scope for this feature.
- **Null-collection behavior**: a null collection subject **fails** `NotBeEmpty`, consistent with the existing `BeEmpty` behavior that validates the subject before checking element count.
- Whitespace-only strings are considered non-empty by this assertion; whitespace-awareness is a separate concern.
- The collection assertion applies to all sequence types (arrays, lists, and other enumerable types), not only concrete list implementations.
- The `NotBeEmpty` assertions for both strings and collections are positioned in the public API immediately after their corresponding `BeEmpty` assertions to maintain readability and discoverability.
