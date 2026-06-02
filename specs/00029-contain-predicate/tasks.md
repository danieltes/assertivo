# Tasks: Predicate-Based Collection Containment

**Feature**: `00029-contain-predicate` | **Branch**: `00029-contain-predicate`  
**Input**: [spec.md](spec.md) · [plan.md](plan.md) · [research.md](research.md) · [data-model.md](data-model.md) · [contracts/public-api.md](contracts/public-api.md) · [quickstart.md](quickstart.md)

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel with other [P] tasks in the same phase (different files, no blocking dependency)
- **[US1/US2/US3]**: User story this task belongs to
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish a clean baseline and confirm target files for additive API work.

- [X] T001 Run baseline build using Assertivo.slnx and append one evidence line in specs/00029-contain-predicate/tasks.md under this task as BaselineBuild: PASS or FAIL, with UTC timestamp and short commit SHA
	BaselineBuild: PASS | UTC: 2026-06-02T13:46:14Z | SHA: e73f165
- [X] T002 Verify target implementation and test locations in src/Assertivo/Collections/GenericCollectionAssertions.cs and tests/Assertivo.Tests/CollectionAssertionsTests.cs

**Checkpoint**: Baseline is green and implementation surface is confirmed.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define shared contracts and behavior expectations that all user stories rely on.

- [X] T003 Verify no-match expected/actual message contract remains aligned with spec and quickstart in specs/00029-contain-predicate/contracts/public-api.md
- [X] T004 Verify null-predicate throw ordering contract remains aligned with spec and quickstart in specs/00029-contain-predicate/contracts/public-api.md
- [X] T005 Verify traceability matrix mapping FR-001..FR-010 and SC-001..SC-005 to acceptance scenarios in specs/00029-contain-predicate/contracts/public-api.md

**Checkpoint**: Contract behavior is fixed and all user stories can implement against one source of truth.

---

## Phase 3: User Story 1 - Predicate-Based Collection Containment (Priority: P1) 🎯 MVP

**Goal**: Implement and validate Contain(predicate) so assertions pass with one-or-more matches and fail when no element matches.

**Independent Test**: In tests/Assertivo.Tests/CollectionAssertionsTests.cs, run tests covering one match, no match, multiple matches, empty collection, and null subject.

### Tests for User Story 1

- [X] T006 [US1] Add pass/fail predicate containment tests in tests/Assertivo.Tests/CollectionAssertionsTests.cs for one-match, no-match, and multiple-match scenarios, asserting expected/actual failure message content for no-match
- [X] T007 [US1] Add empty-collection, null-subject, and null-predicate-ordering tests in tests/Assertivo.Tests/CollectionAssertionsTests.cs, including a case that proves null predicate throws ArgumentNullException before subject null-guard evaluation

### Implementation for User Story 1

- [X] T008 [US1] Add Contain(Func<T, bool> predicate, string because = "", params object[] becauseArgs) overload in src/Assertivo/Collections/GenericCollectionAssertions.cs, including guard order, one-or-more match semantics, and no-match failure messaging via MessageFormatter.Fail
- [X] T009 [US1] Add or update XML documentation comments for the new public Contain(predicate) overload in src/Assertivo/Collections/GenericCollectionAssertions.cs (summary, params, returns, exception)

**Checkpoint**: US1 is independently functional and testable as MVP behavior.

---

## Phase 4: User Story 2 - Informative Failure Messages with because (Priority: P2)

**Goal**: Ensure because/becauseArgs are preserved in failure output for predicate containment.

**Independent Test**: In tests/Assertivo.Tests/CollectionAssertionsTests.cs, verify because text appears in failing Contain(predicate, because, becauseArgs) messages.

### Tests for User Story 2

- [X] T010 [US2] Add failing predicate-containment reason-formatting tests in tests/Assertivo.Tests/CollectionAssertionsTests.cs, asserting Because text inclusion and formatted becauseArgs output

### Implementation for User Story 2

- [X] T011 [US2] Ensure because and becauseArgs are forwarded on predicate no-match failure in src/Assertivo/Collections/GenericCollectionAssertions.cs

**Checkpoint**: US2 is independently testable and confirms diagnostic reason behavior.

---

## Phase 5: User Story 3 - Assertion Chaining (Priority: P3)

**Goal**: Preserve fluent continuation so Contain(predicate).And.HaveCount(n) compiles and executes correctly.

