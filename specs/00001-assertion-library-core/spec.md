# Feature Specification: Assertion Library Core

**Feature Branch**: `00001-assertion-library-core`
**Created**: 2026-03-31
**Status**: Accepted
**Input**: Fluent, strongly-typed assertion library for .NET test suites with `.Should()` entry-point pattern, typed assertion objects, and chainable constraint methods.

## Clarifications

### Session 2026-03-31

- Q: Should string comparisons (`Be`, `Contain`, `NotContain`) default to case-sensitive or case-insensitive? → A: Case-sensitive (ordinal) by default. Case-insensitive overloads accepting `StringComparison` are deferred to a future feature.
- Q: Should equality/comparison methods accept optional custom comparer parameters (`IEqualityComparer<T>`, `IComparer<T>`) in this feature? → A: Yes, add optional comparer parameters to all equality and comparison methods now.
- Q: What matching semantics should `.WithMessage(pattern)` use — exact, substring, or wildcard? → A: Substring (contains) match by default.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Write Simple Value Assertions (Priority: P1)

A developer writing a unit test wants to assert that a computed value equals an expected result using natural-language syntax. They call `.Should().Be(expected)` on a value of any supported primitive type (`bool`, `int`, `long`, `string`) and receive a clear pass or a descriptive failure message.

**Why this priority**: This is the foundational use case. Without value equality assertions, no other assertion category is useful. Every test suite starts here.

**Independent Test**: Can be fully tested by asserting primitive values (`int`, `string`, `bool`) against expected results and verifying both passing and failing paths produce correct outcomes and messages.

**Acceptance Scenarios**:

1. **Given** an integer variable with value `42`, **When** the developer writes `.Should().Be(42)`, **Then** the assertion passes silently with no exception thrown.
2. **Given** an integer variable with value `42`, **When** the developer writes `.Should().Be(99)`, **Then** the assertion fails with a message containing both the expected value (`99`) and the actual value (`42`).
3. **Given** a string variable with value `"hello"`, **When** the developer writes `.Should().Be("hello")`, **Then** the assertion passes.
4. **Given** a boolean variable with value `true`, **When** the developer writes `.Should().BeTrue()`, **Then** the assertion passes.
5. **Given** a boolean variable with value `true`, **When** the developer writes `.Should().BeFalse()`, **Then** the assertion fails with a descriptive message.
6. **Given** a null reference, **When** the developer writes `.Should().BeNull()`, **Then** the assertion passes.
7. **Given** a non-null reference, **When** the developer writes `.Should().NotBeNull()`, **Then** the assertion passes.
8. **Given** any assertion call with a `because` reason string, **When** the assertion fails, **Then** the failure message includes the reason appended to the structured output.

---

### User Story 2 — Assert String Properties (Priority: P1)

A developer wants to verify string content beyond exact equality — checking that a string contains a substring, does not contain sensitive data, or is not null/empty.

**Why this priority**: String assertions are among the most frequently used in any test suite and are needed from day one alongside value equality.

**Independent Test**: Can be tested by asserting various string subjects against `Contain`, `NotContain`, `NotBeNullOrEmpty`, and `Be`, verifying correct pass/fail behavior and messages.

**Acceptance Scenarios**:

1. **Given** a string `"hello world"`, **When** the developer writes `.Should().Contain("world")`, **Then** the assertion passes.
2. **Given** a string `"hello world"`, **When** the developer writes `.Should().Contain("mars")`, **Then** the assertion fails with a message showing the actual string and the missing substring.
3. **Given** a string `"safe content"`, **When** the developer writes `.Should().NotContain("secret")`, **Then** the assertion passes.
4. **Given** a string `"has secret inside"`, **When** the developer writes `.Should().NotContain("secret", "credentials must not be logged")`, **Then** the assertion fails and the message includes "credentials must not be logged".
5. **Given** a non-empty string, **When** the developer writes `.Should().NotBeNullOrEmpty()`, **Then** the assertion passes.
6. **Given** a null string, **When** the developer writes `.Should().NotBeNullOrEmpty()`, **Then** the assertion fails.
7. **Given** an empty string `""`, **When** the developer writes `.Should().NotBeNullOrEmpty()`, **Then** the assertion fails.

---

### User Story 3 — Assert Numeric Comparisons (Priority: P2)

A developer wants to verify that a numeric value meets comparison constraints — greater than or equal to a threshold, or less than an upper bound.

