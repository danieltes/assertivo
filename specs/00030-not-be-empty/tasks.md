# Tasks: NotBeEmpty Assertion

**Input**: Design documents from `specs/00030-not-be-empty/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | contracts/public-api.md ✅ | quickstart.md ✅
**Last updated**: 2026-06-07 (added T022/T023 for SC-004 Expression coverage; updated T021 for contract cross-check; corrected because-whitespace contract in contracts/public-api.md — see analysis report)

**Tests**: Included — spec Implementation Notes explicitly name `StringAssertionsTests.cs` and `CollectionAssertionsTests.cs`.  
**Approach**: Red → Green — write failing tests first, then implement to make them pass.

**Organization**: Two independent user stories (both P1); each has its own phase and can be completed and verified independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (touches different files, no dependency on incomplete tasks)
- **[Story]**: Which user story this task belongs to ([US1] = string, [US2] = collection)
- Exact file paths included in every task description

---

## Phase 1: Setup (Baseline Verification)

**Purpose**: Confirm the test suite is fully green before any changes are made.

- [x] T001 Run `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj` and confirm all tests pass (zero failures, coverage thresholds met)

**Checkpoint**: Green baseline established — US1 and US2 can now begin independently.

---

## Phase 2: User Story 1 — Assert String Is Not Empty (Priority: P1) 🎯 MVP

**Goal**: Developer can call `value.Should().NotBeEmpty()` on a string and get the correct pass/fail behavior per the acceptance scenarios.

**Independent test**: `dotnet test --filter "Category=StringAssertionsTests" tests/Assertivo.Tests/` (or filter by class name) — all six `NotBeEmpty` tests pass; no pre-existing tests break.

### Tests for User Story 1 (Red Phase — write BEFORE implementation)

> **Write these tests first and confirm they FAIL with `CS0117` or `MethodNotFoundException` before proceeding to T008.**

- [x] T002 [US1] Add test `NotBeEmpty_WithNonEmptyString_Passes` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: call `"hello".Should().NotBeEmpty()` and expect no exception
- [x] T003 [US1] Add test `NotBeEmpty_WithEmptyString_Fails` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: assert `Assert.Throws<AssertionFailedException>(() => "".Should().NotBeEmpty())` then verify `ex.Expected == "a non-empty string"` and `ex.Actual == "\"\""`
- [x] T004 [US1] Add test `NotBeEmpty_WithBecauseReason_IncludesReasonInMessage` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: assert failure on `""` with `because: "response body must not be blank"` and verify `ex.Message` contains the reason text
- [x] T005 [US1] Add test `NotBeEmpty_WithNullSubject_Passes` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: assign `string? value = null` and call `value.Should().NotBeEmpty()` — expect no exception
- [x] T006 [US1] Add test `NotBeEmpty_WithWhitespaceString_Passes` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: call `"   ".Should().NotBeEmpty()` — expect no exception (whitespace is not `""`)
- [x] T007 [US1] Add test `NotBeEmpty_ReturnsAndConstraint_AllowingChaining` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: call `"hello".Should().NotBeEmpty().And.Be("hello")` — expect no exception
- [x] T022 [US1] Add test `NotBeEmpty_WithEmptyString_IncludesExpressionInMessage` to `tests/Assertivo.Tests/StringAssertionsTests.cs`: declare `string value = ""`, assert `Assert.Throws<AssertionFailedException>(() => value.Should().NotBeEmpty())`, then verify `Assert.NotNull(ex.Expression)` and `Assert.Contains("Expression:", ex.Message)` — validates SC-004 (subject expression element)

### Implementation for User Story 1 (Green Phase)

- [x] T008 [US1] Add `NotBeEmpty` method after `BeEmpty` in `src/Assertivo/StringAssertions.cs`: decorate with `[StackTraceHidden]`, add XML doc `<summary>Asserts that the subject is not an empty string.</summary>` with `<remarks>` noting null passes, signature `public AndConstraint<StringAssertions> NotBeEmpty(string because = "", params object[] becauseArgs)`, logic: fail with `MessageFormatter.Fail("a non-empty string", MessageFormatter.FormatValue(Subject), Expression, because, becauseArgs)` when `string.Equals(Subject, string.Empty, StringComparison.Ordinal)` is true, return `new AndConstraint<StringAssertions>(this)`
- [x] T009 [US1] Run `dotnet test --filter StringAssertionsTests tests/Assertivo.Tests/Assertivo.Tests.csproj` and confirm all six `NotBeEmpty_*` tests pass and all pre-existing `StringAssertionsTests` tests still pass

**Checkpoint**: User Story 1 complete — `StringAssertions.NotBeEmpty` fully functional and tested independently.

---

## Phase 3: User Story 2 — Assert Collection Has at Least One Element (Priority: P1)

**Goal**: Developer can call `collection.Should().NotBeEmpty()` on any `IEnumerable<T>` and get the correct pass/fail/null-guard behavior per the acceptance scenarios.

**Independent test**: Run `dotnet test --filter CollectionAssertionsTests tests/Assertivo.Tests/` — all six `NotBeEmpty` tests pass; no pre-existing collection tests break.

### Tests for User Story 2 (Red Phase — write BEFORE implementation)

> **Write these tests first and confirm they FAIL before proceeding to T016.**

- [x] T010 [US2] Add test `NotBeEmpty_WithNonEmptyCollection_Passes` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: call `new List<int> { 1 }.Should().NotBeEmpty()` — expect no exception
- [x] T011 [US2] Add test `NotBeEmpty_WithEmptyCollection_Fails` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: assert `Assert.Throws<AssertionFailedException>(() => new List<int>().Should().NotBeEmpty())` then verify `ex.Expected == "a non-empty collection"` and `ex.Actual == "an empty collection"`
- [x] T012 [US2] Add test `NotBeEmpty_WithBecauseReason_IncludesReasonInMessage` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: assert failure on empty list with `because: "pipeline must produce results"` and verify `ex.Message` contains the reason text
- [x] T013 [US2] Add test `NotBeEmpty_WithNullSubject_FailsWithNullGuard` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: assign `IEnumerable<int>? results = null`, assert `Assert.Throws<AssertionFailedException>(() => results.Should().NotBeEmpty())`, verify `ex.Expected == "a collection"` and `ex.Actual == "<null>"`
- [x] T014 [US2] Add test `NotBeEmpty_ReturnsAndConstraint_AllowingChaining` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: call `new[] { 1, 2 }.Should().NotBeEmpty().And.HaveCount(2)` — expect no exception
- [x] T015 [US2] Add test `NotBeEmpty_WithLazySequence_Passes` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: call `Enumerable.Range(1, 5).Select(x => x).Should().NotBeEmpty()` — expect no exception (verifies non-`List<T>` sequences work)
- [x] T023 [US2] Add test `NotBeEmpty_WithEmptyCollection_IncludesExpressionInMessage` to `tests/Assertivo.Tests/CollectionAssertionsTests.cs`: declare `var collection = new List<int>()`, assert `Assert.Throws<AssertionFailedException>(() => collection.Should().NotBeEmpty())`, then verify `Assert.NotNull(ex.Expression)` and `Assert.Contains("Expression:", ex.Message)` — validates SC-004 (subject expression element)

### Implementation for User Story 2 (Green Phase)

- [x] T016 [US2] Create `src/Assertivo/Collections/GenericCollectionAssertions.NotBeEmpty.cs` as a new partial struct file: namespace `Assertivo.Collections`, `using System.Diagnostics;` and `using Assertivo.Primitives;`, declare `public readonly partial struct GenericCollectionAssertions<T>`, add `NotBeEmpty` method decorated with `[StackTraceHidden]`, XML doc `<summary>Asserts that the collection is not empty (contains at least one element).</summary>`, signature `public AndConstraint<GenericCollectionAssertions<T>> NotBeEmpty(string because = "", params object[] becauseArgs)`, logic: call `GuardNull()`, then fail with `MessageFormatter.Fail("a non-empty collection", "an empty collection", Expression, because, becauseArgs)` when `!Subject!.Any()` is true, return `new AndConstraint<GenericCollectionAssertions<T>>(this)`
- [x] T017 [US2] Run `dotnet test --filter CollectionAssertionsTests tests/Assertivo.Tests/Assertivo.Tests.csproj` and confirm all six `NotBeEmpty_*` tests pass and all pre-existing `CollectionAssertionsTests` tests still pass

**Checkpoint**: User Story 2 complete — `GenericCollectionAssertions<T>.NotBeEmpty` fully functional and tested independently.

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Full-suite regression, coverage gate, build quality, and documentation verification.

- [x] T018 Run full test suite `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj` and confirm zero failures — validates SC-001 (no pre-existing tests broken by either new method)
- [x] T019 Run coverage check `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura` and confirm line coverage ≥93% and branch coverage ≥90% — validates SC-003 and constitution §IV.1
- [x] T020 [P] Build in Release mode `dotnet build src/Assertivo/Assertivo.csproj -c Release` and confirm zero warnings — validates `TreatWarningsAsErrors`, nullable reference type correctness, and XML doc completeness on both new methods
- [x] T021 [P] Review `specs/00030-not-be-empty/quickstart.md` scenarios and confirm each scenario is covered by a corresponding passing test in `tests/Assertivo.Tests/StringAssertionsTests.cs` and `tests/Assertivo.Tests/CollectionAssertionsTests.cs`; also cross-check that method signatures in `specs/00030-not-be-empty/contracts/public-api.md` match the final implementations in `src/Assertivo/StringAssertions.cs` and `src/Assertivo/Collections/GenericCollectionAssertions.NotBeEmpty.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — run immediately
- **User Story 1 (Phase 2)**: Depends on Phase 1 green baseline; independent of User Story 2
- **User Story 2 (Phase 3)**: Depends on Phase 1 green baseline; independent of User Story 1
- **Polish (Phase 4)**: Depends on both Phase 2 and Phase 3 being complete