**Independent Test**: In tests/Assertivo.Tests/CollectionAssertionsTests.cs, assert chaining after passing predicate containment compiles and evaluates both assertions.

### Tests for User Story 3

- [X] T012 [US3] Add chaining test for Contain(predicate).And.HaveCount(n) in tests/Assertivo.Tests/CollectionAssertionsTests.cs

### Implementation for User Story 3

- [X] T013 [US3] Confirm AndConstraint<GenericCollectionAssertions<T>> return contract for predicate overload in src/Assertivo/Collections/GenericCollectionAssertions.cs

**Checkpoint**: US3 is independently testable and fluent chaining is validated.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Verify feature completeness, API contract sync, and regression safety across stories.

- [X] T014 Update feature quickstart examples to reflect final implementation behavior in specs/00029-contain-predicate/quickstart.md
- [X] T015 [P] Verify and sync API signature, behavior notes, and XML-documentation completeness for the new public overload across specs/00029-contain-predicate/contracts/public-api.md and src/Assertivo/Collections/GenericCollectionAssertions.cs
- [X] T016 [P] Execute targeted collection assertion tests in tests/Assertivo.Tests/CollectionAssertionsTests.cs and confirm all new scenarios pass
	TargetedResult: PASS | Command: dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj --filter "FullyQualifiedName~CollectionAssertionsTests.Contain_Predicate" /p:CollectCoverage=false | Tests: 9 passed, 0 failed
- [X] T017 Execute full test suite regression run from tests/Assertivo.Tests/Assertivo.Tests.csproj and record outcome in specs/00029-contain-predicate/tasks.md
	RegressionResult: PASS | Command: dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj | Tests: 315 passed, 0 failed | Coverage: Line 93.81%, Branch 96.87%, Method 100%

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies
- **Phase 2 (Foundational)**: Depends on Phase 1
- **Phase 3 (US1)**: Depends on Phase 2
- **Phase 4 (US2)**: Depends on Phase 3 (reuses US1 overload)
- **Phase 5 (US3)**: Depends on Phase 3 (reuses US1 overload)
- **Final Phase (Polish)**: Depends on completion of US1, US2, US3

### User Story Dependencies

- **US1 (P1)**: Independent after Foundational; delivers MVP
- **US2 (P2)**: Depends on US1 overload existence but validates diagnostics independently
- **US3 (P3)**: Depends on US1 overload existence but validates chaining independently

### Within Each User Story

- Tests first (Red), then implementation adjustments
- Assertion behavior implementation before polishing docs/contracts
- Story checkpoint must pass before moving to cross-cutting completion

---

## Parallel Opportunities

- T015 and T016 can run in parallel during Final Phase (different files, no blocking dependency)

---

## Parallel Example: User Story 1

```bash
# No safe [P] tasks in US1 because all changes target the same source/test files.
# Recommended execution: T006 -> T007 -> T008 -> T009
```

## Parallel Example: User Story 2

```bash
# No safe [P] tasks in US2 because test and implementation adjustments target files used by other story tasks.
# Recommended execution: T010 -> T011
```

## Parallel Example: User Story 3

```bash
# No safe [P] tasks in US3 because test and implementation adjustments target files used by other story tasks.
# Recommended execution: T012 -> T013
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1 and Phase 2
2. Complete Phase 3 (US1)
3. Validate US1 tests and behavior
4. Stop for MVP review/demo

### Incremental Delivery

1. Deliver US1 (core predicate containment)
2. Add US2 (because diagnostics)
3. Add US3 (fluent chaining)
4. Finish polish and regression

### Parallel Team Strategy

1. One developer implements overload + no-match behavior (T008, T009)
2. One developer prepares US2 diagnostics tests (T010)
3. One developer prepares US3 chaining test (T012)
4. Merge into final polish/test run tasks (T015-T017)

---

## Summary

| Metric | Value |
|---|---|
| Total tasks | 17 |
| Setup tasks | 2 |
| Foundational tasks | 3 |
| US1 tasks | 4 |
| US2 tasks | 2 |
| US3 tasks | 2 |
| Final phase tasks | 4 |
| Parallel-marked tasks | 2 |
| MVP scope | Phase 3 (US1) after Setup + Foundational |

## Format Validation

All tasks follow the required checklist format:

- Markdown checkbox prefix: `- [ ]`
- Sequential task IDs: `T001` to `T017`
- `[P]` marker applied only to parallelizable tasks
- `[US#]` labels applied only to user-story phase tasks
- Each task description includes a concrete file path
