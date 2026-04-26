# Tasks: Should() Type-Aware Dispatch

**Input**: Design documents from `/specs/00018-should-type-dispatch/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · contracts/ ✅ · quickstart.md ✅

**Scope summary**:
- `src/Assertivo/Should.cs` — +3 extension method overloads (`IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `Func<T>`)
- `tests/Assertivo.Tests/ShouldDispatchTests.cs` — NEW file, compile-time dispatch regression + behavior tests
- `IReadOnlyDictionary<K,V>` is **pre-satisfied** — no source change needed; tests only
- No new assertion classes; no new projects; no new packages

---

## Phase 1: Setup

**Purpose**: Verify the baseline is clean before any changes are made.

- [X] T001 Run `dotnet test tests/Assertivo.Tests/` and confirm all pre-existing tests pass with zero failures as a baseline before any edits

**Checkpoint**: Baseline green — proceed to Phase 2.

---

## Phase 2: Foundational

**Purpose**: Create the shared test file that all user story dispatch tests will live in.

**⚠️ CRITICAL**: T002 must be complete before any user-story test tasks (T003+) begin.

- [X] T002 Create `tests/Assertivo.Tests/ShouldDispatchTests.cs` with the namespace declaration (`namespace Assertivo.Tests;`), required using directives (`using Assertivo; using Assertivo.Collections; using Assertivo.Exceptions;`), and an empty `public class ShouldDispatchTests` body — no test methods yet

**Checkpoint**: Foundation ready — user story tasks T003+ can now begin.

---

## Phase 3: User Story 1 — Assert on Read-Only Collection Interfaces (Priority: P1) 🎯 MVP

**Goal**: `IReadOnlyList<T>.Should()` and `IReadOnlyCollection<T>.Should()` return `GenericCollectionAssertions<T>`, enabling the full collection assertion API without any cast or wrapper.

**Independent Test**: Declare a local variable as `IReadOnlyList<string>` and call `.HaveCount()`, `.Contain()`, `.BeEmpty()`, `.ContainSingle()`, `.AllSatisfy()`, and `.BeEquivalentTo()` in a single test run — all must pass without any `(IEnumerable<string>)` cast.

### Tests for User Story 1 (write first — must fail before T006/T007)

- [X] T003 [US1] Add dispatch type-assertion tests `Should_IReadOnlyListSubject_ReturnsGenericCollectionAssertions` and `Should_IReadOnlyCollectionSubject_ReturnsGenericCollectionAssertions` in `tests/Assertivo.Tests/ShouldDispatchTests.cs` — use `Assert.IsType<GenericCollectionAssertions<string>>(subject.Should())` pattern; these MUST fail (compile error or wrong type) before T006/T007 are implemented — `Assert.IsType<>()` is the agreed runtime proxy for SC-004's compile-time dispatch guarantee: static correctness is confirmed by the test project building and the type assertion passing without any cast
- [X] T004 [US1] Add behavior tests in `tests/Assertivo.Tests/ShouldDispatchTests.cs` covering each required assertion method on an `IReadOnlyList<string>` subject declared at interface type: `Should_IReadOnlyListSubject_HaveCount_Passes`, `Should_IReadOnlyListSubject_Contain_Passes`, `Should_IReadOnlyListSubject_BeEmpty_Passes`, `Should_IReadOnlyListSubject_ContainSingle_ProvidesWhichForChaining`, `Should_IReadOnlyListSubject_AllSatisfy_Passes`, `Should_IReadOnlyListSubject_BeEquivalentTo_Passes`. Also add `IReadOnlyCollection<T>` behavior tests (FR-002): `Should_IReadOnlyCollectionSubject_HaveCount_Passes`, `Should_IReadOnlyCollectionSubject_ContainSingle_ProvidesWhichForChaining`. Also add negative tests (constitution §4.1): `Should_IReadOnlyListSubject_HaveCount_WithWrongCount_Fails` (assert `AssertionFailedException` thrown and message contains `"Expected"` or count values), `Should_IReadOnlyCollectionSubject_BeEmpty_WhenNotEmpty_Fails` (assert `AssertionFailedException` thrown)
- [X] T005 [US1] Add null-safety edge-case tests in `tests/Assertivo.Tests/ShouldDispatchTests.cs`: `Should_NullIReadOnlyListSubject_WrapsNullSafely` (assert `BeNull()` passes on a null `IReadOnlyList<string>?` subject) and `Should_NullIReadOnlyCollectionSubject_WrapsNullSafely` (same for `IReadOnlyCollection<string>?`)

### Implementation for User Story 1