### User Story Dependencies

- **User Story 1 (String)**: No dependency on User Story 2 — touches only `StringAssertions.cs` and `StringAssertionsTests.cs`
- **User Story 2 (Collection)**: No dependency on User Story 1 — touches only `GenericCollectionAssertions.NotBeEmpty.cs` (new file) and `CollectionAssertionsTests.cs`

### Within Each User Story

1. Write ALL failing tests first (T002–T007, T022 for US1; T010–T015, T023 for US2)
2. Confirm tests fail (compilation error or runtime failure) before writing implementation
3. Implement the method (T008 for US1; T016 for US2)
4. Verify all tests pass (T009 for US1; T017 for US2)
5. Move to Polish phase only after both stories are green

---

## Parallel Opportunities

### User Stories Can Run in Parallel

Since US1 and US2 touch entirely different files, two developers (or two agents) can work simultaneously:

```
Developer A (US1 — String):           Developer B (US2 — Collection):
T002 → T003 → T004 → T005             T010 → T011 → T012 → T013
→ T006 → T007 → T022 → T008 → T009    → T014 → T015 → T023 → T016 → T017
```

Once both complete → T018 → T019, T020 [P], T021 [P]

### Polish Phase Parallel Tasks

T020 (Release build) and T021 (quickstart review) can run concurrently — no shared state.

