# Feature Specification: Should() Entry Point for Task Subjects

**Feature Branch**: `00026-task-should-assertions`  
**Created**: 2026-05-17  
**Status**: Draft  
**Input**: User description: "Add a Should() entry point for Task subjects so that assertions can be chained directly on an already-started task, without requiring the caller to wrap it in a Func<Task>."

## Clarifications

### Session 2026-05-17

- Q: Should `ThrowAsync<TException>()` return the existing `ExceptionAssertions<TException>` (shared with `AsyncFunctionAssertions`) or a new dedicated type? → A: `ExceptionAssertions<TException>` — reuse the existing shared type.
- Q: When the task faults with the wrong exception type (FR-005), should the failure message include only type names or also the actual exception's `.Message` text? → A: Type names + actual exception `.Message` text — e.g., `"Expected ArgumentNullException but found InvalidOperationException: Value cannot be null"`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert Faulted Task Throws Expected Exception (Priority: P1)

A developer writing a test has a `Task` already in hand — returned directly from the method under test — and wants to assert that the task faults with a specific exception type. Rather than wrapping the call in a `Func<Task>` lambda just to use Assertivo, they call `.Should()` directly on the task and chain `.ThrowAsync<TException>()`.

**Why this priority**: This is the core use case and the primary motivation for the feature. Any other user story depends on this foundation working correctly. Developers encounter the lambda-wrapping ceremony immediately whenever they test async methods, making this the highest-impact improvement.

**Independent Test**: Can be fully validated by calling `.Should()` on a `Task` that faults with `InvalidOperationException` and asserting that `.ThrowAsync<InvalidOperationException>()` passes without any wrapping lambda. Delivers full value on its own.

**Acceptance Scenarios**:

1. **Given** a `Task` that faults with `InvalidOperationException`, **When** `.Should().ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion passes.
2. **Given** a `Task` that faults with a subtype of the asserted exception type, **When** `.Should().ThrowAsync<Exception>()` is awaited, **Then** the assertion passes (subtype matching).
3. **Given** a `Task` that completes successfully without faulting, **When** `.Should().ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion fails with an `AssertionFailedException` that describes the expected exception was not thrown.
4. **Given** a `Task` that faults with `ArgumentException`, **When** `.Should().ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion fails with an `AssertionFailedException` that includes both the expected and actual exception types in its message.
5. **Given** a `Task` that faults wrapped in an `AggregateException` with a single inner `InvalidOperationException`, **When** `.Should().ThrowAsync<InvalidOperationException>()` is awaited, **Then** the assertion passes (matching existing `AsyncFunctionAssertions` unwrapping behaviour).
6. **Given** a cancelled `Task` (e.g., created via `Task.FromCanceled`), **When** `.Should().ThrowAsync<OperationCanceledException>()` is awaited, **Then** the assertion passes — cancellation is treated as a fault like any other, and `TaskCanceledException` (a subtype of `OperationCanceledException`) satisfies subtype matching per FR-002.
7. **Given** a `Task<int>` variable declared at compile time as `Task<int>` (not widened to `Task`), **When** `.Should()` is called, **Then** the method returns `TaskAssertions` (not `ObjectAssertions<Task<int>>`), confirming that `Task<T>` resolves to the new overload via inheritance with no separate overload required.

---

### User Story 2 - Chain Further Assertions on the Caught Exception (Priority: P2)

After asserting that a task threw the expected exception type, a developer wants to inspect properties of the caught exception — such as its message or inner exception — without re-executing the task or capturing the exception separately.

**Why this priority**: Exception chaining via `.Which` is a core Assertivo pattern that makes tests expressive and avoids repetition. Without it, developers must catch exceptions manually in separate try/catch blocks to inspect them, defeating the ergonomic goal.

**Independent Test**: Can be fully validated by asserting `.ThrowAsync<InvalidOperationException>()` on a faulting task, then accessing `.Which.Message` on the result and asserting its content with `.Should().Contain(...)`.

**Acceptance Scenarios**:

1. **Given** a task that faults with an exception carrying a known message, **When** `.Should().ThrowAsync<TException>()` completes, **Then** the returned result exposes a `.Which` property providing the caught exception for further assertion.
2. **Given** a task with an unwrapped `AggregateException` whose single inner exception has a known message, **When** `.Should().ThrowAsync<TException>()` completes and `.Which.Message` is inspected, **Then** the message reflects the inner exception's message.

---

### User Story 3 - Provide Contextual Failure Messages with Because (Priority: P3)

A developer wants failure messages to include test-context information explaining why a particular exception was expected, so that CI output is immediately understandable without reading the surrounding test code.

**Why this priority**: Diagnostic quality is valuable but the feature is usable without it. The `because`/`becauseArgs` pattern is consistent with all other assertion methods in the library and should be supported for API completeness.

**Independent Test**: Can be fully validated by calling `.ThrowAsync<TException>("because {0} requires it", "the contract")` on a task that does not fault, and confirming the failure message includes the because phrase.

**Acceptance Scenarios**:

1. **Given** a task that completes successfully, **When** `.Should().ThrowAsync<InvalidOperationException>("because {0} is required", "validation")` is awaited, **Then** the `AssertionFailedException` message contains the formatted `because` phrase.
2. **Given** a task that faults with the wrong exception type, **When** `.ThrowAsync<TException>("because of contract X")` is awaited, **Then** the failure message includes "because of contract X".

---

### Edge Cases

- What happens when the `Task` subject is `null`? The assertion MUST fail with a descriptive `AssertionFailedException` rather than throw a `NullReferenceException`. A null task cannot be awaited, so the check is performed up front.
- What happens when the `Task` faults with an `AggregateException` that contains multiple inner exceptions? The unwrapping logic only applies when there is exactly one inner exception; a multi-inner `AggregateException` is treated as the exception itself. If `AggregateException` was not the asserted type, the assertion fails.
- What happens when the `Task` is already completed (faulted, cancelled, or ran to completion) when `.Should()` is called? The task's already-recorded outcome is used — no re-execution occurs. Awaiting an already-completed task is safe and idiomatic.
- What happens when the `Task` is cancelled (throws `OperationCanceledException`)? Cancellation is treated the same as any other fault: if `OperationCanceledException` is the asserted type, the assertion passes; otherwise it fails.
- What happens when `ThrowAsync<TException>` is called on a `Task<T>` (a generic task)? `Task<T>` inherits from `Task`, so it is covered by the same `Task` extension method without a separate overload. The return value of the task is discarded; only the fault state is relevant.
- What happens when `.Should()` or `ThrowAsync` is called on a `Task` that is still in-flight (not yet completed)? `ThrowAsync` awaits the task normally using `await Subject` — it does not require the task to be already completed at `.Should()` call time. All observable task states (ran to completion, faulted, cancelled) are handled uniformly by the single `await` call.
- What happens when the `Task` faults with an `AggregateException` containing zero inner exceptions? The single-inner unwrapping condition (`InnerExceptions.Count == 1`) is not met, so the `AggregateException` is not unwrapped and is treated as the caught exception itself, falling through to the wrong-type failure path unless `AggregateException` was the asserted type.
- Note on task cancellation and `TaskCanceledException`: `Task.FromCanceled(...)` when awaited throws `TaskCanceledException`, which is a subtype of `OperationCanceledException`. Because `ThrowAsync` uses C# subtype matching (FR-002), asserting `.ThrowAsync<OperationCanceledException>()` passes for a cancelled task.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST expose a `.Should()` extension method on `Task?` (nullable) subjects that returns a `TaskAssertions` value, accepting a `[CallerArgumentExpression]`-sourced caller expression string for diagnostic output. The parameter type is `Task?` so that a null task — a testable condition rather than a programming error — is forwarded into the struct; null detection is intentionally deferred to `ThrowAsync` (see FR-010).
- **FR-002**: `TaskAssertions` MUST expose an async `ThrowAsync<TException>()` method that awaits the subject `Task` and passes when the task faults with an exception that is `TException` or a subtype of `TException`.
- **FR-003**: `ThrowAsync<TException>()` MUST apply the same `AggregateException` single-inner-exception unwrapping logic used by the existing `Func<Task>` assertion path: if the task faults with an `AggregateException` containing exactly one inner exception, that inner exception is extracted and used for both type-matching and result chaining (`.Which`). An `AggregateException` with zero inner exceptions or more than one inner exception MUST NOT be unwrapped and is treated as the exception itself.
- **FR-004**: `ThrowAsync<TException>()` MUST fail with a descriptive `AssertionFailedException` when the subject task completes successfully without faulting. The failure message MUST state that an exception was expected but none was thrown, and MUST include the subject expression captured by `[CallerArgumentExpression]`.
- **FR-005**: `ThrowAsync<TException>()` MUST fail with a descriptive `AssertionFailedException` when the task faults with an exception type that is not `TException` or a subtype of `TException`. The failure message MUST include the expected type name, the actual (unwrapped) exception's type name, and the actual (unwrapped) exception's `.Message` text — where "unwrapped" means the inner exception after single-inner `AggregateException` extraction per FR-003, not the original `AggregateException` wrapper — for example: `"Expected ArgumentNullException but found InvalidOperationException: Value cannot be null"`.
- **FR-006**: `ThrowAsync<TException>()` MUST accept `because` and `becauseArgs` parameters and incorporate the formatted reason phrase into any failure message it produces.
- **FR-007**: `ThrowAsync<TException>()` MUST return `ExceptionAssertions<TException>` — the same type already returned by `AsyncFunctionAssertions.ThrowAsync` — ensuring that the post-assertion chaining API (including `.Which`) is identical between the `Task` and `Func<Task>` entry points.
- **FR-008**: `ThrowAsync<TException>()` MUST be annotated with `[StackTraceHidden]` so that internal assertion frames do not appear in failure stack traces shown to the developer.
- **FR-009**: `TaskAssertions` MUST be a `readonly struct`, consistent with all other assertion types in the library.
- **FR-010**: A null `Task` subject MUST produce a descriptive `AssertionFailedException` from `ThrowAsync` rather than a `NullReferenceException`. The failure message MUST convey the expected value (`task to be non-null`), the actual value (`task was null`), and the subject expression captured by `[CallerArgumentExpression]`.
- **FR-011**: The existing `AsyncFunctionAssertions` for `Func<Task>` subjects MUST remain unaffected; both entry points MUST coexist without conflict. The `Task.Should()` overload MUST NOT interfere with `Func<Task>.Should()` dispatch, including at compile-time overload resolution — an expression whose compile-time type is `Func<Task>` MUST continue to resolve to `AsyncFunctionAssertions` after the new overload is introduced.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every acceptance scenario defined in User Stories 1–3 is covered by an automated test that passes without modification to the test.
- **SC-002**: All existing tests for `AsyncFunctionAssertions` and every other assertion type continue to pass after the new entry point is added — zero regressions. Specifically, all test classes in `tests/Assertivo.Tests/` — including `AsyncFunctionAssertionsTests`, `ActionAssertionsTests`, `BooleanAssertionsTests`, `ChainingTests`, `CollectionAssertionsTests`, `DictionaryAssertionsTests`, `ShouldDispatchTests`, and all others — MUST pass.
- **SC-003**: A developer can migrate a test from the `Func<Task>` wrapping pattern to the direct `Task.Should()` pattern by removing the lambda wrapper alone, with no other change required, and the test behaviour is identical. "Identical" means: (1) `.Which` chaining provides the same caught exception with the same type and message; (2) `because` and `becauseArgs` are formatted and incorporated into failure messages in the same way; (3) failure message format, including the `[CallerArgumentExpression]` label, is equivalent; and (4) `AggregateException` single-inner unwrapping behaves the same way.
- **SC-004**: Failure messages for all four failure conditions (no exception thrown, wrong exception type, null subject, and multi-inner `AggregateException` — a sub-case of wrong exception type, since a multi-inner `AggregateException` is not unwrapped and falls through to the wrong-type failure path unless `AggregateException` itself was the asserted type) include enough information for a developer to identify the problem without running a debugger. For the wrong-type case specifically, the message MUST include the expected type name, the actual (unwrapped) exception's type name, and the actual (unwrapped) exception's `.Message` text.
- **SC-005**: The `[CallerArgumentExpression]` subject label appears in every failure message so that the failing task expression is visible in CI output.

## Non-Functional Requirements

- **NFR-001**: `TaskAssertions` and its `ThrowAsync<TException>` method MUST be AOT-compatible — they MUST NOT use reflection, `dynamic`, or any mechanism incompatible with .NET AOT compilation. The hosting project has `IsAotCompatible` set to `true`.
- **NFR-002**: Each `ThrowAsync<TException>` invocation MUST complete within 100 ms in a test environment, per the constitution §IV.2 test timeout budget. No additional synchronisation or blocking constructs are introduced that would inflate this budget.
- **NFR-003**: `TaskAssertions` MUST be safe to instantiate and read concurrently from multiple threads. This is guaranteed by the `readonly struct` design (FR-009) — the struct has no mutable fields and each assertion invocation is fully self-contained.

## Assumptions

- `Task<T>` subjects are in scope via inheritance: since `Task<T>` derives from `Task`, the `Task` extension method covers them automatically. The return value `T` is not surfaced; only the fault/completion state matters for this feature.
- `NotThrowAsync` is explicitly out of scope for this feature and will be addressed separately if needed.
- The `AggregateException` unwrapping rule (single inner exception) matches the behaviour already specified and implemented in `AsyncFunctionAssertions` (FR-020 in the existing library specification). No divergence from that rule is introduced.
- The new `Should()` overload for `Task` does not conflict with the existing overload for `Func<Task>` because they target distinct compile-time types.
- Cancellation (`OperationCanceledException`) is treated identically to any other exception fault; no special cancellation handling is added in this feature.
