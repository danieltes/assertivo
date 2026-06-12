# Tasks: Float/Double/Decimal Should Dispatch

**Input**: Design documents from `specs/00049-float-double-decimal-should/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/public-api.md ✓

**Tests**: Included — required by spec FR-010 and SC-004 (90% line coverage gate).

**Organization**: Tasks grouped by user story. The core implementation (Phase 2) is foundational and unlocks all test phases.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (independent of other in-progress tasks)
- **[Story]**: Which user story this task belongs to (US1/US2/US3)

---

## Phase 1: Setup

**Purpose**: Confirm baseline before making changes

- [x] T001 Verify solution builds clean on branch `00049-float-double-decimal-should` (`dotnet build Assertivo.slnx`)

---

## Phase 2: Foundational — Core Dispatch Fix

**Purpose**: The 3 new extension method overloads that unblock all user story test phases

**⚠️ CRITICAL**: No user story test tasks can compile until this phase is complete

- [x] T002 Add `float`, `double`, and `decimal` `Should()` overloads to `src/Assertivo/Should.cs` immediately after the `long` overload — each returning `new NumericAssertions<T>(subject, caller)` with `[CallerArgumentExpression(nameof(subject))] string? caller = null`
- [x] T003 Add XML documentation comment (`/// <summary>…</summary>`) to each of the three new overloads in `src/Assertivo/Should.cs` (required by constitution §III.2)
- [x] T004 Verify solution builds with zero warnings after the overloads are added (`dotnet build Assertivo.slnx --configuration Release`); confirm `src/Assertivo/Numeric/NumericAssertions.cs` has no changes in the diff (SC-005)

**Checkpoint**: Solution compiles — all test phases can now proceed

---

## Phase 3: User Story 1 — Range Assertions (Priority: P1) 🎯 MVP

**Goal**: `BeGreaterThanOrEqualTo` and `BeLessThan` compile and behave correctly for `float`, `double`, and `decimal`

**Independent Test**: `dotnet test --filter "FullyQualifiedName~NumericAssertionsTests"` — all new range tests pass

