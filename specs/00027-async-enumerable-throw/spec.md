# Feature Specification: ThrowAsync Support for IAsyncEnumerable<T> Subjects

**Feature Branch**: `00027-async-enumerable-throw`  
**Created**: 2026-05-23  
**Status**: Draft  
**Input**: User description: "Add ThrowAsync support for IAsyncEnumerable<T> subjects"

## Clarifications

### Session 2026-05-27

- Q: When `DisposeAsync` throws after the iterator has already thrown during enumeration, which exception should `ThrowAsync` evaluate for type-matching and return via `.Which`? → A: Preserve the original enumeration exception — capture it before disposal runs and re-throw it even if `DisposeAsync` also throws; the disposal exception is discarded.
- Q: Should `ThrowAsync<TException>()` accept an optional `CancellationToken` parameter? → A: No — omit `CancellationToken`; keep the signature consistent with `AsyncFunctionAssertions.ThrowAsync` and `TaskAssertions.ThrowAsync`. Test-level timeout control is the caller’s responsibility.
- Q: Should `AsyncEnumerableAssertions<T>.ThrowAsync` failure messages use the same wording templates as `TaskAssertions.ThrowAsync`? → A: Yes — same templates, substituting domain nouns (e.g., “source” in place of “task”); consistent phrasing reduces cognitive load and allows shared assertion patterns in library tests.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert Async Iterator Throws Expected Exception (Priority: P1)

A developer writing a test has an `IAsyncEnumerable<T>` source — typically an async iterator method used in a streaming pipeline, channel reader, or server-sent-event feed — and wants to assert that it throws a specific exception type during enumeration. Rather than manually writing an `await foreach` try/catch block, they call `.Should()` directly on the source and chain `.ThrowAsync<TException>()`.

**Why this priority**: This is the core capability that unlocks testability for async iterator subjects. Async iterators defer exceptions until enumeration starts, making them impossible to test with the existing `Func<Task>` path. All other user stories depend on this foundation working correctly.

**Independent Test**: Can be fully validated by calling `.Should()` on an `IAsyncEnumerable<int>` that throws `InvalidOperationException` after yielding one item and asserting that `.ThrowAsync<InvalidOperationException>()` passes. Delivers the primary value on its own.

**Acceptance Scenarios**:

1. **Given** an async iterator that throws `InvalidOperationException` after yielding one item, **When** `await source.Should().ThrowAsync<InvalidOperationException>()` is written, **Then** the assertion passes.
2. **Given** an async iterator that throws a subtype of the asserted exception type, **When** `.ThrowAsync<Exception>()` is awaited, **Then** the assertion passes (subtype matching).
3. **Given** an async iterator that completes without throwing, **When** `.ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion fails with an `AssertionFailedException` stating that an exception was expected but none was thrown.
4. **Given** an async iterator that throws `ArgumentException`, **When** `.ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion fails with an `AssertionFailedException` that includes both the expected and actual exception type names, plus the actual exception's message text.
5. **Given** an async iterator that wraps its fault in an `AggregateException` with a single inner `InvalidOperationException`, **When** `.ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion passes (unwrapping parity with `AsyncFunctionAssertions`).

---

### User Story 2 - Chain Further Assertions on the Caught Exception (Priority: P2)

After asserting that an async iterator threw the expected exception type, a developer wants to inspect properties of the caught exception — such as its message or inner exception — without re-enumerating the source or capturing the exception separately.

**Why this priority**: Exception chaining via `.Which` is a core Assertivo pattern that keeps tests expressive and avoids manual try/catch boilerplate. Without it, developers gain type assertion but lose the ability to verify exception details.

**Independent Test**: Can be fully validated by calling `.ThrowAsync<InvalidOperationException>()` on a source that throws with a known message, then accessing `.Which.Message` on the result and asserting its content with `.Should().Contain(...)`.

**Acceptance Scenarios**:

1. **Given** the result of a successful `ThrowAsync<TException>()` call, **When** `.Which` is accessed, **Then** it provides the caught exception instance for further assertion chaining.
2. **Given** a source that faults via an unwrapped `AggregateException` with a single inner exception carrying a known message, **When** `.ThrowAsync<TException>()` completes and `.Which.Message` is inspected, **Then** the message reflects the inner exception's message.

---

### User Story 3 - Provide Contextual Failure Messages with Because (Priority: P3)

A developer wants failure messages to include test-context information explaining why a particular exception was expected, so that CI output is immediately understandable without reading the surrounding test code.

**Why this priority**: Diagnostic quality is valuable but the feature is usable without it. The `because`/`becauseArgs` pattern is consistent with all other assertion methods in the library and should be supported for API completeness.

**Independent Test**: Can be fully validated by calling `.ThrowAsync<TException>("because {0} is required", "validation")` on a source that does not throw, and confirming the failure message includes the formatted because phrase.

**Acceptance Scenarios**:

1. **Given** an async iterator that completes without throwing, **When** `.ThrowAsync<InvalidOperationException>("because {0} is required", "validation")` is awaited, **Then** the `AssertionFailedException` message contains the formatted because phrase.
2. **Given** an async iterator that throws the wrong exception type, **When** `.ThrowAsync<TException>("because of contract X")` is awaited, **Then** the failure message includes "because of contract X".

---

### Edge Cases

- What happens when the `IAsyncEnumerable<T>` subject is `null`? The assertion MUST fail with a descriptive `AssertionFailedException` rather than throw a `NullReferenceException`. The check is performed before enumeration begins.
- What happens when the async iterator throws on the very first `MoveNextAsync()` call, before any item is yielded? The exception MUST be caught and the assertion evaluated normally — the enumerable need not yield any items for the assertion to work.
- What happens when the iterator throws an `AggregateException` with multiple inner exceptions? The multi-inner case MUST NOT be unwrapped; the `AggregateException` is treated as the caught exception itself. If `AggregateException` is not the asserted type, the assertion fails.
- What happens when the iterator throws an `AggregateException` with zero inner exceptions? Same as the multi-inner case — the single-inner condition is not met, so the `AggregateException` is not unwrapped.
- What happens when the `IAsyncEnumerable<T>` yields many items before eventually throwing? The assertion MUST drain all items until the exception is caught or the sequence ends. Yielded items are discarded; only the fault state matters.
- What happens when the iterator never throws and yields an arbitrarily long sequence? The assertion MUST enumerate to completion and then fail with a "no exception thrown" message. The sequence is drained fully.
- What happens when `DisposeAsync` throws after the iterator has already thrown during enumeration? The original enumeration exception is preserved. `ThrowAsync` captures the enumeration exception before disposal runs and re-throws it even if `DisposeAsync` also throws; the disposal exception is discarded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST expose a `.Should()` extension method on `IAsyncEnumerable<T>?` (nullable) subjects that returns an `AsyncEnumerableAssertions<T>` value, accepting a `[CallerArgumentExpression]`-sourced caller expression string for diagnostic output.
- **FR-002**: `AsyncEnumerableAssertions<T>` MUST expose an async `ThrowAsync<TException>()` method that enumerates the subject `IAsyncEnumerable<T>` to completion and passes when an exception is thrown during enumeration that is `TException` or a subtype of `TException`. The method signature MUST NOT include a `CancellationToken` parameter, consistent with the signatures of `AsyncFunctionAssertions.ThrowAsync` and `TaskAssertions.ThrowAsync`; test-level timeout control is the caller’s responsibility.
- **FR-003**: `ThrowAsync<TException>()` MUST drain the enumerable via the manual enumerator pattern (`GetAsyncEnumerator()` / `MoveNextAsync()` with `ConfigureAwait(false)` / `DisposeAsync()` with `ConfigureAwait(false)`), discarding yielded values, and capture any exception thrown during `MoveNextAsync()` for type matching. Enumeration continues until an exception is caught or the sequence ends normally. If `DisposeAsync` also throws after an enumeration exception is already captured, the disposal exception MUST be discarded and the original enumeration exception MUST be used for type-matching and result chaining.
- **FR-004**: `ThrowAsync<TException>()` MUST apply the same `AggregateException` single-inner-exception unwrapping logic used by the existing `Func<Task>` assertion path: if the enumeration faults with an `AggregateException` containing exactly one inner exception, that inner exception is extracted and used for both type-matching and result chaining (`.Which`). An `AggregateException` with zero or more than one inner exception MUST NOT be unwrapped and is treated as the caught exception itself.
- **FR-005**: `ThrowAsync<TException>()` MUST fail with a descriptive `AssertionFailedException` when the sequence completes without throwing. The failure message MUST state that an exception was expected but none was thrown, MUST include the subject expression captured by `[CallerArgumentExpression]`, and MUST follow the same message template as `TaskAssertions.ThrowAsync` for this failure path, substituting “source” for “task” in the wording.
- **FR-006**: `ThrowAsync<TException>()` MUST fail with a descriptive `AssertionFailedException` when the enumeration throws an exception type that is not `TException` or a subtype of `TException`. The failure message MUST include the expected type name, the actual (unwrapped) exception’s type name, and the actual (unwrapped) exception’s `.Message` text. The message MUST follow the same template as `TaskAssertions.ThrowAsync` for this failure path, substituting “source” for “task” in the wording.
- **FR-007**: `ThrowAsync<TException>()` MUST accept `because` and `becauseArgs` parameters and incorporate the formatted reason phrase into any failure message it produces.
- **FR-008**: `ThrowAsync<TException>()` MUST return `ExceptionAssertions<TException>` — the same type already returned by `AsyncFunctionAssertions.ThrowAsync` and `TaskAssertions.ThrowAsync` — ensuring that the post-assertion chaining API (including `.Which`) is identical across all `ThrowAsync` entry points.
- **FR-009**: `ThrowAsync<TException>()` MUST be annotated with `[StackTraceHidden]` so that internal assertion frames do not appear in failure stack traces shown to the developer.
- **FR-010**: `AsyncEnumerableAssertions<T>` MUST be a `readonly struct`, consistent with all other assertion types in the library.
- **FR-011**: A null `IAsyncEnumerable<T>` subject MUST produce a descriptive `AssertionFailedException` from `ThrowAsync` rather than a `NullReferenceException`. The failure message MUST convey the expected value (`source to be non-null`), the actual value (`source was null`), and the subject expression captured by `[CallerArgumentExpression]`, following the same message template as `TaskAssertions.ThrowAsync` for the null-subject path, substituting “source” for “task” in the wording.
- **FR-012**: The existing `.Should()` overloads for `Func<Task>`, `Task`, and all other types MUST remain unaffected; the new `IAsyncEnumerable<T>` overload MUST NOT interfere with any existing overload at compile-time overload resolution.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every acceptance scenario defined in User Stories 1–3 is covered by an automated test that passes without modification to the test.
- **SC-002**: All existing tests for `AsyncFunctionAssertions`, `TaskAssertions`, and every other assertion type continue to pass after the new entry point is added — zero regressions.
- **SC-003**: A developer can assert that an async iterator throws an exception using a single `await` expression with no manual try/catch, `await foreach`, or helper method required in the test body.
- **SC-004**: The post-assertion `.Which` property on the returned `ExceptionAssertions<TException>` provides the same caught exception instance that interrupted enumeration, enabling further assertion chaining without re-enumerating the source.

## Assumptions

- The feature targets developers authoring unit tests for code that produces `IAsyncEnumerable<T>` — including streaming pipelines, channel readers, and server-sent-event feeds.
- Only exception throwing is in scope for this feature. Assertions about yielded values (e.g., sequence equality or element predicates) are out of scope and belong to a separate feature.
- `IAsyncEnumerable<T>` subjects with custom `IAsyncEnumerator<T>` implementations are treated the same as iterator-method-produced enumerables; the enumeration path is identical.
- Cancellation token forwarding (via `WithCancellation`) is out of scope for this feature. The enumerable is drained with the default cancellation token. `ThrowAsync` does not accept a `CancellationToken` parameter; test-level timeout control is the caller’s responsibility (e.g., test framework timeout attributes or `Task.WhenAny`).
