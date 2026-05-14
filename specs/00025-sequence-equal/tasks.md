# Tasks: Ordered Sequence Equality Assertion

**Input**: Design documents from `/specs/00025-sequence-equal/`  
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · contracts/ ✅ · quickstart.md ✅

**Scope summary**:
- `src/Assertivo/Collections/GenericCollectionAssertions.cs` — add `partial` keyword to existing struct declaration
- `src/Assertivo/Collections/GenericCollectionAssertions.Equal.cs` — new file: both `Equal` overloads (partial struct continuation)
- `tests/Assertivo.Tests/EqualAssertionsTests.cs` — new file: full test coverage for all user stories and edge cases

---

## Phase 1: Setup

**Purpose**: Structural preparation required before any implementation task can compile.

- [x] T001 Add `partial` keyword to `GenericCollectionAssertions<T>` struct declaration in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T002 [P] Create `src/Assertivo/Collections/GenericCollectionAssertions.Equal.cs` — partial struct file with `using` directives (`System.Collections.Generic`, `System.Diagnostics`, `Assertivo.Primitives`) and stub signatures for `Equal(IEnumerable<T>, ...)` and `Equal(params T[])` that throw `NotImplementedException` _(can be authored in parallel with T003 but requires T001 complete before the project compiles)_
- [x] T003 [P] Create `tests/Assertivo.Tests/EqualAssertionsTests.cs` — empty test class scaffold with namespace `Assertivo` and `using Assertivo.Collections;`

**Checkpoint**: Project compiles; test file exists; stub methods are reachable from tests.

---

## Phase 2: User Story 1 — Assert Two Ordered Sequences Are Equal (Priority: P1) 🎯 MVP

**Goal**: `Equal` passes when sequences match in order and fails correctly for null subject and ordering mismatches. Dispatch works across all supported collection types.

**Independent Test**: Call `Equal` via `IEnumerable<T>`, `List<T>`, and array subjects. Verify same-order pass, different-order fail, both-empty pass, null-subject fail, and `.And.` chaining on passing result — all without asserting message content.

### Tests for User Story 1 (write first — must be red before T005)

