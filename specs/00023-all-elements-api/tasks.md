# Tasks: First-Class All-Elements Assertions

**Input**: Design documents from `/specs/00023-all-elements-api/`  
**Prerequisites**: plan.md âœ… Â· spec.md âœ… Â· research.md âœ… Â· data-model.md âœ… Â· contracts/ âœ… Â· quickstart.md âœ…

**Scope summary**:
- `src/Assertivo/Collections/GenericCollectionAssertions.cs` - enhance all-elements execution path and add index-aware overload
- `src/Assertivo/Collections/AllSatisfyFailureFormatter.cs` - new internal diagnostics formatter for bounded detail and adaptive index rendering
- `tests/Assertivo.Tests/*.cs` - add focused all-elements behavior, diagnostics, and index-aware regression tests
- `README.md` - add first-class all-elements assertion-body examples

---

## Phase 1: Setup

**Purpose**: Establish baseline and create feature-specific test/implementation scaffolding.

- [x] T001 Run baseline suite using `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj` and capture current status before edits
- [x] T002 Create test scaffolds `tests/Assertivo.Tests/AllSatisfyAssertionsTests.cs`, `tests/Assertivo.Tests/AllSatisfyDiagnosticsTests.cs`, and `tests/Assertivo.Tests/AllSatisfyIndexAwareTests.cs`
- [x] T003 Create implementation scaffold `src/Assertivo/Collections/AllSatisfyFailureFormatter.cs` with namespace and internal formatter type shell

**Checkpoint**: Baseline and scaffolding complete.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build shared infrastructure required by all user stories.

**âš ï¸ CRITICAL**: No user-story implementation should start until this phase is complete.

- [x] T004 Implement shared diagnostics formatter entry points in `src/Assertivo/Collections/AllSatisfyFailureFormatter.cs` (detail capping, index rendering hooks, payload assembly)
- [x] T005 Introduce shared all-elements core execution path in `src/Assertivo/Collections/GenericCollectionAssertions.cs` that will be reused by both overloads
- [x] T006 [P] Create throwing-enumerable test fixture in `tests/Assertivo.Tests/TestUtilities/ThrowingEnumerable.cs`
- [x] T007 [P] Create failing-index parsing/assertion helper in `tests/Assertivo.Tests/TestUtilities/FailingIndexParser.cs`

**Checkpoint**: Shared implementation and reusable test fixtures are ready.

---

## Phase 3: User Story 1 - Assert Every Element with Assertion Bodies (Priority: P1) ðŸŽ¯ MVP

**Goal**: Ensure assertion-body all-elements behavior is first-class for `AllSatisfy(Action<T>)` with correct pass/fail, traversal, null handling, and chaining semantics.

**Independent Test**: Use `AllSatisfy(Action<T>)` on a collection to verify pass, empty-vacuous pass, single/multi failure aggregation, full traversal (no fail-fast), null subject failure behavior, and null inspector argument handling.

### Tests for User Story 1 (write first)

- [x] T008 [P] [US1] Add pass, empty-collection, and chaining tests for `AllSatisfy(Action<T>)` in `tests/Assertivo.Tests/AllSatisfyAssertionsTests.cs`
- [x] T009 [P] [US1] Add single-failure, multi-failure, and no-fail-fast traversal tests in `tests/Assertivo.Tests/CollectionAssertionsTests.cs`
- [x] T010 [US1] Add null-subject and null-inspector behavior tests for `AllSatisfy(Action<T>)` in `tests/Assertivo.Tests/AllSatisfyAssertionsTests.cs`

### Implementation for User Story 1

- [x] T011 [US1] Implement null-inspector `ArgumentNullException` guard and full traversal behavior for `AllSatisfy(Action<T>)` in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T012 [US1] Ensure `AllSatisfy(Action<T>)` preserves fluent return and standard failure pipeline usage in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T013 [US1] Run targeted US1 verification with `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj --filter "FullyQualifiedName~AllSatisfyAssertionsTests|FullyQualifiedName~CollectionAssertionsTests.AllSatisfy"`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Diagnose All-Elements Failures Precisely (Priority: P1)

**Goal**: Provide deterministic, bounded, and complete aggregated diagnostics (ordering, detail cap, index coverage, thrown type, and because propagation).

**Independent Test**: Trigger mixed and large failure sets and verify deterministic ordering, first-50 detail cap, total failed count, full index coverage with adaptive rendering, and framework-compatible final thrown type with `AssertionFailedException` fallback.

### Tests for User Story 2 (write first)

