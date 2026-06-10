---
description: "Task list for Fix BeEmpty Double Enumeration"
---

# Tasks: Fix BeEmpty Double Enumeration

**Input**: Design documents from `/specs/00045-fix-be-empty-double-enumeration/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, contracts/public-api.md ✓, quickstart.md ✓

**Scope**: Single method change in one file + two new tests + one XML doc update. No foundational infrastructure required.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to ([US1] or [US2])

---

## Phase 1: Setup — Confirm Baseline

**Purpose**: Verify the existing test suite is green before making any changes.

- [x] T001 Run existing BeEmpty tests and confirm all pass as baseline: `dotnet test tests/Assertivo.Tests --filter "BeEmpty"` in `tests/Assertivo.Tests/CollectionAssertionsTests.cs`

**Checkpoint**: All existing BeEmpty tests green. Safe to proceed with changes.

---

## Phase 2: User Story 1 — Correct Failure Message on Non-Replayable Sequence (Priority: P1) 🎯 MVP

**Goal**: `BeEmpty` enumerates the subject exactly once; failure messages on non-replayable sequences report the correct count.

**Independent Test**: Add `ThrowOnSecondEnumerationSequence<T>` helper and run `BeEmpty_WithNonReplayableSequence_FailsWithCorrectCount` — it must pass without any `InvalidOperationException`.

### Tests for User Story 1

> **Write these tests FIRST and confirm they FAIL before applying the implementation fix.**

- [x] T002 [US1] Add private sealed `ThrowOnSecondEnumerationSequence<T>` helper class that throws `InvalidOperationException` on a second `GetEnumerator()` call in `tests/Assertivo.Tests/CollectionAssertionsTests.cs`
- [x] T003 [P] [US1] Add test `BeEmpty_WithNonReplayableSequence_FailsWithCorrectCount` asserting `Expected = "an empty collection"` and `Actual = "a collection with 3 item(s)"` in `tests/Assertivo.Tests/CollectionAssertionsTests.cs`
- [x] T004 [P] [US1] Add test `BeEmpty_WithNonReplayableEmptySequence_Passes` asserting no exception is thrown in `tests/Assertivo.Tests/CollectionAssertionsTests.cs`

### Implementation for User Story 1

- [x] T005 [US1] In `BeEmpty` method replace `if (Subject!.Any())` + `Subject!.Count()` with `var count = Subject!.Count(); if (count > 0)` and remove the now-redundant second `Subject!` null-forgiving operator from the failure message expression in `src/Assertivo/Collections/GenericCollectionAssertions.cs` (lines 170–181; edit targets lines 173–179)

**Checkpoint**: T003 and T004 now pass. T001 baseline tests still green.

---

## Phase 3: User Story 2 — Existing Behavior Preserved for Replayable Collections (Priority: P2)

**Goal**: All pre-existing `BeEmpty` tests continue to pass without modification after the fix.

**Independent Test**: No standalone task required — regression-free behaviour is validated by T007 (full test suite run in Phase 4), which includes all pre-existing `BeEmpty` tests. See Dependencies section.

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: XML documentation update (constitution §3.2), final build validation, and contract verification.

- [x] T006 [P] Append ` Enumerates the subject exactly once; safe for non-replayable sequences.` to the existing `<summary>` XML doc on `BeEmpty` in `src/Assertivo/Collections/GenericCollectionAssertions.cs`
- [x] T007 Run full test suite confirming: (a) zero regressions on existing BeEmpty tests, (b) new T003/T004 tests pass, and (c) clean build with zero warnings (`TreatWarningsAsErrors`): `dotnet test tests/Assertivo.Tests` from repo root
- [x] T008 [P] Verify `contracts/public-api.md` enumeration guarantee section matches the implemented behaviour in `specs/00045-fix-be-empty-double-enumeration/contracts/public-api.md`
- [x] T009 [P] Confirm `BeEmpty` happy-path allocation is zero: inspect via `dotnet-counters` or BenchmarkDotNet allocation run, or verify the compiled IL introduces no boxing or closure allocation for the `var count = Subject!.Count()` replacement; document the result in a new `## Allocation Verification` section in `specs/00045-fix-be-empty-double-enumeration/research.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **User Story 1 (Phase 2)**: Depends on Phase 1 (baseline confirmation)
- **User Story 2 (Phase 3)**: No standalone tasks; validated by T007 in Phase 4 after T005 is applied
- **Polish (Phase 4)**: Depends on all Phase 2 tasks complete (T005 applied, T003/T004 passing)

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 1. No dependency on US2.
- **User Story 2 (P2)**: Validated by T007 — confirmed regression-free once T005 is applied.

### Within User Story 1

1. T002 first (helper class — T003 and T004 depend on it)
2. T003 and T004 in parallel after T002 (both use the helper, target different scenarios)
3. Confirm T003 and T004 FAIL (red phase)
4. T005 (implementation fix)
5. Confirm T003 and T004 now PASS (green phase)

### Parallel Opportunities

- T003 and T004 can be written in parallel after T002
- T006, T008, and T009 (polish tasks) can all run in parallel after T005
- T007 is the final gate and depends on T006 completing first (XML doc must be present for zero-warning build)

---

## Parallel Example: User Story 1

```
# After T002 (helper class) is written:

Task T003: Add BeEmpty_WithNonReplayableSequence_FailsWithCorrectCount test
Task T004: Add BeEmpty_WithNonReplayableEmptySequence_Passes test
  → Both reference ThrowOnSecondEnumerationSequence<T> from T002
  → Can be written in parallel; both will FAIL until T005 is applied

# After T005 (fix applied), Phase 4 tasks run in parallel:
Task T006: Update XML summary on BeEmpty
Task T008: Verify contracts/public-api.md
Task T009: Confirm happy-path allocation is zero

# Final gate (after T006 completes):
Task T007: Run full test suite — all tests including T003/T004 green, zero warnings
```

---

## Implementation Strategy

### MVP (User Story 1 Only)

1. ✅ Phase 1: Confirm baseline green
2. ✅ Phase 2: Add helper + 2 tests (red) → apply fix (green)
3. **STOP and VALIDATE**: `dotnet test --filter BeEmpty` — 2 new tests + all existing tests pass
4. Done — US2 (regression guard) is inherently satisfied by the same test run

### Full Delivery

1. Complete Phase 1 → 2 → 4 sequentially (Phase 3 has no standalone tasks)
2. Total wall-clock time: minutes; total tasks: 9

---

## Notes

- [P] tasks can run in parallel — they target different files or independent test methods
- T002 must complete before T003/T004 (helper dependency)
- T006 must complete before T007 (XML doc required for zero-warning build)
- Constitution §3.2: T006 is non-optional — all public members must have XML docs
- Constitution §VI.2: T009 validates the allocation budget; acceptable to scope as IL inspection if BenchmarkDotNet suite does not yet exist
- US2 (Phase 3) has no standalone task — its acceptance criterion (no regressions) is validated by T007
