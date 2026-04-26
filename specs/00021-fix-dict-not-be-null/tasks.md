# Tasks: Collection and Dictionary Null-Guard Assertions (BeNull / NotBeNull)

**Input**: Design documents from `/specs/00021-fix-dict-not-be-null/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · contracts/public-api.md ✅

**Tests**: Included — spec acceptance criteria (SC-002–SC-010) require explicit test coverage.

**Organization**: Tasks grouped by user story. US1 (dictionary) and US2 (collection) touch different files and can be implemented fully in parallel. US3 (`because` message) is delivered as part of US1/US2 — no separate implementation phase needed.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to
- Each task includes an exact file path

---

## Phase 1: Setup

> **N/A** — No project initialization required. All changes are additive within existing files. No new projects, packages, or directories are created.

---

## Phase 2: Foundational

> **N/A** — No blocking prerequisites. `MessageFormatter`, `AndConstraint<T>`, and the `Should.cs` dispatch overloads are all already in place. US1 and US2 can begin immediately and in parallel.

---

## Phase 3: User Story 1 — Dictionary Null-Guard Assertions (Priority: P1) 🎯 MVP

**Goal**: Add `NotBeNull()` and `BeNull()` to `GenericDictionaryAssertions<TKey, TValue>` so that null-guarding a dictionary compiles, runs correctly, and chains into further dictionary assertions.

**Independent Test**: Run `dotnet test --filter "DictionaryAssertionsTests"` — all existing tests must still pass AND the ten new tests (T003–T009, T022, T024, T025) must pass.

**US3 coverage note**: `NotBeNull_NullDictionary_WithBecause_IncludesReasonInMessage` (T009) covers US3 scenario 1 (`NotBeNull`+`because`). `BeNull_NonNullDictionary_WithBecause_IncludesReasonInMessage` (T022) covers US3 scenario 3 (`BeNull`+`because`). US3 is fully covered for this type.

### Implementation for User Story 1

- [X] T001 [US1] Add `NotBeNull(string because = "", params object[] becauseArgs)` with `[StackTraceHidden]` attribute and full XML doc (`<summary>`, `<param>`, `<returns>`) to `src/Assertivo/Collections/GenericDictionaryAssertions.cs` — expected string `"not <null>"`, actual string `"<null>"`, returns `new AndConstraint<GenericDictionaryAssertions<TKey, TValue>>(this)`
- [X] T002 [US1] Add `BeNull(string because = "", params object[] becauseArgs)` with `[StackTraceHidden]` attribute and full XML doc (`<summary>`, `<param>`, `<returns>`) to `src/Assertivo/Collections/GenericDictionaryAssertions.cs` — expected string `"<null>"`, actual string `MessageFormatter.FormatValue(Subject)`, returns `new AndConstraint<GenericDictionaryAssertions<TKey, TValue>>(this)`

### Tests for User Story 1

- [X] T003 [US1] Add `NotBeNull_NonNullDictionary_Passes` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: non-null `IReadOnlyDictionary<string, int>`; act: `.Should().NotBeNull()`; assert: no exception thrown (SC-002)
- [X] T004 [US1] Add `NotBeNull_NullDictionary_Fails` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: `IReadOnlyDictionary<string, int>? dict = null`; act: `Assert.Throws<AssertionFailedException>`; assert: `ex.Expected == "not <null>"` and `ex.Actual == "<null>"` (SC-003)
- [X] T005 [US1] Add `NotBeNull_Chaining_ContainKey` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: non-null `IReadOnlyDictionary<string, int>` with known key; act: `.Should().NotBeNull().And.ContainKey("key")`; assert: no exception thrown at runtime (SC-004)
- [X] T006 [US1] Add `BeNull_NullDictionary_Passes` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: `IDictionary<string, int>? dict = null`; act: `.Should().BeNull()`; assert: no exception thrown (SC-005)
- [X] T007 [US1] Add `BeNull_NonNullDictionary_Fails` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: non-null `IDictionary<string, int>`; act: `Assert.Throws<AssertionFailedException>`; assert: `ex.Expected == "<null>"` (SC-006)
- [X] T008 [US1] Add `BeNull_NullDictionary_Chaining` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: `IDictionary<string, int>? dict = null`; act: `.Should().BeNull().And` resolves to `GenericDictionaryAssertions<string, int>`; assert: compiles, no exception thrown
- [X] T009 [US1] Add `NotBeNull_NullDictionary_WithBecause_IncludesReasonInMessage` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: null dict; act: `.Should().NotBeNull("the config must be loaded")`; assert: `ex.Message` contains `"the config must be loaded"` (US3/SC from spec §US-3 scenario 1)
- [X] T022 [US1] Add `BeNull_NonNullDictionary_WithBecause_IncludesReasonInMessage` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: non-null `IDictionary<string, int>`; act: `.Should().BeNull("the config must be initialised")`; assert: `ex.Message` contains `"the config must be initialised"` (US3/SC from spec §US-3 scenario 3 — fixes C1)
- [X] T024 [US1] Add `NotBeNull_NonNullConcreteDictionary_Passes` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: `new Dictionary<string, int>()`; act: `.Should().NotBeNull()`; assert: no exception thrown — verifies FR-008 coverage for concrete `Dictionary<K,V>` dispatch (fixes C2)
- [X] T025 [US1] Add `NotBeNull_IEnumerableKeyValuePair_Passes` to `tests/Assertivo.Tests/DictionaryAssertionsTests.cs` — arrange: `IEnumerable<KeyValuePair<string, int>>` reference via a `List<KeyValuePair<string,int>>`; act: `.Should().NotBeNull()`; assert: no exception thrown — verifies FR-008 coverage for the third dispatch overload (fixes C3)

**Checkpoint**: `dotnet test --filter "DictionaryAssertionsTests"` is fully green — all existing + ten new tests pass. User Story 1 complete and independently testable.

---

## Phase 4: User Story 2 — Collection Null-Guard Assertions (Priority: P1)

**Goal**: Add `NotBeNull()` and `BeNull()` to `GenericCollectionAssertions<T>` so that null-guarding any `IEnumerable<T>` compiles, runs correctly, and chains into further collection assertions.

**Independent Test**: Run `dotnet test --filter "CollectionAssertionsTests"` — all existing tests must still pass AND the eight new tests (T012–T018, T023) must pass.

**US3 coverage note**: `NotBeNull_NullCollection_WithBecause_IncludesReasonInMessage` (T018) covers US3 scenario 2 (`NotBeNull`+`because`). `BeNull_NonNullCollection_WithBecause_IncludesReasonInMessage` (T023) covers US3 scenario 4 (`BeNull`+`because`). US3 is fully covered for this type.

**Parallelism**: Phase 4 may begin immediately alongside Phase 3 — `GenericCollectionAssertions.cs` and `CollectionAssertionsTests.cs` are entirely separate files from Phase 3's targets.

### Implementation for User Story 2

- [X] T010 [P] [US2] Add `NotBeNull(string because = "", params object[] becauseArgs)` with `[StackTraceHidden]` attribute and full XML doc (`<summary>`, `<param>`, `<returns>`) to `src/Assertivo/Collections/GenericCollectionAssertions.cs` — expected string `"not <null>"`, actual string `"<null>"`, returns `new AndConstraint<GenericCollectionAssertions<T>>(this)`
- [X] T011 [P] [US2] Add `BeNull(string because = "", params object[] becauseArgs)` with `[StackTraceHidden]` attribute and full XML doc (`<summary>`, `<param>`, `<returns>`) to `src/Assertivo/Collections/GenericCollectionAssertions.cs` — expected string `"<null>"`, actual string `MessageFormatter.FormatValue(Subject)`, returns `new AndConstraint<GenericCollectionAssertions<T>>(this)`

### Tests for User Story 2

- [X] T012 [P] [US2] Add `NotBeNull_NonNullCollection_Passes` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: non-null `IEnumerable<int>`; act: `.Should().NotBeNull()`; assert: no exception thrown (SC-007)
- [X] T013 [P] [US2] Add `NotBeNull_NullCollection_Fails` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: `IEnumerable<int>? list = null`; act: `Assert.Throws<AssertionFailedException>`; assert: `ex.Expected == "not <null>"` and `ex.Actual == "<null>"` (SC-008)
- [X] T014 [P] [US2] Add `NotBeNull_Chaining_HaveCount` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: non-null `IEnumerable<int>` with 3 elements; act: `.Should().NotBeNull().And.HaveCount(3)`; assert: compiles and no exception thrown
- [X] T015 [P] [US2] Add `BeNull_NullCollection_Passes` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: `IEnumerable<int>? list = null`; act: `.Should().BeNull()`; assert: no exception thrown (SC-009)
- [X] T016 [P] [US2] Add `BeNull_NonNullCollection_Fails` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: non-null `List<int>`; act: `Assert.Throws<AssertionFailedException>`; assert: `ex.Expected == "<null>"` (SC-010)
- [X] T017 [P] [US2] Add `BeNull_NullCollection_Chaining` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: `IEnumerable<int>? list = null`; act: `.Should().BeNull().And` resolves to `GenericCollectionAssertions<int>`; assert: compiles, no exception thrown
- [X] T018 [P] [US2] Add `NotBeNull_NullCollection_WithBecause_IncludesReasonInMessage` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: null collection; act: `.Should().NotBeNull("the list must be populated")`; assert: `ex.Message` contains `"the list must be populated"` (US3/SC from spec §US-3 scenario 2)
- [X] T023 [P] [US2] Add `BeNull_NonNullCollection_WithBecause_IncludesReasonInMessage` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs` — arrange: non-null `List<int>`; act: `.Should().BeNull("the list must be populated")`; assert: `ex.Message` contains `"the list must be populated"` (US3/SC from spec §US-3 scenario 4 — fixes C1)