- [X] T006 [US1] Add `IReadOnlyList<T>.Should()` overload in `src/Assertivo/Should.cs`: insert immediately after the existing `Func<Task>` overload and before the `IEnumerable<T>` overload; include `<summary>` XML doc matching existing style; body is `=> new(subject, caller);`; parameter type is `this IReadOnlyList<T>? subject`
- [X] T007 [US1] Add `IReadOnlyCollection<T>.Should()` overload in `src/Assertivo/Should.cs`: insert immediately after the `IReadOnlyList<T>` overload added in T006; include `<summary>` XML doc; same body pattern; parameter type is `this IReadOnlyCollection<T>? subject`
- [X] T008 [US1] Run `dotnet test tests/Assertivo.Tests/ --filter "ClassName=ShouldDispatchTests"` and confirm T003–T005 tests are now green; full regression is confirmed in T019 (this scoped filter run validates new dispatch tests only)

**Checkpoint**: US1 fully functional and independently testable. MVP deliverable complete.

---

## Phase 4: User Story 2 — Assert on Non-Async Delegate Func\<T\> (Priority: P2)

**Goal**: `Func<T>.Should()` (single type parameter, non-Task) returns `ActionAssertions`, enabling `.Throw<TException>()` with `.Which` chaining and `.NotThrow()` without wrapping the delegate in an `Action` lambda.

**Independent Test**: Declare a `Func<string>` that throws `InvalidOperationException`; call `.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("expected text")` — all in a single chain with no cast.

### Tests for User Story 2 (write first — must fail before T013)

- [X] T009 [US2] Add dispatch test `Should_FuncTSubject_ReturnsActionAssertions` in `tests/Assertivo.Tests/ShouldDispatchTests.cs` — use `Assert.IsType<ActionAssertions>(subject.Should())` where `subject` is `Func<string>`; must fail (wrong dispatch) before T013 — `Assert.IsType<>()` is the agreed runtime proxy for SC-004's compile-time dispatch guarantee
- [X] T010 [US2] Add throw+chaining test `Should_FuncTSubjectThatThrows_ThrowPassesAndWhichProvidesException` in `tests/Assertivo.Tests/ShouldDispatchTests.cs` — `Func<string>` throws `InvalidOperationException`; assert `.Should().Throw<InvalidOperationException>()` passes and `.Which.Message` is accessible
- [X] T011 [US2] Add non-throw failure test `Should_FuncTSubjectThatDoesNotThrow_ThrowAssertionFails` in `tests/Assertivo.Tests/ShouldDispatchTests.cs` — `Func<int>` returns normally; assert that `.Should().Throw<InvalidOperationException>()` throws `AssertionFailedException`; also assert the exception message contains the expected type name (e.g., `"InvalidOperationException"`) per constitution §3.4's requirement for expected/actual values in error output
- [X] T012 [US2] Add `NotThrow` and null-guard tests in `tests/Assertivo.Tests/ShouldDispatchTests.cs`: `Should_FuncTSubjectThatDoesNotThrow_NotThrowPasses` (Func<int> returns 42; `.Should().NotThrow()` passes) and `Should_NullFuncTSubject_ThrowsArgumentNullException` (null `Func<string>` passed to `.Should()` throws `ArgumentNullException`)

### Implementation for User Story 2

- [X] T013 [US2] Add `Func<T>.Should()` overload in `src/Assertivo/Should.cs`: insert immediately after the `Func<Task>` overload and before the `IReadOnlyList<T>` overload if it already exists, otherwise immediately before the `IEnumerable<T>` overload; include `<summary>` XML doc and `<exception cref="ArgumentNullException">` XML doc tag; body calls `ArgumentNullException.ThrowIfNull(subject)` then returns `new ActionAssertions(() => subject(), caller)` — the method body is two statements, not an expression body
- [X] T014 [US2] Run `dotnet test tests/Assertivo.Tests/ --filter "ClassName=ShouldDispatchTests"` and confirm T009–T012 (US2) tests are now green alongside T003–T005 (US1); full regression is confirmed in T019 (this scoped filter run validates new dispatch tests only)

**Checkpoint**: US1 and US2 independently functional. Delegates dispatch correctly.

---

## Phase 5: User Story 3 — Assert on Read-Only Dictionary (Priority: P3) [Pre-satisfied — tests only]

**Goal**: Confirm via regression test that `IReadOnlyDictionary<K,V>.Should()` already returns `GenericDictionaryAssertions<K,V>` (the overload exists at line ~58 of `Should.cs`) and that `.ContainKey()`, `.HaveCount()`, and `.NotBeNull()` are callable. No source change required in `Should.cs`.

**Independent Test**: Declare a variable as `IReadOnlyDictionary<string, int>`; verify `.Should().ContainKey()` and `.Should().HaveCount()` compile and pass without cast.

### Tests for User Story 3

