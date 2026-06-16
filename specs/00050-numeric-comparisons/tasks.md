# Tasks: Add BeGreaterThan and BeLessThanOrEqualTo to NumericAssertions

**Input**: Design documents from `/specs/00050-numeric-comparisons/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, contracts/ ✓, quickstart.md ✓

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different logical additions to the same file; no ordering dependency)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Tests are included because the spec explicitly requires them and the constitution mandates spec-driven Red→Green→Refactor

## Path Conventions

Single project: `src/Assertivo/`, `tests/Assertivo.Tests/` at repository root.

---

## Phase 1: Setup

**Purpose**: Confirm the implementation baseline before adding new methods.

- [x] T001 Review `BeGreaterThanOrEqualTo` and `BeLessThan` implementations in `src/Assertivo/Numeric/NumericAssertions.cs` to confirm the four-part pattern (null-coalesce comparer → compare → Fail on violation → return AndConstraint) that both new methods must mirror

**Checkpoint**: Implementation pattern confirmed — user story phases can begin.

---

## Phase 2: User Story 1 — Assert Strict Lower Bound / BeGreaterThan (Priority: P1) 🎯 MVP

**Goal**: Deliver `BeGreaterThan` so a developer can write `result.Should().BeGreaterThan(minValue)` and receive a precise failure message when the assertion fails.

**Independent Test**: Run `dotnet test --filter "BeGreaterThan"` after T008 — all six test methods must pass with no impact on the rest of the suite.

### Tests for User Story 1 (Red — write first, confirm they fail)

- [x] T002 [P] [US1] Add `BeGreaterThan_WhenGreater_Passes` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `5.Should().BeGreaterThan(3)` must pass and return an `AndConstraint`
- [x] T003 [P] [US1] Add `BeGreaterThan_WhenEqual_Fails` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `3.Should().BeGreaterThan(3)` must throw `AssertionFailedException`; assert `Assert.Contains("a value greater than", ex.Expected)` and `Assert.Equal("3", ex.Actual)` to validate the exact FR-008 format (not just that the numbers appear somewhere in the message)
- [x] T004 [P] [US1] Add `BeGreaterThan_WhenLess_Fails` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `2.Should().BeGreaterThan(3)` must throw `AssertionFailedException`; assert `ex.Message` contains threshold and actual values
- [x] T005 [P] [US1] Add `BeGreaterThan_WithCustomComparer_UsesComparer` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — pass an inverted `Comparer<int>` that treats higher numbers as lower; assert `5.Should().BeGreaterThan(3, comparer: invertedComparer)` fails (because inverted comparer sees 5 as less than 3)
- [x] T006 [P] [US1] Add `BeGreaterThan_WithNullComparer_FallsBackToDefault` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `5.Should().BeGreaterThan(3, comparer: null)` must pass without throwing
- [x] T007 [P] [US1] Add `BeGreaterThan_WithBecause_IncludesReasonInMessage` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert that `2.Should().BeGreaterThan(3, because: "the result must exceed zero")` throws and `ex.Message` contains `"the result must exceed zero"`

### Implementation for User Story 1 (Green)

- [x] T008 [US1] Add `BeGreaterThan` method to `NumericAssertions<T>` in `src/Assertivo/Numeric/NumericAssertions.cs`: decorate with `[StackTraceHidden]`, null-coalesce comparer to `Comparer<T>.Default`, fail with `MessageFormatter.Fail($"a value greater than {MessageFormatter.FormatValue(value)}", ...)` when `comparer.Compare(Subject, value) <= 0`, return `new AndConstraint<NumericAssertions<T>>(this)`; add XML `<summary>` doc; run `dotnet test --filter "BeGreaterThan"` to confirm T002–T007 all pass

**Checkpoint**: `BeGreaterThan` fully functional and independently verified. US1 complete.

---

## Phase 4: User Story 2 — Assert Inclusive Upper Bound / BeLessThanOrEqualTo (Priority: P1) — also covers User Story 3

**Goal**: Deliver `BeLessThanOrEqualTo` so a developer can write `result.Should().BeLessThanOrEqualTo(maxValue)` and receive a precise failure message when the assertion fails. The optional `IComparer<T>` parameter satisfies User Story 3 (custom comparer injection).

**Independent Test**: Run `dotnet test --filter "BeLessThanOrEqualTo"` after T015 — all six test methods must pass with no impact on the rest of the suite.

### Tests for User Story 2 + 3 (Red — write first, confirm they fail)

- [x] T009 [P] [US2] Add `BeLessThanOrEqualTo_WhenLess_Passes` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `3.Should().BeLessThanOrEqualTo(5)` must pass
- [x] T010 [P] [US2] Add `BeLessThanOrEqualTo_WhenEqual_Passes` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `5.Should().BeLessThanOrEqualTo(5)` must pass (inclusive boundary)
- [x] T011 [P] [US2] Add `BeLessThanOrEqualTo_WhenGreater_Fails` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `6.Should().BeLessThanOrEqualTo(5)` must throw `AssertionFailedException`; assert `Assert.Contains("a value less than or equal to", ex.Expected)` and `Assert.Equal("6", ex.Actual)` to validate the exact FR-008 format
- [x] T012 [P] [US2] Add `BeLessThanOrEqualTo_WithCustomComparer_UsesComparer` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` (covers US3) — pass an inverted `Comparer<int>`; assert `3.Should().BeLessThanOrEqualTo(5, comparer: invertedComparer)` fails (inverted comparer sees 3 as greater than 5)
- [x] T013 [P] [US2] Add `BeLessThanOrEqualTo_WithNullComparer_FallsBackToDefault` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — `3.Should().BeLessThanOrEqualTo(5, comparer: null)` must pass without throwing
- [x] T014 [P] [US2] Add `BeLessThanOrEqualTo_WithBecause_IncludesReasonInMessage` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — assert that `6.Should().BeLessThanOrEqualTo(5, because: "the result must not exceed the limit")` throws and `ex.Message` contains `"the result must not exceed the limit"`