**Why this priority**: Numeric comparison assertions are common but build on the same core engine as value equality. They extend the value assertion surface.

**Independent Test**: Can be tested by asserting `int` and `long` values against `BeGreaterThanOrEqualTo` and `BeLessThan` with both passing and failing inputs.

**Acceptance Scenarios**:

1. **Given** an integer with value `10`, **When** the developer writes `.Should().BeGreaterThanOrEqualTo(10)`, **Then** the assertion passes.
2. **Given** an integer with value `10`, **When** the developer writes `.Should().BeGreaterThanOrEqualTo(11)`, **Then** the assertion fails with a message showing `10` is not >= `11`.
3. **Given** an integer with value `5`, **When** the developer writes `.Should().BeLessThan(10)`, **Then** the assertion passes.
4. **Given** an integer with value `10`, **When** the developer writes `.Should().BeLessThan(10)`, **Then** the assertion fails.

---

### User Story 4 — Assert Collection Contents (Priority: P2)

A developer wants to verify the contents, count, and structure of collections — checking element count, presence of specific items, emptiness, equivalence (order-independent), and that all elements satisfy a condition.

**Why this priority**: Collection assertions are the second most common assertion category after value/string assertions and are essential for verifying any operation that produces multiple results.

**Independent Test**: Can be tested by asserting against `List<T>` and `IEnumerable<T>` instances with `HaveCount`, `Contain`, `BeEmpty`, `ContainSingle`, `BeEquivalentTo`, and `AllSatisfy`.

**Acceptance Scenarios**:

1. **Given** a list with 3 elements, **When** the developer writes `.Should().HaveCount(3)`, **Then** the assertion passes.
2. **Given** a list with 3 elements, **When** the developer writes `.Should().HaveCount(5)`, **Then** the assertion fails showing expected count `5` vs actual `3`.
3. **Given** a list containing `"apple"`, **When** the developer writes `.Should().Contain("apple")`, **Then** the assertion passes.
4. **Given** an empty list, **When** the developer writes `.Should().BeEmpty()`, **Then** the assertion passes.
5. **Given** a list with one element, **When** the developer writes `.Should().ContainSingle()`, **Then** the assertion passes and `.Which` exposes the single element for further assertions.
6. **Given** a list `["b", "a", "c"]`, **When** the developer writes `.Should().BeEquivalentTo("a", "b", "c")`, **Then** the assertion passes (order-independent).
7. **Given** a list of objects all having `Type == "OrderPlaced"`, **When** the developer writes `.Should().AllSatisfy(m => m.Type.Should().Be("OrderPlaced"))`, **Then** the assertion passes.
8. **Given** a list with two elements where one fails the predicate in `AllSatisfy`, **When** the assertion runs, **Then** it fails with a message identifying which element(s) did not satisfy the condition.

---

### User Story 5 — Assert Dictionary Keys (Priority: P3)

A developer wants to verify that a dictionary contains a specific key.

**Why this priority**: Dictionary assertions are a focused extension of collection assertions, used in configuration and routing scenarios.

**Independent Test**: Can be tested by asserting `IDictionary<K,V>` and `IReadOnlyDictionary<K,V>` subjects against `ContainKey`.

**Acceptance Scenarios**:

1. **Given** a dictionary with key `"host"`, **When** the developer writes `.Should().ContainKey("host")`, **Then** the assertion passes.
2. **Given** a dictionary without key `"missing"`, **When** the developer writes `.Should().ContainKey("missing")`, **Then** the assertion fails with a message listing the available keys.

---

### User Story 6 — Assert Synchronous Exceptions (Priority: P2)

A developer wants to verify that an `Action` throws a specific exception type when invoked, and then inspect properties of the caught exception.

**Why this priority**: Exception assertions are critical for negative-path testing and validating error handling logic.

**Independent Test**: Can be tested by wrapping throwing and non-throwing actions in `.Should().Throw<T>()` and verifying both exception type matching and `.Which` property drill-down.

**Acceptance Scenarios**:

1. **Given** an `Action` that throws `ArgumentNullException`, **When** the developer writes `.Should().Throw<ArgumentNullException>()`, **Then** the assertion passes.
2. **Given** an `Action` that throws `ArgumentNullException`, **When** the developer writes `.Should().Throw<InvalidOperationException>()`, **Then** the assertion fails with a message showing the expected vs actual exception type.
3. **Given** an `Action` that does not throw, **When** the developer writes `.Should().Throw<Exception>()`, **Then** the assertion fails indicating no exception was thrown.
4. **Given** a caught exception from `.Throw<T>()`, **When** the developer accesses `.Which.Message`, **Then** they can chain `.Should().Contain("parameter")` to assert on the exception message.
5. **Given** a caught exception, **When** the developer accesses a custom property via `.Which.PropertyName`, **Then** they can assert on that property value with `.Should().Be(expected)`.

---

### User Story 7 — Assert Asynchronous Exceptions (Priority: P3)

A developer wants to verify that a `Func<Task>` throws a specific exception when awaited, using `await .Should().ThrowAsync<T>()`.

**Why this priority**: Async exception testing is essential for modern async codebases but builds on the synchronous exception pattern.

**Independent Test**: Can be tested by wrapping async lambdas in `.Should().ThrowAsync<T>()` and verifying exception capture and `.Which` drill-down.

**Acceptance Scenarios**:

1. **Given** a `Func<Task>` that throws `InvalidOperationException` asynchronously, **When** the developer writes `await act.Should().ThrowAsync<InvalidOperationException>()`, **Then** the assertion passes.
2. **Given** a `Func<Task>` that completes without throwing, **When** the developer writes `await act.Should().ThrowAsync<Exception>()`, **Then** the assertion fails.
3. **Given** the result of `ThrowAsync<T>()`, **When** the developer accesses `.Which`, **Then** they can inspect exception properties with further `.Should()` chains.

---

### User Story 8 — Chain Assertions Fluently (Priority: P1)

A developer wants to chain multiple assertions on the same subject using `.And`, or drill into a nested subject using `.Which`, without repeating the subject expression.

**Why this priority**: Fluent chaining is core to the library's identity and readability promise. It must work from the start.

**Independent Test**: Can be tested by writing multi-step chains using `.And` and `.Which` and verifying each link in the chain executes its assertion correctly.

**Acceptance Scenarios**:

1. **Given** a collection with one element, **When** the developer writes `.Should().ContainSingle().Which.Name.Should().Be("Alice")`, **Then** the assertion drills into the single element and verifies the `Name` property.
2. **Given** a caught exception, **When** the developer writes `.Should().Throw<T>().And` followed by another assertion, **Then** both assertions run against the exception scope.
3. **Given** a `BeOfType<T>()` assertion that passes, **When** the developer accesses `.Which`, **Then** they receive the subject cast to `T` for further `.Should()` chains.

---

### User Story 9 — Type Assertions (Priority: P3)

A developer wants to verify that a runtime object is of an exact type, then optionally drill into the typed object.

**Why this priority**: Type assertions are a niche but important category for polymorphic scenarios. They complement object assertions.

**Independent Test**: Can be tested by asserting objects against `BeOfType<T>()` and verifying both exact-match and mismatch behavior.

**Acceptance Scenarios**:

1. **Given** an object that is exactly `ArgumentNullException`, **When** the developer writes `.Should().BeOfType<ArgumentNullException>()`, **Then** the assertion passes.
2. **Given** an object that is `ArgumentNullException`, **When** the developer writes `.Should().BeOfType<Exception>()`, **Then** the assertion fails (exact type match, not inheritance).
3. **Given** a passing `BeOfType<T>()`, **When** the developer accesses `.Which`, **Then** they receive the object typed as `T` for further assertions.

---

### Edge Cases

