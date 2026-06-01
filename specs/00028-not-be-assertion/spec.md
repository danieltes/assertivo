# Feature Specification: NotBe Inequality Assertion

**Feature Branch**: `00028-not-be-assertion`  
**Created**: 2026-05-31  
**Status**: Ready for Implementation  
**Input**: User description: "Add NotBe inequality assertion to ObjectAssertions, NumericAssertions, and StringAssertions"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Assert That a Value Does Not Equal an Expected Value (Priority: P1)

A developer writing a unit test wants to assert that a value is *not* equal to a specific value — for example, verifying that a mutated ID changed, a counter incremented, or that two objects are not equal by value. They call `.Should().NotBe(unexpected)` and receive a clear pass or a descriptive failure message.

**Why this priority**: This is the core use case. The `NotBe` method on all three assertion types is the direct symmetric counterpart of `Be`, and all other stories depend on this foundational behaviour being correct.

**Independent Test**: Can be fully tested by calling `NotBe` with matching and non-matching values on `int`, `long`, `string`, and object types and verifying both passing and failing paths produce correct outcomes.

**Acceptance Scenarios**:

1. **Given** an integer with value `42`, **When** the developer writes `.Should().NotBe(99)`, **Then** the assertion passes silently with no exception thrown.
2. **Given** an integer with value `42`, **When** the developer writes `.Should().NotBe(42)`, **Then** the assertion throws `AssertionFailedException` with a message containing `not 42` and `42`.
3. **Given** a string with value `"hello"`, **When** the developer writes `.Should().NotBe("world")`, **Then** the assertion passes.
4. **Given** a string with value `"hello"`, **When** the developer writes `.Should().NotBe("hello")`, **Then** the assertion fails with a message indicating the subject was unexpectedly equal to `"hello"`.
5. **Given** any supported type, **When** the assertion fails with a `because` reason, **Then** the failure message includes the formatted reason.
6. **Given** any passing `NotBe` call, **When** the result is used for chaining, **Then** it returns an `AndConstraint` that allows further assertions on the same subject — for example, `42.Should().NotBe(99).And.Be(42)` completes without exception.
7. **Given** a `long` with value `100L`, **When** the developer writes `.Should().NotBe(200L)`, **Then** the assertion passes silently.

---

### User Story 2 — Assert Inequality with a Custom Equality Comparer (Priority: P2)

A developer has a custom equality rule (e.g., case-insensitive string identity, domain-specific value object equality) and wants `NotBe` to respect it. They pass an `IEqualityComparer<T>` to `ObjectAssertions<T>.NotBe` or `NumericAssertions<T>.NotBe` to override default equality.

**Why this priority**: Custom comparers are already supported on `Be` for the same types. Symmetric support in `NotBe` is required for consistency and enables all the same use cases where custom equality semantics matter.

**Independent Test**: Can be tested independently by passing a custom `IEqualityComparer<T>` that considers two non-identical values equal (e.g., case-insensitive comparer for a string-keyed object) and verifying the assertion fails when the comparer reports equality and passes when it reports inequality.

**Acceptance Scenarios**:

1. **Given** two values that are unequal by default but equal by a custom comparer, **When** `NotBe` is called with that comparer, **Then** the assertion fails (the comparer rules).
2. **Given** two values that are equal by default but unequal by a custom comparer, **When** `NotBe` is called with that comparer, **Then** the assertion passes (the comparer rules).
3. **Given** `NotBe` is called on `ObjectAssertions<T>` or `NumericAssertions<T>` with `comparer: null`, **When** the assertion runs, **Then** the default equality comparer for `T` is used.

---

### User Story 3 — Assert Inequality with Null Subjects and Unexpected Values (Priority: P2)

A developer wants to assert inequality involving `null` — for example, confirming that a result is not `null`, or that a `null` subject is not equal to some non-null value.

**Why this priority**: Null handling is a common source of bugs and test gaps. The `Be` counterpart on `ObjectAssertions<T>` and `StringAssertions` already handles null subjects; `NotBe` must handle the same cases consistently.

**Independent Test**: Can be tested by calling `NotBe` with null subjects, null unexpected values, and combinations of both on `ObjectAssertions<T>` and `StringAssertions`, verifying the pass/fail behaviour matches what `!object.Equals(subject, unexpected)` would produce.

**Acceptance Scenarios**:

1. **Given** a null `string` subject, **When** `NotBe("something")` is called, **Then** the assertion passes (null ≠ "something").
2. **Given** a null `string` subject, **When** `NotBe(null)` is called, **Then** the assertion fails (null = null).
3. **Given** a non-null `string` subject `"hello"`, **When** `NotBe(null)` is called, **Then** the assertion passes.
4. **Given** a reference-type `ObjectAssertions<T>` subject that is null, **When** `NotBe(null)` is called, **Then** the assertion fails.

---

### Edge Cases

- What happens when `NotBe` is called with the exact same value as the subject? → Assertion fails with a clear message showing the unexpected value and the found value.
- What happens when both subject and unexpected are `null` for `StringAssertions`? → Assertion fails (null equals null by ordinal/reference semantics).
- What happens when a custom comparer throws an exception? → The exception propagates as-is to the caller with its original type and message preserved; no wrapping, rethrowing, or special handling occurs inside the assertion method.
- What happens when `becauseArgs` is provided but `because` is an empty string? → The `because` message is omitted from the failure output (consistent with `Be` behaviour).
- What happens when `because` contains format placeholders (e.g., `"result must differ from {0}"`) and `becauseArgs` is non-empty? → The `because` string is formatted via `string.Format(because, becauseArgs)` before being appended to the failure message, consistent with the existing `Be` behaviour.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The assertion library MUST expose a `NotBe` method on `ObjectAssertions<T>` that accepts an unexpected value, an optional equality comparer, an optional because string, and optional because args.
- **FR-002**: The assertion library MUST expose a `NotBe` method on `NumericAssertions<T>` that accepts an unexpected value, an optional equality comparer, an optional because string, and optional because args.
- **FR-003**: The assertion library MUST expose a `NotBe` method on `StringAssertions` that accepts a nullable unexpected string, an optional because string, and optional because args.
- **FR-004**: `NotBe` MUST pass (return without throwing) when the subject does not equal the unexpected value.
- **FR-005**: `NotBe` MUST fail with an `AssertionFailedException` when the subject equals the unexpected value.
- **FR-006**: The failure message for `NotBe` MUST identify both the unexpected value (prefixed with `not`) and the actual found value so the developer can understand the violation.
- **FR-007**: When a `because` reason is supplied and the assertion fails, the failure message MUST include the formatted reason.
- **FR-008**: `NotBe` on `ObjectAssertions<T>` and `NumericAssertions<T>` MUST use `EqualityComparer<T>.Default` when no comparer is provided.
- **FR-009**: `NotBe` on `StringAssertions` MUST use ordinal (case-sensitive) string comparison, consistent with the existing `Be` method.
- **FR-010**: All three `NotBe` methods MUST return an `AndConstraint` wrapping the same assertion object, enabling call chaining.
- **FR-011**: All three `NotBe` methods MUST carry XML `<summary>` documentation comments following the style established by the existing `Be` methods.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All cases where subjects are equal to the unexpected value produce a failing assertion with a descriptive message — 100% of such cases caught.
- **SC-002**: All cases where subjects differ from the unexpected value complete without error — 100% pass rate.
- **SC-003**: Developers can chain further assertions after `NotBe` without additional boilerplate — chaining works in a single expression.
- **SC-004**: The failure message is human-readable and identifies the unexpected value and the actual found value without requiring the developer to inspect source code.
- **SC-005**: Custom equality comparers on `ObjectAssertions<T>` and `NumericAssertions<T>` are fully respected — 100% of comparer-influenced pass/fail outcomes match the comparer's semantics.

## Assumptions

- The three assertion types targeted (`ObjectAssertions<T>`, `NumericAssertions<T>`, `StringAssertions`) already exist in the codebase with a working `Be` method; `NotBe` mirrors that structure.
- `StringAssertions.NotBe` does not need a `StringComparison` parameter — ordinal (case-sensitive) comparison is the established default in this library. Case-insensitive variants are deferred to a future feature.
- `NumericAssertions<T>` has the same shape as `ObjectAssertions<T>` with respect to comparer support; no numeric-specific comparison semantics (e.g., `IComparer<T>`) are needed for inequality.
- Failure reporting continues to use the existing `MessageFormatter.Fail` path and `AssertionConfiguration.ReportFailure` hook, with no changes to the failure infrastructure.
- XML documentation comments (`<summary>`) follow the existing style used on `Be` and other assertion methods.