- [x] T004 [P] [US1] Write US1 tests: `Equal_SameElementsSameOrder_Passes`, `Equal_SameElementsDifferentOrder_Fails`, `Equal_BothEmpty_Passes`, `Equal_NullSubject_Fails`, `Equal_ReturnsAndConstraint_ForChaining`, and dispatch verification across `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `List<T>`, and array subjects (FR-012) in `tests/Assertivo.Tests/EqualAssertionsTests.cs`

### Implementation for User Story 1

- [x] T005 [US1] Implement `Equal(IEnumerable<T> expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` in `src/Assertivo/Collections/GenericCollectionAssertions.Equal.cs`: `ArgumentNullException.ThrowIfNull(expected)`, `GuardNull()`, `comparer ??= EqualityComparer<T>.Default`, materialize both with `.ToList()`, count-mismatch path using `MessageFormatter.Fail("collection with {n} element(s)", "{m} element(s)", ...)`, element-loop with first-mismatch path using `MessageFormatter.Fail("{FormatValue(expected[i])} at index {i}", "{FormatValue(actual[i])}", ...)`, return `new AndConstraint<GenericCollectionAssertions<T>>(this)`

**Checkpoint**: US1 tests pass; `Equal` is functional for the primary overload.

---

## Phase 3: User Story 2 — Diagnose Ordered Equality Failures Precisely (Priority: P1)

**Goal**: Failure messages match the exact templates in `contracts/public-api.md` for both count-mismatch and element-mismatch cases; `because` is incorporated; only the first differing index is reported; null elements render as `<null>`.

**Independent Test**: Trigger count-mismatch and element-mismatch failures; assert exact `AssertionFailedException` message content for each scenario. No new implementation — T005 handles it; tests drive any corrections.

### Tests for User Story 2

- [x] T006 [P] [US2] Write US2 diagnostic tests: `Equal_DifferentCounts_MessageStatesExpectedAndActualCounts`, `Equal_ElementMismatch_MessageStatesIndexAndBothValues`, `Equal_ElementMismatch_ReportsOnlyFirstDifferingIndex`, `Equal_ElementMismatch_AtLastIndex_ReportsLastIndex`, `Equal_NullElementInExpected_RenderedAsNullLiteral`, `Equal_NullElementInActual_RenderedAsNullLiteral`, `Equal_WithBecause_IncludesBecauseInMessage` in `tests/Assertivo.Tests/EqualAssertionsTests.cs`

**Checkpoint**: All US2 diagnostic tests pass; message format is verified end-to-end.

---

## Phase 4: User Story 3 — Assert Against Inline Expected Values (Priority: P2)

**Goal**: `Equal(params T[])` convenience overload is available and produces identical behavior to the `IEnumerable<T>` overload for the same inputs.

**Independent Test**: Call `Equal` with inline values and compare output to an equivalent `IEnumerable<T>` invocation; verify pass and fail cases are identical.

### Tests for User Story 3 (write first — must be red before T008)

- [x] T007 [P] [US3] Write US3 test: `Equal_ParamsOverload_BehavesIdenticallyToEnumerableOverload` — assert both pass case and fail case produce the same outcome via params vs `IEnumerable<T>` in `tests/Assertivo.Tests/EqualAssertionsTests.cs`

### Implementation for User Story 3

- [x] T008 [US3] Implement `Equal(params T[] expected)` in `src/Assertivo/Collections/GenericCollectionAssertions.Equal.cs`: single-line delegation `return Equal((IEnumerable<T>)expected);` with `[StackTraceHidden]` attribute and full XML doc comment per `contracts/public-api.md`

**Checkpoint**: US3 test passes; params overload is available and delegates correctly.

---

## Phase 5: User Story 4 — Compare Elements with a Custom Equality Rule (Priority: P2)

**Goal**: Element comparison delegates to the provided `IEqualityComparer<T>`; sequences that differ only by default equality but match under the custom comparer pass; sequences that match by default equality but differ under the custom comparer fail.

**Independent Test**: Use `StringComparer.OrdinalIgnoreCase` as a concrete comparer; verify pass and fail cases. No new implementation — comparer support is already in T005.

### Tests for User Story 4

- [x] T009 [P] [US4] Write US4 tests: `Equal_CustomComparer_TreatsElementsAsEqualWhenComparerSaysSo_Passes` and `Equal_CustomComparer_TreatsElementsAsUnequalWhenComparerSaysSo_Fails` using `StringComparer.OrdinalIgnoreCase` in `tests/Assertivo.Tests/EqualAssertionsTests.cs`

**Checkpoint**: All US4 tests pass; custom comparer path is fully verified.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Edge-case coverage, build health, and coverage validation.

- [x] T010 [P] Write edge-case tests: `Equal_NullExpectedArgument_ThrowsArgumentNullException`, `Equal_NullComparerTreatedAsDefault_DoesNotThrow`, `Equal_BecauseWithFormatArgs_SubstitutesCorrectly`, `Equal_NonEmptySubjectVsEmptyExpected_CountMismatchFail`, `Equal_EmptySubjectVsNonEmptyExpected_CountMismatchFail` in `tests/Assertivo.Tests/EqualAssertionsTests.cs`
- [x] T011 Build solution and run full test suite: `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj`; confirm all new tests pass and zero pre-existing tests regress
- [x] T012 Verify coverage meets constitution §4.1 minimums (95% line, 90% branch) using existing coverage tooling; confirm `EqualAssertionsTests.cs` exercises both `Equal` overloads across positive, negative, and edge-case paths
- [x] T013 Add `Equal` benchmark to `tests/Assertivo.Benchmarks/Program.cs` covering a 1,000-element passing case; verify throughput meets constitution §6.3 (collection assertions ≥ 100,000 ops/sec)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (US1)**: Depends on Phase 1 — T004 can be written once T002+T003 exist; T005 depends on T004 being red
- **Phase 3 (US2)**: Depends on T005 (tests the implemented method; no new implementation)
- **Phase 4 (US3)**: Depends on T005 (params overload delegates to it); T007 can be written after T002+T003; T008 depends on T007 being red
- **Phase 5 (US4)**: Depends on T005; T009 can be written in parallel with T006 and T007 after T005 is complete
- **Final Phase**: T010 depends on T005; T011 depends on all implementation tasks; T012 depends on T011

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 1 — no dependency on other stories
- **US2 (P1)**: Depends on US1 implementation (T005) — tests the same method
- **US3 (P2)**: Depends on US1 implementation (T005) — params delegates to primary; test writing can start after Phase 1
- **US4 (P2)**: Depends on US1 implementation (T005) — tests the comparer path already present

### Parallel Opportunities

Within Phase 1: T002 and T003 can be created in parallel (separate files)  
After Phase 1: T004 and T007 can be written in parallel (both test-writing, separate methods in the same file)  
After T005: T006, T009, and T010 can all be written in parallel (separate test methods, no inter-dependencies)  

---

## Parallel Execution: Phase 1

```
T001 (serial) → T002 [P] ─┐
                            ├─ → T004 → T005 (US1 impl) → T006, T007, T009, T010
               T003 [P] ─┘
```

## Parallel Execution: After T005

```
T005 complete
     ├─ T006 [P] (US2 diagnostic tests)
     ├─ T007 [P] (US3 params test) → T008 (params impl)
     ├─ T009 [P] (US4 comparer tests)
     └─ T010 [P] (edge-case tests)
          ↓ all complete
        T011 (build + test run) → T012 (coverage check)
```

---

## Implementation Strategy

**MVP scope**: Phase 1 + Phase 2 (US1) — delivers a fully functional `Equal(IEnumerable<T>)` assertion with pass/fail behavior and `GuardNull()`. This is independently demonstrable.

**Increment 2**: Phase 3 (US2) — validates message diagnostics; no code change expected if T005 is implemented correctly.

**Increment 3**: Phase 4 (US3) + Phase 5 (US4) — convenience overload and comparer path tests; minimal implementation work.

**Increment 4**: Final Phase — edge cases, coverage gate, clean build.

Total: **13 tasks** | US1: 2 | US2: 1 | US3: 2 | US4: 1 | Setup/Polish: 7  
Parallel opportunities: **7 tasks** marked [P]  
New source files: 2 (`GenericCollectionAssertions.Equal.cs`, `EqualAssertionsTests.cs`)  
Modified source files: 1 (`GenericCollectionAssertions.cs` — `partial` keyword only)