---

## Implementation Strategy

### MVP (User Story 1 Only)

1. T001 — establish green baseline
2. T002–T008 — write tests, implement string `NotBeEmpty`
3. T009 — verify string story is independently green
4. **STOP and VALIDATE**: `value.Should().NotBeEmpty()` works for all string scenarios

### Full Delivery (Both Stories)

1. Complete Phase 1 (T001)
2. Complete Phase 2 — US1 (T002–T009)
3. Complete Phase 3 — US2 (T010–T017) — can overlap with Phase 2 in parallel
4. Complete Phase 4 — Polish (T018–T021)

---

## Notes

- `[P]` tasks touch different files and have no blocking dependencies — safe for parallel execution
- `[US1]` / `[US2]` labels map tasks to spec user stories for traceability
- Tests MUST fail before implementing — confirm with a build or test run first
- Both methods follow the same [StackTraceHidden] + XML doc + AndConstraint<T> pattern as all other assertion methods
- The partial file `GenericCollectionAssertions.NotBeEmpty.cs` must declare `public readonly partial struct GenericCollectionAssertions<T>` — the `partial` keyword is required to compile
- `GuardNull()` is a private method on `GenericCollectionAssertions<T>` defined in `GenericCollectionAssertions.cs` — it is accessible from any partial of the same struct
