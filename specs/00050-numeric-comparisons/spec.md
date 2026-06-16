# Feature Specification: Add BeGreaterThan and BeLessThanOrEqualTo to NumericAssertions

**Feature Branch**: `00050-numeric-comparisons`
**Created**: 2026-06-15
**Status**: Draft
**Input**: User description: "Add the missing `BeGreaterThan` and `BeLessThanOrEqualTo` methods to `NumericAssertions<T>` to complete the four-method comparison set."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert Strict Lower Bound (Priority: P1)

A developer writing a numeric range test needs to assert that a computed value is strictly greater than a threshold. They write `result.Should().BeGreaterThan(minValue)` and, if the assertion fails, receive a clear message naming the condition and both the expected threshold and the actual value.

**Why this priority**: Strict lower-bound assertions are among the most common numeric checks (e.g., "response time must be above zero", "score must exceed the passing threshold"). The absence of this method forces awkward workarounds that produce weaker failure diagnostics. This story also covers the optional `IComparer<T>` parameter for `BeGreaterThan` — custom comparer injection is part of this story's scope (FR-001), not deferred to User Story 3.

**Independent Test**: Fully testable by calling `BeGreaterThan` on a `NumericAssertions<int>` instance — the passing case returns an `AndConstraint`, the failing case throws `AssertionFailedException` with the correct message. No other method or story is required.

**Acceptance Scenarios**:

1. **Given** a subject value of `5`, **When** `BeGreaterThan(3)` is called, **Then** the assertion passes and an `AndConstraint<NumericAssertions<T>>` is returned.
2. **Given** a subject value of `3`, **When** `BeGreaterThan(3)` is called (equal to threshold), **Then** the assertion fails with a message that states the expected condition ("a value greater than 3") and the actual value ("3").
2. **Given** a subject value of `2`, **When** `BeGreaterThan(3)` is called, **Then** the assertion fails with a message that states the expected condition and the actual value.
3. **Given** a subject value of `5` and a custom `IComparer<int>` that inverts natural order, **When** `BeGreaterThan(3, comparer: invertedComparer)` is called, **Then** the assertion fails because the comparer treats `5` as less than `3`.
4. **Given** a failing assertion with `because: "the value must be positive"`, **When** the failure message is captured, **Then** it includes the `because` phrase.

---

### User Story 2 - Assert Inclusive Upper Bound (Priority: P1)

A developer needs to assert that a computed value is within an upper limit, including the limit itself. They write `result.Should().BeLessThanOrEqualTo(maxValue)` and, on failure, receive a message that names the condition and both values.

**Why this priority**: Inclusive upper-bound assertions are as common as strict lower-bound assertions (e.g., "latency must not exceed 100 ms", "count must be at most 50"). Without this method the API is visibly asymmetric: `BeLessThan` exists but `BeLessThanOrEqualTo` does not.

**Independent Test**: Fully testable by calling `BeLessThanOrEqualTo` on a `NumericAssertions<int>` instance — passing case returns an `AndConstraint`, failing case throws `AssertionFailedException`.

**Acceptance Scenarios**:

1. **Given** a subject value of `3`, **When** `BeLessThanOrEqualTo(5)` is called, **Then** the assertion passes.
2. **Given** a subject value of `5`, **When** `BeLessThanOrEqualTo(5)` is called (equal to threshold), **Then** the assertion passes.
3. **Given** a subject value of `6`, **When** `BeLessThanOrEqualTo(5)` is called, **Then** the assertion fails with a message that states the expected condition ("a value less than or equal to 5") and the actual value ("6").
4. **Given** a failing assertion with `because: "the result must not exceed the limit"`, **When** the failure message is captured, **Then** it includes the `because` phrase.

---

### User Story 3 - Custom Comparer Injection for BeLessThanOrEqualTo (Priority: P2)

A developer with a domain-specific ordering (e.g., case-insensitive string comparison, inverted priority scale) injects a custom `IComparer<T>` to override the default comparison logic for `BeLessThanOrEqualTo`.

**Why this priority**: Consistency with the existing `BeGreaterThanOrEqualTo` and `BeLessThan` methods, both of which accept an optional comparer. Omitting this parameter would create an asymmetric API and violate the constitution's rule that custom comparers must be injectable at the call site.

**Independent Test**: Testable by passing a custom comparer to `BeLessThanOrEqualTo` and verifying the result differs from default comparison.

**Acceptance Scenarios**:

1. **Given** a subject value of `3` and an inverted comparer (treats higher as lower), **When** `BeLessThanOrEqualTo(5, comparer: invertedComparer)` is called, **Then** the assertion fails because the comparer treats `3` as greater than `5`.

---

### Edge Cases

- **Equal to threshold for BeGreaterThan**: A subject equal to the threshold MUST fail (`>` not `>=`).
- **Equal to threshold for BeLessThanOrEqualTo**: A subject equal to the threshold MUST pass (`<=` not `<`).
- **Null comparer parameter**: When `null` is passed as the comparer, the method MUST fall back to `Comparer<T>.Default` without throwing.
- **Empty because / becauseArgs**: When `because` is empty and `becauseArgs` is empty, the failure message MUST still be well-formed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `NumericAssertions<T>` MUST expose a `BeGreaterThan(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` method.
- **FR-002**: `BeGreaterThan` MUST throw `AssertionFailedException` when the subject is less than or equal to the threshold value (i.e., `comparer.Compare(subject, value) <= 0`).
- **FR-003**: `BeGreaterThan` MUST pass (return `AndConstraint`) when the subject is strictly greater than the threshold (i.e., `comparer.Compare(subject, value) > 0`).
- **FR-004**: `NumericAssertions<T>` MUST expose a `BeLessThanOrEqualTo(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` method.
- **FR-005**: `BeLessThanOrEqualTo` MUST throw `AssertionFailedException` when the subject is strictly greater than the threshold (i.e., `comparer.Compare(subject, value) > 0`).
- **FR-006**: `BeLessThanOrEqualTo` MUST pass when the subject is less than or equal to the threshold (i.e., `comparer.Compare(subject, value) <= 0`).
- **FR-007**: Both methods MUST use `Comparer<T>.Default` when the `comparer` parameter is `null`.
- **FR-008**: Failure messages MUST follow the established format: `"a value [condition] {threshold}"`, where `[condition]` is `"greater than"` or `"less than or equal to"`.
- **FR-009**: Failure messages MUST include the actual subject value alongside the expected condition.
- **FR-010**: Both methods MUST incorporate the `because` and `becauseArgs` values into the failure message using the existing `MessageFormatter` contract.
- **FR-011**: Both methods MUST return `AndConstraint<NumericAssertions<T>>` to allow assertion chaining (e.g., `.BeGreaterThan(0).And.BeLessThan(100)`).
- **FR-012**: Both methods MUST be decorated with `[StackTraceHidden]` so that assertion failure stack traces point to the test call site, not the library internals.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can write all four comparison assertions (`BeGreaterThan`, `BeGreaterThanOrEqualTo`, `BeLessThan`, `BeLessThanOrEqualTo`) on any `NumericAssertions<T>` instance without a compile error.
- **SC-002**: Every failing assertion produces a message that uniquely identifies the expected condition, the actual value, and the caller expression — no two distinct failure modes produce identical messages.
- **SC-003**: Test coverage for both new methods meets the project minimum: at least one passing scenario, one failing scenario, one custom-comparer scenario, and one `because`-formatting scenario per method.
- **SC-004**: The new methods integrate with the assertion chain such that `.BeGreaterThan(x).And.BeLessThan(y)` compiles and executes correctly.
- **SC-005**: Boundary cases (subject equal to threshold) produce the correct pass/fail result for each method.

## Assumptions

- The two new methods will be added directly to `NumericAssertions<T>` in the same file as the existing `BeGreaterThanOrEqualTo` and `BeLessThan` methods, requiring no structural changes.
- The spec uses the term "threshold" in prose to describe the boundary value being compared against. Technical documents (plan, contracts, method signatures) use "value" — the actual parameter name consistent with the existing method pair. Both terms refer to the same thing.
- The `MessageFormatter.Fail` and `MessageFormatter.FormatValue` utilities available to the existing methods are sufficient for the new methods — no new formatting infrastructure is needed.
- `T` is constrained to numeric types at the dispatch layer; `NumericAssertions<T>` itself relies on `IComparer<T>` rather than arithmetic operators, so no additional generic constraints are required on the class.
- No negation variants (`Not.BeGreaterThan`, `Not.BeLessThanOrEqualTo`) are in scope for this feature; they may be addressed in a follow-up per the constitution's allowance for incremental negation delivery.
- Performance characteristics of the new methods will match the existing pair, as the implementation pattern is identical.