- What happens when `.Should()` is called on a `null` subject for collection assertions? The assertion must fail with a clear message ("Expected collection, but found null") rather than throwing `NullReferenceException`.
- What happens when `ContainSingle()` is called on a collection with zero elements? The assertion must fail stating "Expected exactly 1 element, but found 0."
- What happens when `ContainSingle()` is called on a collection with more than one element? The assertion must fail stating the actual count.
- What happens when `ContainSingle(predicate)` matches zero elements? Must fail indicating no elements matched.
- What happens when `ContainSingle(predicate)` matches more than one element? Must fail indicating the number of matching elements.
- What happens when `BeEquivalentTo` is called with duplicate items? Element frequency must be compared — `[1, 1, 2]` is not equivalent to `[1, 2, 2]`.
- What happens when `AllSatisfy` is called on an empty collection? The assertion passes vacuously (universal quantifier over empty set is true).
- What happens when `Throw<T>()` catches an exception that is a subclass of `T`? Must pass if the thrown exception is exactly `T` or a subclass of `T`.
- What happens when `ThrowAsync<T>()` catches an `AggregateException` wrapping `T`? Must unwrap and match the inner exception.
- What happens when `BeSameAs` is called with value types? Must throw `InvalidOperationException` at runtime with message "BeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality." A compile-time constraint is not feasible because ObjectAssertions<T> must remain unconstrained.
- What happens when the `because` parameter contains format placeholders and `becauseArgs` are supplied? Must format the reason string correctly.
- What happens when `Be(expected)` is called with `null` as the expected value? Must behave identically to `BeNull()`.
- What happens when `.WithMessage("")` is called with an empty string? Must pass (every string contains the empty string).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST provide a `.Should()` extension method as the sole entry point for all assertions, resolved by the static type of the subject to return the appropriate typed assertion object.
- **FR-002**: The library MUST provide `ObjectAssertions` supporting `Be` (with optional `IEqualityComparer<T>`), `BeSameAs`, `BeNull`, `NotBeNull`, and `BeOfType<T>`.
- **FR-003**: The library MUST provide `BooleanAssertions` supporting `BeTrue` and `BeFalse`.
- **FR-004**: The library MUST provide `NumericAssertions<T>` supporting `Be` (with optional `IEqualityComparer<T>`), `BeGreaterThanOrEqualTo` (with optional `IComparer<T>`), and `BeLessThan` (with optional `IComparer<T>`) for `int` and `long` subjects.
- **FR-005**: The library MUST provide `StringAssertions` supporting `Be`, `Contain`, `NotContain`, `NotBeNullOrEmpty`, and `BeEmpty`. All string comparisons MUST use `StringComparison.Ordinal` (case-sensitive) by default.
- **FR-006**: The library MUST provide `GenericCollectionAssertions<T>` for `IEnumerable<T>` supporting `HaveCount`, `ContainSingle` (with and without predicate), `Contain` (with optional `IEqualityComparer<T>`), `BeEmpty`, `BeEquivalentTo` (with optional `IEqualityComparer<T>`), and `AllSatisfy`.
- **FR-007**: The library MUST provide `GenericDictionaryAssertions<K,V>` for `IDictionary<K,V>` and `IReadOnlyDictionary<K,V>` supporting `ContainKey`.
- **FR-008**: The library MUST provide `ActionAssertions` for `Action` subjects supporting `Throw<TException>`.
- **FR-009**: The library MUST provide `AsyncFunctionAssertions` for `Func<Task>` subjects supporting `ThrowAsync<TException>`, returning `Task<ExceptionAssertions<TException>>`.
- **FR-010**: Every assertion method MUST return `AndConstraint<TAssertions>` (providing `.And`) or `AndWhichConstraint<TAssertions, TSubject>` (providing `.And` and `.Which`) to enable fluent chaining.
- **FR-011**: `ExceptionAssertions<TException>` MUST expose `.Which` (the caught exception), `.And` (continue asserting), and `.WithMessage(expectedSubstring)` (assert that `Exception.Message` contains `expectedSubstring` using ordinal case-sensitive substring matching).
- **FR-012**: Every assertion method MUST accept an optional `string because` parameter (and optional `params object[] becauseArgs`). On failure, the formatted reason MUST be appended to the structured failure message.
- **FR-013**: On assertion failure, the library MUST throw a descriptive exception containing: the expected value/condition, the actual value/condition, the subject expression captured via [CallerArgumentExpression] (when available), and a human-readable mismatch description.
- **FR-014**: The library MUST define its own failure exception type and provide a pluggable failure-reporting mechanism so framework adapters can bridge failures to any test runner.
- **FR-015**: The library MUST NOT depend on any specific test runner or third-party assertion package. Zero external dependencies for the core library.
- **FR-016**: `ContainSingle()` on `IEnumerable<T>` MUST infer `T` so that `.Which` exposes typed properties without requiring a cast.
- **FR-017**: `BeEquivalentTo` MUST compare elements order-independently, including element frequency (duplicates matter).
- **FR-018**: `AllSatisfy` MUST pass on an empty collection (vacuous truth).
- **FR-019**: `Throw<T>()` MUST match when the thrown exception is exactly `T` or a subclass of `T`.
- **FR-020**: `ThrowAsync<T>()` MUST unwrap `AggregateException` to match the inner exception against `T`.
- **FR-021**: Assertion chains MUST be immutable — each chained call returns a new context with no shared mutable state.
- **FR-022**: All assertion methods MUST be safe for concurrent use from parallel test runners.
- **FR-023**: Internal assertion engine methods (failure reporting, message formatting) MUST apply `[StackTraceHidden]` so that assertion failure stack traces point directly to the test code, not to internal library frames.