**Checkpoint**: `dotnet test --filter "CollectionAssertionsTests"` is fully green — all existing + eight new tests pass. User Story 2 complete and independently testable.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final compile + full test-suite gate verifying no regressions and no build warnings.

- [X] T019 Run `dotnet build src/Assertivo/Assertivo.csproj --no-incremental` and confirm zero warnings and zero errors (`TreatWarningsAsErrors=true`; `Nullable=enable`; `net10.0`) — satisfies SC-011
- [X] T020 Run `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj` (full suite, no filter) and confirm all tests pass including the existing `DictionaryAssertionsTests` and `CollectionAssertionsTests` methods — satisfies SC-001
- [X] T021 Verify quickstart.md examples in `specs/00021-fix-dict-not-be-null/quickstart.md` by tracing each code block against the implementation in `src/Assertivo/Collections/GenericDictionaryAssertions.cs` and `src/Assertivo/Collections/GenericCollectionAssertions.cs`. As an automated alternative, extend `tests/Validation/QuickstartSmoke/` with the new null-guard scenarios and run it as part of this gate.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: N/A
- **Phase 2 (Foundational)**: N/A
- **Phase 3 (US1 — Dictionary)**: No dependencies — can start immediately
- **Phase 4 (US2 — Collection)**: No dependencies on Phase 3 — can start immediately in parallel
- **Phase 5 (Polish)**: Depends on Phase 3 AND Phase 4 being complete

