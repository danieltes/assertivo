# Tasks: Should() Entry Point for Task Subjects

**Input**: Design documents from `/specs/00026-task-should-assertions/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · contracts/ ✅ · quickstart.md ✅

**Scope summary**:
- `src/Assertivo/Exceptions/TaskAssertions.cs` — NEW file (~60 lines)
- `src/Assertivo/Should.cs` — +1 extension method overload (`Task?`)
- `tests/Assertivo.Tests/TaskAssertionsTests.cs` — NEW file (~90 lines)
- `tests/Assertivo.Tests/ShouldDispatchTests.cs` — +2 dispatch tests

---

## Phase 1: Setup

**Purpose**: Verify the baseline is clean before any changes are made.

- [x] T001 Run `dotnet test tests/Assertivo.Tests/` and confirm all pre-existing tests pass with zero failures as a baseline before any edits

**Checkpoint**: Baseline green — proceed to Phase 2.

---

## Phase 2: Foundational

**Purpose**: Create the `TaskAssertions` struct skeleton and the `Should()` extension overload that every user-story task depends on for compilation and dispatch.

**⚠️ CRITICAL**: T002 and T003 must both be complete before any user-story task (T004+) begins. They can be executed in parallel with each other.

- [x] T002 [P] Create `src/Assertivo/Exceptions/TaskAssertions.cs` with: namespace declaration (`namespace Assertivo.Exceptions;`), `public readonly struct TaskAssertions` with struct-level XML `<summary>` doc ("Assertions for a <see cref="Task"/> subject that is already started. Obtain an instance via <c>task.Should()</c>."), internal constructor `internal TaskAssertions(Task? subject, string? expression)` assigning both fields, `public Task? Subject { get; }` with XML doc, `internal string? Expression { get; }`, and a stubbed `[StackTraceHidden] public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs) where TException : Exception` with full XML docs (`<summary>`, `<typeparam>`, `<param>`, `<returns>`) and body `throw new NotImplementedException();` — the stub is required so test references compile and produce observable failures before T009
- [x] T003 [P] Add `Should(this Task? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)` overload to `src/Assertivo/Should.cs` immediately after the existing `Func<Task>` overload; include XML `<summary>` doc ("Returns a <see cref="TaskAssertions"/> for the specified task subject.") matching the existing overload style; return type is `TaskAssertions`; body is `=> new(subject, caller);`; add `using Assertivo.Exceptions;` import if not already present at the top of the file

**Checkpoint**: Foundation ready — task compiles, `task.Should()` resolves to `TaskAssertions`, T004+ can now begin.

---

## Phase 3: User Story 1 — Assert Faulted Task Throws Expected Exception (Priority: P1) 🎯 MVP

**Goal**: `task.Should().ThrowAsync<TException>()` passes when the task faults with the expected type; fails with a descriptive `AssertionFailedException` for every failure condition: no exception thrown, wrong exception type, null subject, and multi-inner `AggregateException`.

**Independent Test**: Declare a `Task` that faults with `InvalidOperationException` and call `.Should().ThrowAsync<InvalidOperationException>()` — assert passes with no lambda wrapper and no intermediate variable.

### Tests for User Story 1 (write first — MUST fail with `NotImplementedException` before T009) ⚠️

- [x] T004 [P] [US1] Create `tests/Assertivo.Tests/TaskAssertionsTests.cs` with class `public class TaskAssertionsTests`, required usings (`using Assertivo; using Assertivo.Exceptions; using System.Threading.Tasks;`), and three tests: `ThrowAsync_CorrectType_Passes` (task is `Task.FromException(new InvalidOperationException())`, awaits `.Should().ThrowAsync<InvalidOperationException>()`, asserts no exception is thrown); `ThrowAsync_Subtype_Passes` (task faults with `ArgumentNullException`, awaits `.Should().ThrowAsync<Exception>()` requesting the base type, asserts pass — verifies subtype matching per FR-002); and `Should_Subject_ReturnsOriginalTask` (`Task task = Task.CompletedTask;`, asserts `task.Should().Subject == task` — constitution §IV.1 positive test for `public Task? Subject`, C1)
- [x] T005 [P] [US1] Add `ThrowAsync_NoThrow_Fails` (task is `Task.CompletedTask`, awaits `.Should().ThrowAsync<InvalidOperationException>()`, asserts `AssertionFailedException` is thrown and its `Message` contains `"InvalidOperationException"` and `"no exception was thrown"` (the exact phrase prescribed by T009's `MessageFormatter.Fail` actual argument — M2)) and `ThrowAsync_WrongType_Fails` (task faults with `new ArgumentException("bad arg")`, asserts `AssertionFailedException` message contains `"InvalidOperationException"` as expected type, `"ArgumentException"` as actual type, and `"bad arg"` as the actual exception's `.Message` text — per FR-005 and SC-004) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`
- [x] T006 [P] [US1] Add `ThrowAsync_AggregateException_SingleInner_Unwraps` (task is `Task.FromException(new AggregateException(new InvalidOperationException("inner-msg")))`, awaits `.Should().ThrowAsync<InvalidOperationException>()`, asserts pass — verifies the single-inner unwrap path succeeds without throwing; `.Which` access on the result is verified separately in T011 (US2), H1/M3) and `ThrowAsync_AggregateException_MultipleInners_Fails` (task is `Task.FromException(new AggregateException(new InvalidOperationException(), new ArgumentException()))`, asserts `.Should().ThrowAsync<InvalidOperationException>()` throws `AssertionFailedException` whose message contains `"AggregateException"` as the actual type — multi-inner aggregate is not unwrapped and falls through to the standard wrong-type failure path per SC-004) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`
- [x] T007 [P] [US1] Add `ThrowAsync_NullSubject_Fails` (`Task? nullTask = null;`, awaits `nullTask.Should().ThrowAsync<InvalidOperationException>()`, asserts `AssertionFailedException` is thrown and `Message` contains `"null"` — verifies FR-010, no `NullReferenceException`, M1); `ThrowAsync_CallerExpression_AppearsInFailureMessage` (`Task? namedTask = Task.CompletedTask;`, calls `namedTask.Should().ThrowAsync<InvalidOperationException>()`, catches `AssertionFailedException`, asserts `ex.Message` contains `"namedTask"` — verifies SC-005 `[CallerArgumentExpression]` threading); and `Should_NullSubject_SubjectIsNull` (`Task? nullTask = null;`, asserts `nullTask.Should().Subject is null` — constitution §IV.1 edge-case test for `public Task? Subject`, C1) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`
- [x] T008 [P] [US1] Add `ThrowAsync_CancelledTask_MatchesCancellationException` (task is `Task.FromCanceled(new CancellationToken(canceled: true))`, awaits `.Should().ThrowAsync<OperationCanceledException>()`, asserts pass — cancellation is treated identically to any other fault per spec edge cases) and `ThrowAsync_CancelledTask_WrongType_Fails` (same cancelled task, awaits `.Should().ThrowAsync<InvalidOperationException>()`, asserts `AssertionFailedException` is thrown and message contains `"OperationCanceledException"` as the actual type — verifies cancellation follows the standard wrong-type failure path with no special handling, L3) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`

### Implementation for User Story 1

- [x] T009 [US1] Replace the `throw new NotImplementedException();` stub with the complete `ThrowAsync<TException>` implementation in `src/Assertivo/Exceptions/TaskAssertions.cs` using the following structure (C2 — L1): **(1)** null guard — `if (Subject is null) MessageFormatter.Fail("task to be non-null", "task was null", Expression, because, becauseArgs);`; **(2)** `try { await Subject!.ConfigureAwait(false); }` — if `await` completes without throwing, execution exits the try/catch and falls through to step **(4)**; **(3)** `catch (Exception target)` block — **(3a)** single-inner AggregateException unwrap: `if (target is AggregateException { InnerExceptions.Count: 1 } ae) target = ae.InnerExceptions[0];` — **(3b)** type check + happy path: `if (target is TException typed) return new ExceptionAssertions<TException>(typed, Expression);` — **(3c)** wrong-type failure: `MessageFormatter.Fail(typeof(TException).Name, $"{target.GetType().Name}: {target.Message}", Expression, because, becauseArgs); return default;` (`Fail` always throws; `return default;` satisfies the compiler); **⚠️ the catch block MUST always reach a `return` or `MessageFormatter.Fail` — it must never fall through to step (4)**; **(4)** no-throw failure — placed **after the closing brace of the catch block** in the method body, never inside the `try`: `MessageFormatter.Fail($"task to throw {typeof(TException).Name}", "no exception was thrown", Expression, because, becauseArgs); return default;`; keep `[StackTraceHidden]` and all XML docs from the T002 stub

**Checkpoint**: US1 fully functional and independently testable. MVP deliverable complete — `task.Should().ThrowAsync<X>()` works end-to-end for all five code paths without any lambda wrapper.

---

## Phase 4: User Story 2 — Chain Further Assertions on the Caught Exception (Priority: P2)

**Goal**: The `ExceptionAssertions<TException>` returned by `ThrowAsync` exposes `.Which` providing direct access to the caught exception for further assertion chaining, with `Expression` threaded through so that chained failures identify the originating task.

**Independent Test**: Await `task.Should().ThrowAsync<InvalidOperationException>()`, then call `.Which.Message.Should().Contain("expected-text")` in a single chain — no intermediate variable required beyond the awaited result.

### Tests for User Story 2

- [x] T010 [P] [US2] Add `ThrowAsync_Which_ExposesException` (task faults with `new ArgumentNullException("paramName")`, awaits `.ThrowAsync<ArgumentNullException>()`, asserts `result.Which.ParamName == "paramName"` — verifies `.Which` provides the typed caught exception, not a base reference) and `ThrowAsync_Which_ChainsToShould` (task faults with `new InvalidOperationException("expected text")`, asserts `(await task.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("expected text")` — verifies fluent chaining through `.Which.Message.Should()` compiles and passes) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`
- [x] T011 [P] [US2] Add `ThrowAsync_AggregateUnwrap_WhichReflectsInnerMessage` (task is `Task.FromException(new AggregateException(new InvalidOperationException("inner-msg")))`, awaits `.ThrowAsync<InvalidOperationException>()`, asserts `.Which.Message == "inner-msg"` — verifies the unwrapped inner exception (not the `AggregateException`) is the one surfaced via `.Which`, matching the intent of FR-003) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`

**Checkpoint**: US2 verified — `.Which` chaining provides the expected exception object with full type fidelity.

---

## Phase 5: User Story 3 — Provide Contextual Failure Messages with Because (Priority: P3)

**Goal**: `because` and `becauseArgs` are threaded through `MessageFormatter.Fail` for every failure code path in `ThrowAsync`, so CI output includes developer-provided context without extra tooling.

**Independent Test**: Call `.ThrowAsync<InvalidOperationException>("because {0} requires it", "the contract")` on a task that completes successfully; catch the `AssertionFailedException` and assert its message contains the string `"the contract"`.

### Tests for User Story 3

- [x] T012 [P] [US3] Add `ThrowAsync_Because_NoThrow_IncludesFormattedReason` (task is `Task.CompletedTask`, calls `.ThrowAsync<InvalidOperationException>("because {0} requires it", "validation")`, catches `AssertionFailedException`, asserts `ex.Message` contains `"validation"` — verifies `becauseArgs` are formatted and included in the no-throw failure path); `ThrowAsync_Because_WrongType_IncludesReason` (task faults with `new ArgumentException("x")`, calls `.ThrowAsync<InvalidOperationException>("because of contract X")`, catches `AssertionFailedException`, asserts `ex.Message` contains `"because of contract X"` — verifies `because` appears in the wrong-type failure path); and `ThrowAsync_Because_NullSubject_IncludesReason` (`Task? nullTask = null;`, calls `nullTask.Should().ThrowAsync<InvalidOperationException>("because {0}", "the test requires it")`, catches `AssertionFailedException`, asserts `ex.Message` contains `"the test requires it"` — verifies FR-006 `because` threading covers the null-subject failure path, M5) in `tests/Assertivo.Tests/TaskAssertionsTests.cs`

**Checkpoint**: US3 verified — `because`/`becauseArgs` confirmed across both observable failure paths.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Add dispatch regression tests covering `Task` and `Task<T>` subjects, verify `Func<Task>` coexistence (FR-011), and confirm the full suite is green.

- [x] T013 [P] Add the following four tests to `tests/Assertivo.Tests/ShouldDispatchTests.cs`: `Should_TaskSubject_ReturnsTaskAssertions` (`Task task = Task.CompletedTask;`, asserts `Assert.IsType<TaskAssertions>(task.Should())` — verifies `Task` no longer falls through to `ObjectAssertions<Task>`); `Should_TaskOfTSubject_ReturnsTaskAssertions` (`Task<int> task = Task.FromResult(42);`, asserts `Assert.IsType<TaskAssertions>(task.Should())` — verifies `Task<T>` resolves via inheritance without a separate overload); `Should_FuncTaskSubject_StillReturnsAsyncFunctionAssertions` (`Func<Task> fn = () => Task.CompletedTask;`, asserts `Assert.IsType<AsyncFunctionAssertions>(fn.Should())` — verifies FR-011: the new `Task` overload does not shadow `Func<Task>` dispatch, H2); and `Should_TaskAndFuncTask_BehaviourEquivalent_ForFaultingInput` (create `var innerEx = new InvalidOperationException("msg");`; assert both `(await Task.FromException(innerEx).Should().ThrowAsync<InvalidOperationException>()).Which.Message` and `(await ((Func<Task>)(async () => throw innerEx)).Should().ThrowAsync<InvalidOperationException>()).Which.Message` equal `"msg"` — verifies SC-003 migration equivalence between the `Task` and `Func<Task>` entry points, M4)
- [x] T014 Run `dotnet test tests/Assertivo.Tests/` to confirm SC-002 zero regressions — all pre-existing `AsyncFunctionAssertionsTests`, `ShouldDispatchTests`, and all other test classes must pass alongside the new `TaskAssertionsTests`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on T001 passing — **BLOCKS all user story tasks**
- **User Story phases (Phase 3–5)**: All depend on T002 + T003 completion
  - T004–T008 (US1 tests) can be authored in parallel as soon as T002 + T003 are done
  - T009 (US1 implementation) depends on T004–T008 existing with observed failures
  - T010–T011 (US2 tests) and T012 (US3 tests) depend on T009 (test the completed implementation)
- **Polish (Phase 6)**: T013 depends on T003 (overload exists to test dispatch); T014 depends on T009 + T013

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2. No story dependencies. Delivers standalone value.
- **User Story 2 (P2)**: Starts after T009. Tests the already-complete implementation; no new production code.
- **User Story 3 (P3)**: Starts after T009. Tests `because` threading already present in T009's implementation.

### Within User Story 1 (Phase 3)

- T004–T008 MUST be written and observed to fail (throw `NotImplementedException`) before T009 is implemented — Red-Green-Refactor per constitution §IV.3
- T004–T008 can be authored in parallel (independent `[Fact]` methods in the same file)
- T009 MUST follow after all test tasks are authored and confirmed failing

### Parallel Opportunities

- T002 and T003 are fully parallel (different files, no shared dependency)
- T004, T005, T006, T007, T008 are fully parallel within Phase 3 (independent test methods in the same file)
- T010 and T011 are fully parallel within Phase 4
- T013 is independent of T014 within Phase 6

---

## Parallel Example: User Story 1

```bash
# Step 1 — parallel: author all US1 test methods (T004–T008)
# Each task adds independent [Fact] methods to TaskAssertionsTests.cs

# Step 2 — verify tests fail (expected, stub throws NotImplementedException):
dotnet test tests/Assertivo.Tests/ --filter "ClassName=TaskAssertionsTests"

# Step 3 — sequential: implement ThrowAsync (T009)

# Step 4 — verify green:
dotnet test tests/Assertivo.Tests/ --filter "ClassName=TaskAssertionsTests"

# Step 5 — parallel: author US2/US3 tests (T010–T012), dispatch tests (T013)

# Step 6 — full suite regression (T014):
dotnet test tests/Assertivo.Tests/
```

---

## Implementation Strategy

**MVP scope**: Phase 3 (User Story 1) alone delivers a fully working `task.Should().ThrowAsync<TException>()` entry point with all failure conditions covered. Users can migrate from the `Func<Task>` wrapping pattern by removing the lambda wrapper — no other change required (SC-003).

**Incremental delivery order**:
1. **Phase 1–2**: Foundation — skeleton compiles, `task.Should()` dispatches correctly, no working assertions yet
2. **Phase 3**: MVP — `ThrowAsync` fully working for all five code paths; all US1 acceptance scenarios pass
3. **Phase 4**: US2 verified — `.Which` chaining confirmed; US2 acceptance scenarios pass
4. **Phase 5**: US3 verified — `because` threading confirmed; US3 acceptance scenarios pass
5. **Phase 6**: Dispatch regression locked in; full suite green; feature complete

**Note on US2 and US3 implementation**: Because `ThrowAsync` returns `ExceptionAssertions<TException>` (which already provides `.Which`) and `MessageFormatter.Fail` already accepts `because`/`becauseArgs`, the T009 implementation simultaneously satisfies all three user stories. Phases 4 and 5 therefore consist entirely of test tasks that verify the US2/US3 acceptance scenarios against the already-complete implementation.
