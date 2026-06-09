# Tasks: NotBeSameAs — Object Reference Inequality Assertion

**Input**: Design documents from `specs/00031-not-be-same-as/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/public-api.md ✅

**Tests**: Included — the spec's acceptance criteria prescribe 6 named test methods (SC-001).

**Organization**: Tasks are grouped by user story to enable independent testing of each scenario class.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files or no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)

---

## Phase 1: Setup & Baseline Verification

**Purpose**: Confirm the project builds cleanly and all existing tests pass before any changes are made.

- [x] T001 Confirm solution builds with no warnings: `dotnet build src/Assertivo/Assertivo.csproj -warnaserror`
- [x] T002 Run existing test suite and confirm all tests pass before modification: `dotnet test tests/Assertivo.Tests/`

**Checkpoint**: Baseline green — all pre-existing tests pass and build is clean. Safe to begin.

---

## Phase 2: User Story 1 — Core Reference Assertion (Priority: P1) 🎯 MVP

**Goal**: Add `NotBeSameAs` to `ObjectAssertions<T>` and verify the primary distinct-reference (pass) and same-reference (fail) paths.

**Independent Test**: Run `dotnet test --filter "NotBeSameAs_WithDifferentReferences|NotBeSameAs_WithSameReference"` — both must pass.

### Tests for User Story 1

> Write these first and confirm they FAIL before adding the implementation.

- [x] T003 [P] [US1] Add `NotBeSameAs_WithDifferentReferences_Passes` to `tests/Assertivo.Tests/ObjectAssertionsTests.cs` — asserts no exception when two distinct `new object()` instances are compared, and chains `.And.NotBeNull()` to confirm the returned `AndConstraint` is composable (covers SC-004)
- [x] T004 [P] [US1] Add `NotBeSameAs_WithSameReference_Fails` to `tests/Assertivo.Tests/ObjectAssertionsTests.cs` — asserts `AssertionFailedException` is thrown with `Expected == "not the same reference"` and `Actual == "same reference"` when the same instance is used for both arguments

### Implementation for User Story 1

- [x] T005 [US1] Add `NotBeSameAs(object? unexpected, string because = "", params object[] becauseArgs)` to `src/Assertivo/ObjectAssertions.cs` — include `[StackTraceHidden]`, value-type guard (`typeof(T).IsValueType`), `ReferenceEquals` comparison, `MessageFormatter.Fail("not the same reference", "same reference", ...)` on failure, and return `new AndConstraint<ObjectAssertions<T>>(this)` on pass; add full XML doc (`<summary>`, `<param>`, `<returns>`, `<exception>`)

**Checkpoint**: US1 complete — `NotBeSameAs_WithDifferentReferences_Passes` and `NotBeSameAs_WithSameReference_Fails` both pass. Method exists and the primary use case works.

---

## Phase 3: User Story 2 — Null Reference Semantics (Priority: P2)

**Goal**: Verify correct null semantics — null/null fails (same reference), null/non-null passes (different references).

**Independent Test**: Run `dotnet test --filter "NotBeSameAs_WithNull"` — both null tests must pass.

### Tests for User Story 2

- [x] T006 [P] [US2] Add `NotBeSameAs_WithNullSubjectAndNullUnexpected_Fails` to `tests/Assertivo.Tests/ObjectAssertionsTests.cs` — calls `((object?)null).Should<object?>().NotBeSameAs(null)` and asserts `AssertionFailedException` is thrown (both nulls are the same reference per `ReferenceEquals(null, null) == true`)
- [x] T007 [P] [US2] Add `NotBeSameAs_WithNullSubjectAndNonNullUnexpected_Passes` to `tests/Assertivo.Tests/ObjectAssertionsTests.cs` — calls `((object?)null).Should<object?>().NotBeSameAs(new object())` and asserts no exception is thrown

**Checkpoint**: US2 complete — both null-semantics tests pass. No implementation changes required (ReferenceEquals handles null correctly by design).

---

## Phase 4: User Story 4 — Failure Message with Reason (Priority: P2)

**Goal**: Verify that a non-empty `because` string is formatted and appears in both the `Reason` property and the full `Message` of the thrown exception.

**Independent Test**: Run `dotnet test --filter "NotBeSameAs_WithBecauseReason"` — must pass.

### Test for User Story 4

- [x] T008 [US4] Add `NotBeSameAs_WithBecauseReason_IncludesReasonInMessage` to `tests/Assertivo.Tests/ObjectAssertionsTests.cs` — triggers a failure with `because: "the factory must return a new instance"` and asserts both `ex.Reason == "the factory must return a new instance"` and `ex.Message.Contains("the factory must return a new instance")`

**Checkpoint**: US4 complete — because-formatting test passes. No implementation changes required (because/becauseArgs plumbing is part of T005).

---

## Phase 5: User Story 3 — Value Type Guard (Priority: P3)

**Goal**: Verify that calling `NotBeSameAs` on a value-type subject throws `InvalidOperationException` with the prescribed message before any comparison is attempted.

**Independent Test**: Run `dotnet test --filter "NotBeSameAs_WithValueType"` — must pass.

### Test for User Story 3

- [x] T009 [US3] Add `NotBeSameAs_WithValueType_ThrowsInvalidOperationException` to `tests/Assertivo.Tests/ObjectAssertionsTests.cs` — calls `42.Should<int>().NotBeSameAs(42)` and asserts `InvalidOperationException` with message `"NotBeSameAs is not meaningful for value type 'Int32'. Use Be() for value equality."`

**Checkpoint**: US3 complete — value-type guard test passes. All 6 acceptance scenario tests are now written and green.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification — coverage thresholds, public API contract consistency, and end-to-end quickstart validation.

- [x] T010 Run the full test suite and confirm all 6 new tests plus all pre-existing tests pass: `dotnet test tests/Assertivo.Tests/`
- [x] T011 [P] Confirm coverage thresholds are met (line ≥93%, branch ≥90%): `dotnet test tests/Assertivo.Tests/ --collect:"XPlat Code Coverage"`
- [x] T012 [P] Verify `NotBeSameAs` signature is present in `specs/00001-assertion-library-core/contracts/public-api.md` under `ObjectAssertions<T>` immediately after `BeSameAs`
- [x] T013 Run all 7 validation scenarios in `specs/00031-not-be-same-as/quickstart.md` and confirm expected outcomes (no exceptions on pass paths, correct exceptions on fail paths, correct fluent chaining)
- [x] T014 [P] Locate the existing BenchmarkDotNet benchmark file for `BeSameAs` (likely under `benchmarks/` or `tests/`) and add a parallel `NotBeSameAs` benchmark entry verifying zero-allocation on the passing path at ≥10M ops/sec (constitution §VI.1, §VI.2, §VI.3)
- [x] T015 [P] Update `CHANGELOG.md` — add a bullet under `[Unreleased] > Added` documenting `ObjectAssertions<T>.NotBeSameAs(object? unexpected, string because = "", params object[] becauseArgs)` (constitution §VIII)

**Checkpoint**: All 6 tests pass, coverage thresholds hold, public API contract is consistent, and quickstart validation is complete. Feature is shippable.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (US1)**: Depends on Phase 1 — T005 (implementation) depends on T003 and T004 (tests) failing first
- **Phases 3–5 (US2, US4, US3)**: All depend on Phase 2 (T005) being complete; can then proceed in any order or in parallel
- **Phase 6 (Polish)**: Depends on all test tasks (T003–T009) being complete and passing

### User Story Dependencies

| Story | Priority | Depends on | Can run in parallel with |
|-------|----------|-----------|--------------------------|
| US1 | P1 (MVP) | Phase 1 | — |
| US2 | P2 | US1 (T005) | US4 |
| US4 | P2 | US1 (T005) | US2 |
| US3 | P3 | US1 (T005) | US2, US4 |

### Within Phase 2 (US1)

1. T003 and T004 (tests) — write in parallel, confirm they FAIL
2. T005 (implementation) — only after T003/T004 fail as expected
3. Confirm T003 and T004 now pass

### Parallel Opportunities

- T003 and T004 (US1 test stubs) — different test method names in same file, write together
- T006 and T007 (US2 tests) — parallel writes
- T011 and T012 (Phase 6) — coverage run and contract check are independent

---

## Parallel Example: Phases 3–5 (after US1 complete)

Once T005 is merged, all three remaining story phases can be written together:

```text
Write in parallel:
  T006 — NotBeSameAs_WithNullSubjectAndNullUnexpected_Fails
  T007 — NotBeSameAs_WithNullSubjectAndNonNullUnexpected_Passes
  T008 — NotBeSameAs_WithBecauseReason_IncludesReasonInMessage
  T009 — NotBeSameAs_WithValueType_ThrowsInvalidOperationException