- [X] T015 [P] [US3] Add regression tests in `tests/Assertivo.Tests/ShouldDispatchTests.cs`: `Should_IReadOnlyDictionarySubject_ReturnsGenericDictionaryAssertions` (dispatch type check), `Should_IReadOnlyDictionarySubject_NotBeNull_Passes`, and `Should_IReadOnlyDictionarySubject_ContainKey_Passes` — these should pass immediately (pre-existing overload) without any implementation changes
- [X] T016 [US3] Run `dotnet test tests/Assertivo.Tests/ --filter "ClassName=ShouldDispatchTests"` and confirm all US3 tests pass green on first run (no implementation needed)

**Checkpoint**: All three user stories independently functional and tested.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Lock in the `IEnumerable<T>` regression guard (FR-005 / SC-002), verify zero compiler warnings, and confirm the full test suite is green.

- [X] T017 [P] Add regression test `Should_IEnumerableTSubject_StillReturnsGenericCollectionAssertions` in `tests/Assertivo.Tests/ShouldDispatchTests.cs` — subject declared as `IEnumerable<string>`; assert `IsType<GenericCollectionAssertions<string>>(subject.Should())`; this locks in that the new `IReadOnlyList<T>`/`IReadOnlyCollection<T>` overloads have not accidentally shadowed the `IEnumerable<T>` path (FR-005)
- [X] T018 Run `dotnet build src/Assertivo/Assertivo.csproj -warnaserror` and confirm zero warnings — verify nullable annotations, XML doc completeness, and that no new CS warnings were introduced by the three new overloads
- [X] T018b Run `dotnet test tests/Assertivo.Tests/ /p:CollectCoverage=true /p:Threshold=95 /p:ThresholdType=line /p:ThresholdStat=Total` and confirm line coverage meets or exceeds the 95% constitution threshold; branch coverage target is 90% (constitution §4.1)
- [X] T019 Run `dotnet test` from the repository root to confirm 100% of all tests (pre-existing + new dispatch tests) pass with zero failures

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **blocks** all test writing (T003+)
- **Phase 3 (US1)**: Depends on Phase 2; tests (T003–T005) must be written and fail before implementation (T006–T007)
- **Phase 4 (US2)**: Depends on Phase 2; tests (T009–T012) must be written before T013; T013 insertion point (after `IReadOnlyList<T>` overload) can use either the T006/T007 result or the existing `Func<Task>` overload as anchor
- **Phase 5 (US3)**: Depends on Phase 2; T015 is independent of Phases 3 and 4 (pre-existing overload); can run after T002 completes
- **Phase 6 (Polish)**: T017 can be written anytime after T002; T018 and T019 depend on all implementation tasks (T006, T007, T013) being complete

### User Story Dependencies

- **US1 (P1)**: Depends only on Phase 2; no dependency on US2 or US3
- **US2 (P2)**: Depends only on Phase 2; no dependency on US1; T013 inserts between `Func<Task>` and `IReadOnlyList<T>` (which may or may not already exist depending on execution order — anchor to `Func<Task>` if US1 not yet done)
- **US3 (P3)**: Depends only on Phase 2; entirely independent of US1 and US2 (tests pre-existing behavior)

### Parallel Opportunities

- **T003–T005 + T013**: Tests in `ShouldDispatchTests.cs` can be written while `Should.cs` implementation is in progress (different files) — but TDD order requires tests exist first
- **T015 [P]** (US3 tests): Can be written anytime after T002, independent of US1 and US2 implementation
- **T017 [P]** (`IEnumerable<T>` regression test): Can be written anytime after T002; does not depend on any new implementation

---

## Parallel Example: US1 + US3 simultaneously (after T002)

```
Stream A — US1 implementation:
  T003 → T004 → T005 (write failing tests in ShouldDispatchTests.cs)
  T006 → T007 (add overloads in Should.cs)
  T008 (run and verify green)

Stream B — US3 regression tests (parallel, independent):
  T015 (write and verify IReadOnlyDictionary regression tests — pass immediately)
```

---

## Implementation Strategy

**MVP (User Story 1 only)**: Complete Phases 1–3 (T001–T008). This delivers the highest-value fix (read-only collections) and produces a passing test suite. US2 and US3 can follow in subsequent sessions.

**Full delivery**: Complete all phases (T001–T019). Total scope is ≤ 20 tasks concentrated in two files, achievable in a single implementation session.

**Format validation**:
- All tasks start with `- [ ]` checkbox ✅
- All tasks have a sequential ID (T001–T019) ✅
- `[P]` present only on T015 and T017 (different-file/independent tasks) ✅
- `[US1]`, `[US2]`, `[US3]` labels on all user-story phase tasks ✅
- Exact file paths included in every task description ✅