### User Story Dependencies

- **US1 (P1 — Dictionary)**: Independent — no dependencies on US2 or US3
- **US2 (P1 — Collection)**: Independent — no dependencies on US1 or US3
- **US3 (P2 — because message)**: Delivered inside US1 (T009, T022) and US2 (T018, T023); no additional implementation phase required

### Within Each User Story

- Implementation tasks (T001–T002, T010–T011) before test tasks in the same type
- Tests for the same type are sequential (same file); tests for different types are parallel
- All tests must pass before moving to Polish

---

## Parallel Opportunities

### Across user stories (different files — fully parallelizable)

```
Phase 3: T001 → T002 → T003–T009, T022, T024, T025   (GenericDictionaryAssertions.cs + DictionaryAssertionsTests.cs)
Phase 4: T010 → T011 → T012–T018, T023               (GenericCollectionAssertions.cs + CollectionAssertionsTests.cs)
```

Both phases can run simultaneously from the start.

### Within User Story 2 (all marked [P])

Tasks T010–T018 are all marked `[P]` because they are in files completely separate from Phase 3. Once the decision is taken to work on US2, all of them can begin independently of Phase 3's progress.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 3 (T001–T009) — dictionary null-guard fix
2. Run `dotnet test --filter "DictionaryAssertionsTests"` — must be fully green
3. **STOP and VALIDATE**: the original reported bug (`IReadOnlyDictionary.Should().NotBeNull()` compiler error) is now resolved
4. Ship or continue to US2

### Incremental Delivery

1. Phase 3 complete → Dictionary null-guard shipped (MVP — fixes the reported bug)
2. Phase 4 complete → Collection null-guard shipped (closes sibling bug proactively)
3. Phase 5 complete → Full-suite regression gate passes → branch ready for PR

### Single-Developer (Sequential) Path

```
T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008 → T009 → T022 → T024 → T025
     → T010 → T011 → T012 → T013 → T014 → T015 → T016 → T017 → T018 → T023
     → T019 → T020 → T021
```

Total: 25 tasks · ~3–4 hours implementation time