```

All four test methods write to the same file (`ObjectAssertionsTests.cs`) but to non-overlapping regions. A single `dotnet test` after all four confirms all pass simultaneously.

---

## Implementation Strategy

### MVP First (Phase 2 Only)

1. Complete Phase 1: Baseline verification
2. Write T003/T004 tests → confirm they fail
3. Complete T005: Add `NotBeSameAs` to `ObjectAssertions.cs`
4. **STOP and VALIDATE**: Confirm T003/T004 pass — core assertion works
5. Ship if null/guard/because are already handled by the implementation (they will be)

### Incremental Delivery

1. Phase 1 → baseline green
2. Phase 2 → method exists, primary paths tested (MVP)
3. Phases 3–5 → null semantics, because-formatting, guard all explicitly tested
4. Phase 6 → coverage confirmed, contract verified, quickstart validated

---

## Notes

- [P] tasks = independent writes with no cross-dependency on incomplete work
- [Story] label maps each test to its acceptance scenario in the spec
- T005 is the only source file change — all other tasks are test additions or verification steps
- `BeSameAs` in `src/Assertivo/ObjectAssertions.cs` is the direct implementation template; mirror its structure with inverted boolean logic
- The value-type guard message must match exactly: `"NotBeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality."`
- Commit after T005 (implementation) and again after all 6 tests pass