### Key Entities

- **Assertion Subject**: The value or object under test. Provided as the receiver of `.Should()`. Its static type determines which assertion class is returned.
- **Assertion Object**: A typed object (e.g., `StringAssertions`, `NumericAssertions<T>`) returned by `.Should()`, exposing only the constraint methods valid for the subject type.
- **AndConstraint**: A continuation object providing `.And` to chain further assertions on the same parent scope. Terminal for most assertions.
- **AndWhichConstraint**: A continuation object providing both `.And` (parent scope) and `.Which` (extracted inner subject for drill-down).
- **ExceptionAssertions**: A specialized result from `Throw<T>()` / `ThrowAsync<T>()` providing `.Which` (the caught exception), `.And`, and `.WithMessage()`.
- **Failure Exception**: The library's own exception type thrown on assertion failure, containing structured failure data (expected, actual, reason, expression).
- **Framework Adapter**: A pluggable component that bridges the library's failure exception to a specific test framework's assertion failure mechanism.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can go from package install to writing and executing their first passing assertion in under 60 seconds.
- **SC-002**: Every assertion method in the catalog (25 methods across 7 assertion categories) is implemented and has at least one positive test, one negative test, and one edge-case test.
- **SC-003**: Assertion failure messages always contain both expected and actual values, plus any user-supplied reason — validated by negative tests inspecting message content.
- **SC-004**: Fluent chains of at least 3 links (e.g., `.Should().ContainSingle().Which.Name.Should().Be("x")`) execute correctly in all supported scenarios.
- **SC-005**: The library can be used in a test project without referencing any specific test framework package — verified by a test project that uses only the assertion library and a generic test host.
- **SC-006**: Simple value assertions on the happy path (assertion passes) are zero-allocation — verified by allocation benchmarks.
- **SC-007**: 95% line coverage and 90% branch coverage for the core assertion engine; 90% line coverage for extension method surfaces.

## Assumptions

- Target users are .NET developers writing unit tests who are familiar with fluent assertion patterns (e.g., FluentAssertions, Shouldly).
- The initial release targets .NET 10 (single TFM) per user direction. This overrides the constitution's broadest-surface guidance; multi-targeting may be added in a future feature.
- `BeSameAs` is only meaningful for reference types; the library will use generic constraints or Roslyn analyzers to prevent misuse with value types.
- `Throw<T>()` matches both the exact type `T` and its derived types, consistent with standard `catch(T)` semantics.
- `ThrowAsync<T>()` unwraps `AggregateException` to extract the first inner exception for type matching, consistent with `await` exception propagation behavior.
- `AllSatisfy` on an empty collection passes (vacuous truth), consistent with LINQ's `All()` behavior.
- `BeEquivalentTo` respects element frequency — `[1, 1, 2]` is NOT equivalent to `[1, 2]` or `[1, 2, 2]`.
- The `because` parameter uses `string.Format`-style formatting when `becauseArgs` are provided.
- All string assertion methods (`Be`, `Contain`, `NotContain`) use `StringComparison.Ordinal` (case-sensitive) by default. Case-insensitive overloads accepting a `StringComparison` parameter are deferred to a future feature.
- Per constitution API Design Rule 8, all equality methods (`Be`, `Contain`, `BeEquivalentTo`) accept an optional `IEqualityComparer<T>` parameter and all comparison methods (`BeGreaterThanOrEqualTo`, `BeLessThan`) accept an optional `IComparer<T>` parameter. When not supplied, the library uses the default equality/comparison semantics for the type.
- The Roslyn analyzer (misuse warnings) and test-framework adapters are separate deliverables outside the scope of this core library feature, but the core must expose the extension points they require.
- A comprehensive BenchmarkDotNet benchmark suite is a separate deliverable. However, this feature includes a minimal allocation spot-check to validate SC-006 (zero-allocation happy path for value assertions).