- [x] T005 [P] [US1] Add happy-path tests `BeGreaterThanOrEqualTo_WhenFloatMeetsThreshold_Passes` and `BeLessThan_WhenFloatBelowThreshold_Passes` in `tests/Assertivo.Tests/NumericAssertionsTests.cs`
- [x] T006 [P] [US1] Add happy-path tests `BeGreaterThanOrEqualTo_WhenDoubleMeetsThreshold_Passes` and `BeLessThan_WhenDoubleBelowThreshold_Passes` in `tests/Assertivo.Tests/NumericAssertionsTests.cs`
- [x] T007 [P] [US1] Add happy-path tests `BeGreaterThanOrEqualTo_WhenDecimalMeetsThreshold_Passes` and `BeLessThan_WhenDecimalBelowThreshold_Passes` in `tests/Assertivo.Tests/NumericAssertionsTests.cs`
- [x] T008 [P] [US1] Add failure-path tests `BeGreaterThanOrEqualTo_WhenFloatBelowThreshold_ThrowsWithMessage` and `BeLessThan_WhenFloatAtOrAboveThreshold_ThrowsWithMessage` in `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert message contains actual value, expected threshold
- [x] T009 [P] [US1] Add failure-path tests `BeGreaterThanOrEqualTo_WhenDoubleBelowThreshold_ThrowsWithMessage` and `BeLessThan_WhenDoubleAtOrAboveThreshold_ThrowsWithMessage` in `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert message contains actual value and expected threshold
- [x] T010 [P] [US1] Add failure-path tests `BeGreaterThanOrEqualTo_WhenDecimalBelowThreshold_ThrowsWithMessage` and `BeLessThan_WhenDecimalAtOrAboveThreshold_ThrowsWithMessage` in `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert message contains actual value and expected threshold
- [x] T011 [P] [US1] Add caller-expression capture tests `BeLessThan_FailureMessage_ContainsCallerExpression_ForDouble`, `BeLessThan_FailureMessage_ContainsCallerExpression_ForFloat`, and `BeLessThan_FailureMessage_ContainsCallerExpression_ForDecimal` in `tests/Assertivo.Tests/NumericAssertionsTests.cs` — for each: declare a named variable, call `.Should().BeLessThan(...)`, assert the failure message contains the variable name (FR-008)

**Checkpoint**: User Story 1 fully functional — range assertions on `float`, `double`, `decimal` work end-to-end

---

## Phase 4: User Story 2 — Equality Assertions (Priority: P2)

**Goal**: `Be` and `NotBe` work for `float`, `double`, and `decimal`

**Independent Test**: `dotnet test --filter "FullyQualifiedName~NumericAssertionsTests"` — all new equality tests pass

- [x] T012 [P] [US2] Add happy-path tests `Be_WhenFloatValuesAreEqual_Passes` and `NotBe_WhenFloatValuesDiffer_Passes` in `tests/Assertivo.Tests/NumericAssertionsTests.cs`
- [x] T013 [P] [US2] Add happy-path tests for `double` equality in `tests/Assertivo.Tests/NumericAssertionsTests.cs`
- [x] T014 [P] [US2] Add happy-path tests for `decimal` equality in `tests/Assertivo.Tests/NumericAssertionsTests.cs`
- [x] T015 [P] [US2] Add failure-path tests `Be_WhenFloatValuesDiffer_ThrowsWithMessage`, `Be_WhenDoubleValuesDiffer_ThrowsWithMessage`, and `Be_WhenDecimalValuesDiffer_ThrowsWithMessage` in `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert each message contains actual and expected values (matching int/long format per SC-003)
- [x] T021 [P] [US2] Add failure-path tests `NotBe_WhenFloatValuesAreEqual_ThrowsWithMessage`, `NotBe_WhenDoubleValuesAreEqual_ThrowsWithMessage`, and `NotBe_WhenDecimalValuesAreEqual_ThrowsWithMessage` in `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert each message contains the actual value (FR-006)

**Checkpoint**: User Stories 1 and 2 both independently functional

---

## Phase 5: User Story 3 — Dispatch Type Verification (Priority: P3)

**Goal**: `.Should()` on `float`, `double`, `decimal` returns `NumericAssertions<T>`, never `ObjectAssertions<T>`

**Independent Test**: `dotnet test --filter "FullyQualifiedName~ShouldDispatchTests"` — all new dispatch tests pass

- [x] T016 [P] [US3] Add dispatch type tests `Should_OnFloat_ReturnsNumericAssertionsFloat`, `Should_OnDouble_ReturnsNumericAssertionsDouble`, `Should_OnDecimal_ReturnsNumericAssertionsDecimal` in `tests/Assertivo.Tests/ShouldDispatchTests.cs` — use `Assert.IsType<NumericAssertions<T>>(subject.Should())`
- [x] T017 [P] [US3] Add chaining tests `Should_OnDouble_SupportsAndChaining` (e.g., `(3.14).Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(4.0)`) and equivalent for `float`/`decimal` in `tests/Assertivo.Tests/NumericAssertionsTests.cs`

**Checkpoint**: All three user stories fully functional and independently verified

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and coverage gate

- [x] T018 Run full test suite and confirm all new and existing tests pass (`dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj`)
- [x] T019 [P] Run coverage report and verify ≥90% line coverage for `src/Assertivo/Should.cs` (`dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage`)
- [x] T020 [P] Validate quickstart scenarios from `specs/00049-float-double-decimal-should/quickstart.md` — run the compile-time and happy/failure path scenarios manually

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **BLOCKS all test phases**
- **Phase 3 (US1)**: Depends on Phase 2 — T005–T011 can proceed in parallel once T004 is green
- **Phase 4 (US2)**: Depends on Phase 2 — can run in parallel with Phase 3 once T004 is green
- **Phase 5 (US3)**: Depends on Phase 2 — can run in parallel with Phases 3 and 4 once T004 is green
- **Phase 6 (Polish)**: Depends on all desired user story phases being complete

### Within Phase 2

- T002 → T003 → T004 (sequential; all modify `src/Assertivo/Should.cs`)

### Within User Story Phases

- All [P]-marked tasks within a phase are independent of each other (different test methods, same file)
- T011 (caller expression test) is [P] — independent of T008–T010; the suggested ordering is after the failure-path tests, but it is not blocked by them

### Parallel Opportunities

```
# After T004 completes, launch all Phase 3/4/5 test tasks in parallel:

Phase 3: T005, T006, T007, T008, T009, T010, T011 (all [P] within US1)
Phase 4: T012, T013, T014, T015, T021 (all [P] within US2)
Phase 5: T016, T017 (all [P] within US3)

# Then T018, T019, T020 (Phase 6 — all [P] except T018)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Verify build
2. Complete Phase 2: Add the 3 overloads — **this is the entire implementation** (3 extension methods)
3. Complete Phase 3: US1 range assertion tests
4. **STOP and VALIDATE**: `dotnet test --filter "FullyQualifiedName~NumericAssertionsTests"` passes
5. The primary reported gap (compile error on `BeGreaterThanOrEqualTo` for `double`) is fixed

### Incremental Delivery

1. Phase 1 + Phase 2 → Fix compiles, all 5 primitive numeric types route correctly (SC-002)
2. Phase 3 → Range assertion tests pass (SC-001, SC-003, SC-004 for US1)
3. Phase 4 → Equality assertion tests pass (US2 coverage)
4. Phase 5 → Dispatch type tests confirm no `ObjectAssertions<T>` fallback (SC-002 verified)
5. Phase 6 → Coverage gate + quickstart validation

---

## Notes

- [P] tasks = independent, no shared in-progress state
- Phase 2 is the entire production code change — just 3 extension method overloads in one file
- Tests cannot compile until Phase 2 (T002–T003) is complete
- Follow existing test method naming: `MethodName_Scenario_ExpectedOutcome`
- Commit after Phase 2 and after each user story phase completes
- Constitution §IV.2: test class `NumericAssertionsTests` mirrors `NumericAssertions.cs`; `ShouldDispatchTests` mirrors `Should.cs` dispatch routing