### Implementation for User Story 2 + 3 (Green)

- [x] T015 [US2] Add `BeLessThanOrEqualTo` method to `NumericAssertions<T>` in `src/Assertivo/Numeric/NumericAssertions.cs`: decorate with `[StackTraceHidden]`, null-coalesce comparer to `Comparer<T>.Default`, fail with `MessageFormatter.Fail($"a value less than or equal to {MessageFormatter.FormatValue(value)}", ...)` when `comparer.Compare(Subject, value) > 0`, return `new AndConstraint<NumericAssertions<T>>(this)`; add XML `<summary>` doc; run `dotnet test --filter "BeLessThanOrEqualTo"` to confirm T009–T014 all pass

**Checkpoint**: `BeLessThanOrEqualTo` fully functional and independently verified. US2 + US3 complete.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate chaining, full suite green, coverage thresholds met, zero build warnings.

- [x] T016 [P] Add two chaining tests to `tests/Assertivo.Tests/NumericAssertionsTests.cs`: (1) `BeGreaterThan_ReturnsAndConstraint_AllowingChaining` — `42.Should().BeGreaterThan(0).And.BeLessThan(100)` must compile and pass; (2) `BeLessThanOrEqualTo_ReturnsAndConstraint_AllowingChaining` — `42.Should().BeLessThanOrEqualTo(100).And.BeGreaterThan(0)` must compile and pass (validates FR-011 and SC-004 for both new methods independently)
- [x] T019 [P] Add `BeGreaterThan_FailureMessage_ContainsCallerExpression` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — declare `int result = 2;`, call `result.Should().BeGreaterThan(3)`, catch the exception, and assert `ex.Message` contains `"result"` (validates SC-002 / constitution §3.4 — caller expression captured via CallerArgumentExpression)
- [x] T020 [P] Add `BeLessThanOrEqualTo_FailureMessage_ContainsCallerExpression` test to `tests/Assertivo.Tests/NumericAssertionsTests.cs` — declare `int result = 6;`, call `result.Should().BeLessThanOrEqualTo(5)`, catch the exception, and assert `ex.Message` contains `"result"` (same SC-002 / §3.4 requirement for the second new method)
- [x] T021 [P] Add `BeGreaterThan_HappyPath_ZeroAllocation` and `BeLessThanOrEqualTo_HappyPath_ZeroAllocation` benchmark methods to `ShouldBeBenchmarks` in `tests/Assertivo.Benchmarks/Program.cs` — use `_value.Should().BeGreaterThan(0)` and `_value.Should().BeLessThanOrEqualTo(100)` patterns; run `dotnet run --project tests/Assertivo.Benchmarks/Assertivo.Benchmarks.csproj -c Release` and confirm `Allocated` column shows `0 B` for both (resolves constitution §6.1 MUST — comparison benchmarks, and verifies §6.2 zero-allocation)
- [x] T017 Run full test suite with coverage thresholds: `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj /p:CollectCoverage=true /p:Threshold=93,90 /p:ThresholdType=line,branch` — all existing and new tests must pass; coverage must meet 93% line / 90% branch
- [x] T018 [P] Run `dotnet build src/Assertivo/Assertivo.csproj` and confirm zero warnings — absence of warnings proves XML `<summary>` docs are present on both new public methods (`TreatWarningsAsErrors=true` will catch missing docs)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (US1 — BeGreaterThan)**: Depends on T001 only
- **Phase 3 (US2/US3 — BeLessThanOrEqualTo)**: Independent of Phase 2; can begin after T001 in parallel if desired
- **Phase 4 (Polish)**: Depends on T008 and T015 both complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after T001; no dependency on US2
- **User Story 2 (P1)**: Can start after T001; no dependency on US1
- **User Story 3 (P2)**: Subsumed by US2 — the `comparer` parameter is inherent to `BeLessThanOrEqualTo`; T012 covers the US3 acceptance scenario