- [x] T014 [P] [US2] Add diagnostics ordering, index-tag, framework-thrown-type (with fallback), and because-propagation tests in `tests/Assertivo.Tests/AllSatisfyDiagnosticsTests.cs`
- [x] T015 [P] [US2] Add large-failure threshold tests (50/51 details and 100/101 indices) in `tests/Assertivo.Tests/AllSatisfyLargeFailureTests.cs`
- [x] T016 [P] [US2] Add source-enumeration passthrough and non-assertion exception detail tests in `tests/Assertivo.Tests/AllSatisfyEnumerationFailureTests.cs`

### Implementation for User Story 2

- [x] T017 [US2] Implement ordered element-failure capture and aggregate summary building in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T018 [US2] Implement first-50 detailed failure rendering and total-failed-count output in `src/Assertivo/Collections/AllSatisfyFailureFormatter.cs`
- [x] T019 [US2] Implement adaptive failing-index rendering (explicit <=100, compressed >100) in `src/Assertivo/Collections/AllSatisfyFailureFormatter.cs`
- [x] T020 [US2] Integrate formatter output into `MessageFormatter.Fail` payload generation in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T021 [US2] Run targeted US2 verification with `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj --filter "FullyQualifiedName~AllSatisfyDiagnosticsTests|FullyQualifiedName~AllSatisfyLargeFailureTests|FullyQualifiedName~AllSatisfyEnumerationFailureTests"`

**Checkpoint**: US2 diagnostics are independently functional and testable.

---

## Phase 5: User Story 3 - Use Index-Aware Assertion Bodies (Priority: P2)

**Goal**: Add first-class `AllSatisfy(Action<T, int>)` for position-sensitive assertion bodies with parity to existing all-elements behavior.

**Independent Test**: Verify index-aware callback receives zero-based indices, failure indices map correctly, null-inspector guard works, and overload is callable across all collection dispatch paths.

### Tests for User Story 3 (write first)

