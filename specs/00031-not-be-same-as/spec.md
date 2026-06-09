# Feature Specification: NotBeSameAs — Object Reference Inequality Assertion

**Feature Branch**: `00031-not-be-same-as`  
**Created**: 2026-06-08  
**Status**: Draft  
**Input**: User description: "Add `NotBeSameAs(object? unexpected, ...)` to `ObjectAssertions<T>` to complement the existing `BeSameAs()` assertion."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert Two Distinct Object Instances (Priority: P1)

A developer calling a factory, builder, or `new` expression wants to verify that the result is a fresh object — not the same cached or shared instance as another variable. They write `a.Should().NotBeSameAs(b)` and expect the assertion to pass when the two variables point to different heap objects.

**Why this priority**: This is the primary motivating use case. Without it, developers must break out of the fluent chain or use a non-fluent assertion API.

**Independent Test**: Fully testable with two `new object()` allocations. Delivers standalone value as soon as the method exists.

**Acceptance Scenarios**:

1. **Given** two variables holding distinct object instances, **When** `NotBeSameAs` is called with the other instance, **Then** the assertion passes and returns an `AndConstraint` for further chaining.
2. **Given** two variables holding the same object instance (same reference), **When** `NotBeSameAs` is called with that reference, **Then** an `AssertionFailedException` is thrown.

---

### User Story 2 - Null Reference Semantics (Priority: P2)

A developer wants to verify that a null subject is not the same reference as a non-null object (e.g., confirming that an optional output was not populated), or understand that two null values are considered the same reference (both point to the null singleton).

**Why this priority**: Null handling is a critical edge case; incorrect null semantics would silently produce wrong test results.

**Independent Test**: Testable with `null` literals and `new object()`. Verifies correct `ReferenceEquals(null, null)` behaviour independently of the happy path.

**Acceptance Scenarios**:

1. **Given** a null subject and a non-null unexpected value, **When** `NotBeSameAs` is called, **Then** the assertion passes (null is not the same reference as a heap object).
2. **Given** a null subject and a null unexpected value, **When** `NotBeSameAs` is called, **Then** an `AssertionFailedException` is thrown (both are the same null reference).

---

### User Story 3 - Value Type Guard (Priority: P3)

A developer accidentally calls `NotBeSameAs` on a value-type subject (e.g., an `int` or `struct`). Because reference identity is meaningless for boxed value types, the assertion should reject the call with a clear, actionable error message rather than silently producing misleading results.

**Why this priority**: Guard rails prevent misuse; a confusing `InvalidOperationException` at test time is far better than a silently wrong green test.

**Independent Test**: Testable by calling `42.Should().NotBeSameAs(42)` and asserting that `InvalidOperationException` is thrown with the correct message.

**Acceptance Scenarios**:

1. **Given** a value-type subject (e.g., `int`, `bool`, `struct`), **When** `NotBeSameAs` is called, **Then** an `InvalidOperationException` is thrown with a message indicating that reference comparison is not meaningful for value types and directing the developer to use `Be()` instead.

---

### User Story 4 - Failure Message with Reason (Priority: P2)

A developer wants to include a human-readable explanation in the failure output to make CI logs self-explanatory when the assertion fails. They call `a.Should().NotBeSameAs(b, because: "the factory must return a new instance")`.

**Why this priority**: Consistent with all other assertion methods in the library; omitting `because` support would be a surprising gap.

**Independent Test**: Testable by triggering a failure with a `because` string and asserting the reason appears in the exception message.

**Acceptance Scenarios**:

1. **Given** two variables holding the same reference and a non-empty `because` string, **When** `NotBeSameAs` is called, **Then** the failure message contains the formatted reason text.

---

### Edge Cases

- null/null and null/non-null semantics: covered by User Story 2 acceptance scenarios.
- Value-type boxing: covered by User Story 3 acceptance scenario.
- `because` with empty `becauseArgs`: the raw `because` string is included as-is; no format substitution is attempted.
- Non-null subject / null `unexpected`: `ReferenceEquals(obj, null)` is `false` — assertion passes. Covered implicitly by FR-002; no dedicated acceptance scenario (BCL guarantee, not library logic).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST expose a `NotBeSameAs(object? unexpected, string because = "", params object[] becauseArgs)` method on `ObjectAssertions<T>` returning `AndConstraint<ObjectAssertions<T>>`.
- **FR-002**: `NotBeSameAs` MUST pass (return without throwing) when `ReferenceEquals(Subject, unexpected)` is `false`.
- **FR-003**: `NotBeSameAs` MUST throw `AssertionFailedException` when `ReferenceEquals(Subject, unexpected)` is `true`.
- **FR-004**: `NotBeSameAs` MUST throw `InvalidOperationException` with the message `"NotBeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality."` when `T` is a value type, before performing any reference comparison.
- **FR-005**: The failure message produced by FR-003 MUST include the formatted `because` / `becauseArgs` reason when a non-empty `because` is supplied.
- **FR-006**: `NotBeSameAs` MUST treat two `null` references as the same reference (assertion fails), consistent with `ReferenceEquals(null, null) == true`.
- **FR-007**: `NotBeSameAs` MUST treat a `null` subject paired with a non-null `unexpected` as different references (assertion passes).

### Key Entities *(include if feature involves data)*

- **`ObjectAssertions<T>`**: The fluent assertion context for arbitrary objects; `NotBeSameAs` is added here alongside `BeSameAs`.
- **`AndConstraint<ObjectAssertions<T>>`**: The return type enabling further chaining after a passing assertion.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All six acceptance scenarios listed above pass as automated tests with no regressions in existing tests.
- **SC-002**: Calling `NotBeSameAs` with a `because` string produces a failure message that contains the supplied reason text — verifiable by reading the exception message.
- **SC-003**: Attempting to use `NotBeSameAs` on a value type produces an immediately actionable error message without any ambiguity about the correct alternative.
- **SC-004**: The returned `AndConstraint` allows at least one additional chained assertion call, confirming the fluent chain is unbroken.

## Assumptions

- `NotBeSameAs` follows the same guard-before-comparison ordering as `BeSameAs` — value-type check runs first, reference comparison second.
- The `because` / `becauseArgs` formatting uses the same helper already used by all other assertion methods in the library (no new infrastructure needed).
- No overload with a custom comparer is required — reference identity is unambiguous and non-configurable.
- The implementation targets the same .NET surface as the existing `ObjectAssertions<T>` class; no multi-targeting changes are required.
- `public-api.md` is updated to document the new method as part of the `ObjectAssertions<T>` contract.