### Within Each User Story

1. Write all tests (T002–T007 or T009–T014) first — confirm they fail (compile error until method exists)
2. Implement the method (T008 or T015) — all tests for that story must go green
3. Move to next story or Polish phase

### Parallel Opportunities

- T002–T007 (US1 tests): All six can be written together before T008
- T009–T014 (US2 tests): All six can be written together before T015
- T002–T007 and T009–T014 can be written in a single pass if desired (both method stubs absent → both sets fail)
- T016, T019, T020, T021, T018 (polish tasks): All are independent of each other and can run in parallel after T015

---

## Parallel Example: User Story 1 Tests

```
# Write all six US1 tests in one pass (all will fail until T008):
T002 — BeGreaterThan_WhenGreater_Passes
T003 — BeGreaterThan_WhenEqual_Fails
T004 — BeGreaterThan_WhenLess_Fails
T005 — BeGreaterThan_WithCustomComparer_UsesComparer
T006 — BeGreaterThan_WithNullComparer_FallsBackToDefault
T007 — BeGreaterThan_WithBecause_IncludesReasonInMessage

# Then implement in T008 to turn them all green.
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 (Setup)
2. Write T002–T007 (US1 tests — Red)
3. Implement T008 (US1 — Green)
4. **STOP and VALIDATE**: `dotnet test --filter "BeGreaterThan"` — all pass
5. `BeGreaterThan` is shippable as an isolated increment if needed

### Full Delivery (Both Stories)

1. T001 → T002–T007 → T008 (US1 complete)
2. T009–T014 → T015 (US2+US3 complete)
3. T016, T019, T020, T021 in parallel → T017 → T018 (Polish)
4. All four comparison methods available; coverage, benchmarks, and build clean

---

## Notes

- [P] tasks involve logically independent edits; both can target the same file since no edit conflicts
- Test method names follow the project convention: `MethodName_Scenario_ExpectedOutcome`
- The spec's edge case "equal to threshold" is critical: T003 verifies BeGreaterThan *fails* on equal; T010 verifies BeLessThanOrEqualTo *passes* on equal — do not swap these
- `comparer.Compare(Subject, value) <= 0` → fail for BeGreaterThan; `> 0` → fail for BeLessThanOrEqualTo
- Coverlet always-throw caveat: `MessageFormatter.Fail` call sites may not register as covered; this is the documented project limitation (see test project csproj comment); 93% threshold accounts for it