- [x] T022 [P] [US3] Add index-aware pass and index-violation mapping tests in `tests/Assertivo.Tests/AllSatisfyIndexAwareTests.cs`
- [x] T023 [P] [US3] Add index-aware edge-case tests (null subject, null inspector, empty collection, because propagation, chaining) in `tests/Assertivo.Tests/AllSatisfyIndexAwareEdgeCasesTests.cs`
- [x] T024 [P] [US3] Add index-aware dispatch compatibility regressions for `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `List<T>`, and `T[]` in `tests/Assertivo.Tests/ShouldDispatchTests.cs`

### Implementation for User Story 3

- [x] T025 [US3] Add public `AllSatisfy(Action<T, int> inspector, string because = "", params object[] becauseArgs)` overload with XML docs in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T026 [US3] Refactor `AllSatisfy(Action<T>)` to delegate to shared index-aware core execution path in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T027 [US3] Run targeted US3 verification with `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj --filter "FullyQualifiedName~AllSatisfyIndexAwareTests|FullyQualifiedName~AllSatisfyIndexAwareEdgeCasesTests|FullyQualifiedName~ShouldDispatchTests"`

**Checkpoint**: US3 index-aware API is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize documentation, quality gates, and full-system regression confidence.

- [x] T028 [P] Update all-elements usage examples in `README.md` (assertion-body and index-aware overload)
- [x] T029 [P] Align implementation examples and diagnostics wording in `specs/00023-all-elements-api/quickstart.md`
- [x] T030 Run strict compile validation using `dotnet build src/Assertivo/Assertivo.csproj -warnaserror`
- [x] T031 Run core assertion-engine line-coverage gate using `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj /p:CollectCoverage=true /p:Threshold=95 /p:ThresholdType=line /p:ThresholdStat=Total /p:ExcludeByFile="**/*Assertions.cs%2c**/Should.cs"` and fail CI if threshold is missed
- [x] T032 Run full repository tests using `dotnet test Assertivo.slnx`
- [x] T033 Verify maintainability guardrails by checking `src/Assertivo/Collections/GenericCollectionAssertions.cs` remains <=300 lines and diagnostics formatting stays delegated to `src/Assertivo/Collections/AllSatisfyFailureFormatter.cs`
- [x] T034 Run core assertion-engine branch-coverage gate using `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj /p:CollectCoverage=true /p:Threshold=90 /p:ThresholdType=branch /p:ThresholdStat=Total /p:ExcludeByFile="**/*Assertions.cs%2c**/Should.cs"` and fail CI if threshold is missed
- [x] T035 Enforce cyclomatic complexity gate (<=10) for touched methods by adding CA1502 settings in repository-root `.editorconfig` (`dotnet_diagnostic.CA1502.severity = error` and `dotnet_code_quality.CA1502.max_complexity = 10`) and running `dotnet build src/Assertivo/Assertivo.csproj -warnaserror`
- [x] T036 Run extension-method-surface line-coverage gate using `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj /p:CollectCoverage=true /p:Threshold=90 /p:ThresholdType=line /p:ThresholdStat=Total /p:Include="[Assertivo]Assertivo.*Assertions"` and fail CI if threshold is missed
- [x] T037 Add API-surface guard tests in `tests/Assertivo.Tests/AllSatisfyApiSurfaceGuardsTests.cs` to verify no predicate-only all-elements API is introduced (FR-013)
- [x] T038 Add API-surface guard tests in `tests/Assertivo.Tests/AllSatisfyApiSurfaceGuardsTests.cs` to verify no async per-element `AllSatisfy` overload is introduced (FR-014)
- [x] T039 Add non-all-elements `MessageFormatter` regression guards in `tests/Assertivo.Tests/MessageFormatterTests.cs` to verify behavior outside all-elements diagnostics remains unchanged (FR-015)
- [x] T040 Run constitution test-naming compliance gate over `tests/Assertivo.Tests/AllSatisfy*.cs`, `tests/Assertivo.Tests/CollectionAssertionsTests.cs`, `tests/Assertivo.Tests/ShouldDispatchTests.cs`, and `tests/Assertivo.Tests/MessageFormatterTests.cs` to enforce class-name mirroring with `Tests` suffix and method pattern `MethodName_Scenario_ExpectedOutcome`, and fail CI on violations
- [x] T041 Add explicit FR-018 API-surface guard tests in `tests/Assertivo.Tests/AllSatisfyApiSurfaceGuardsTests.cs` to verify no new public aggregated-failure exception type is introduced and aggregated `AllSatisfy` failures use framework assertion exception surfaces with `AssertionFailedException` fallback

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 - blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2.
- **Phase 5 (US3)**: Depends on Phase 2.
- **Phase 6 (Polish)**: Depends on completion of all selected user stories.

### User Story Dependencies

- **US1 (P1)**: Depends only on Foundational phase.
- **US2 (P1)**: Depends only on Foundational phase; shares core files with US1, so sequence carefully if multiple developers work concurrently.
- **US3 (P2)**: Depends only on Foundational phase; can proceed independently once shared core path exists.

### Within Each User Story

- Tests MUST be created before implementation tasks.
- Overload behavior tests before formatter/output implementation.
- Source code updates before targeted verification command.

### Parallel Opportunities

- **Foundational**: T006 and T007 can run in parallel (different helper files).
- **US1**: T008 and T009 can run in parallel (different test files).
- **US2**: T014, T015, and T016 can run in parallel (different test files).
- **US3**: T022, T023, and T024 can run in parallel (different test files).
- **Polish**: T028 and T029 can run in parallel (different docs files).

---

## Parallel Examples Per User Story

### Parallel Example - US1

```text
Stream A (US1 tests):
  T008 -> T010

Stream B (US1 tests in existing suite):
  T009

Join:
  T011 -> T012 -> T013
```

### Parallel Example - US2

```text
Stream A (diagnostics behavior tests):
  T014

Stream B (large-threshold tests):
  T015

Stream C (enumeration/exception-path tests):
  T016

Join:
  T017 -> T018 -> T019 -> T020 -> T021
```

### Parallel Example - US3

```text
Stream A (index-aware core behavior tests):
  T022

Stream B (index-aware edge-case tests):
  T023

Stream C (dispatch compatibility tests):
  T024

Join:
  T025 -> T026 -> T027
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate US1 independently via T013.
4. Pause for review/demo before expanding diagnostics scope.

### Incremental Delivery

1. Deliver US1 as core all-elements assertion-body reliability.
2. Add US2 for deterministic, bounded diagnostics.
3. Add US3 for index-aware assertion bodies.
4. Finish cross-cutting polish and full regression gates.

### Suggested MVP Scope

- **Recommended MVP**: Phase 1 + Phase 2 + Phase 3 (T001-T013).

### Format Validation

- All tasks use `- [ ]` checkbox format.
- All tasks use sequential IDs (`T001` through `T041`).
- `[P]` is applied only to tasks that can safely run in parallel on different files.
- `[US1]`, `[US2]`, `[US3]` labels are present on user-story tasks only.
- Every task description includes explicit file paths or command targets.
