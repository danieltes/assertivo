# Tasks: NotBe Inequality Assertion

**Feature**: `00028-not-be-assertion` | **Branch**: `00028-not-be-assertion`  
**Input**: [spec.md](spec.md) · [plan.md](plan.md) · [data-model.md](data-model.md) · [contracts/api.md](contracts/api.md)

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel with other [P] tasks in the same phase (different files, no blocking dependency)
- **[US1/US2/US3]**: User story this task belongs to
- Exact file paths included in all descriptions

---

## Phase 1: Setup

**Purpose**: Confirm the branch builds cleanly before any changes are made.

- [X] T001 Verify `dotnet build Assertivo.slnx` passes with zero warnings on branch `00028-not-be-assertion` as a pre-change baseline

**Checkpoint**: Build is green on unmodified branch — safe to begin implementation.

---

## Phase 2: Foundational

**N/A** — All required infrastructure already exists (`MessageFormatter.Fail`, `AndConstraint<T>`, `AssertionConfiguration.ReportFailure`, `EqualityComparer<T>.Default`). The three target source files and test files already exist. No new projects, files, folders, or dependencies are needed.

---

## Phase 3: User Story 1 — Core NotBe Assertion (Priority: P1) 🎯 MVP

**Goal**: Add `NotBe` to all three assertion types so developers can assert value inequality on any supported subject. Covers basic pass/fail, failure message format, `because`/`becauseArgs`, and `AndConstraint` chaining.

**Independent Test**: Run `dotnet test tests/Assertivo.Tests/` filtering to `NotBe` tests. Verify: `int` and `long` subjects with both equal and unequal unexpected values; `string` subjects; failure messages contain `not {value}` as the Expected field; `because` text appears in the failure message; `AndConstraint.And` returns the same assertion object for chaining.

> **§IV.3 — Red first**: Per the constitution's spec-driven cycle, test tasks (T005–T007) MUST be written and failing before implementation tasks (T002–T004) are written. Task IDs reflect logical grouping, not execution order.

### Implementation

- [X] T002 [P] [US1] Add `NotBe(T unexpected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` to `ObjectAssertions<T>` in `src/Assertivo/ObjectAssertions.cs` — with `[StackTraceHidden]`, XML `<summary>`, `comparer ??= EqualityComparer<T>.Default`, call `MessageFormatter.Fail($"not {MessageFormatter.FormatValue(unexpected)}", MessageFormatter.FormatValue(Subject), Expression, because, becauseArgs)` when `comparer.Equals(Subject, unexpected)`, return `new AndConstraint<ObjectAssertions<T>>(this)`
- [X] T003 [P] [US1] Add `NotBe(T unexpected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` to `NumericAssertions<T>` in `src/Assertivo/Numeric/NumericAssertions.cs` — identical logic to T002 with return type `AndConstraint<NumericAssertions<T>>`
- [X] T004 [P] [US1] Add `NotBe(string? unexpected, string because = "", params object[] becauseArgs)` to `StringAssertions` in `src/Assertivo/StringAssertions.cs` — with `[StackTraceHidden]`, XML `<summary>`, `string.Equals(Subject, unexpected, StringComparison.Ordinal)` for equality check, `MessageFormatter.Fail($"not {MessageFormatter.FormatValue(unexpected)}", ...)` on equal, return `new AndConstraint<StringAssertions>(this)`

### Tests

- [X] T005 [P] [US1] Add `NotBe` core tests to `tests/Assertivo.Tests/ObjectAssertionsTests.cs`: `NotBe_WithDifferentValues_Passes` (int 42 vs 99), `NotBe_WithEqualValues_FailsWithMessage` (verifies `ex.Expected` contains `not 42` and `ex.Actual` contains `42`), `NotBe_WithBecauseReason_IncludesReasonInMessage`, `NotBe_ReturnsAndConstraint_AllowingChaining` (chains `.And.Be(...)`)
- [X] T006 [P] [US1] Add `NotBe` core tests to `tests/Assertivo.Tests/NumericAssertionsTests.cs`: `NotBe_WithDifferentInt_Passes`, `NotBe_WithEqualInt_FailsWithMessage`, `NotBe_WithDifferentLong_Passes`, `NotBe_WithEqualLong_FailsWithMessage`, `NotBe_WithBecauseReason_IncludesReasonInMessage`, `NotBe_ReturnsAndConstraint_AllowingChaining`
- [X] T007 [P] [US1] Add `NotBe` core tests to `tests/Assertivo.Tests/StringAssertionsTests.cs`: `NotBe_WithDifferentStrings_Passes`, `NotBe_WithEqualStrings_FailsWithMessage` (verifies `ex.Expected` equals `not "hello"` — quotes are part of the value because `FormatValue` wraps strings, so the C# assertion is `Assert.Equal("not \"hello\"", ex.Expected)` — and `ex.Actual` equals `"\"hello\""`), `NotBe_WithBecauseReason_IncludesReasonInMessage`, `NotBe_ReturnsAndConstraint_AllowingChaining`

**Checkpoint**: All T002–T007 tests green. `NotBe` is fully functional on all three types and can be shipped as MVP.

---

## Phase 4: User Story 2 — Custom Equality Comparer (Priority: P2)

**Goal**: Verify that `ObjectAssertions<T>.NotBe` and `NumericAssertions<T>.NotBe` correctly delegate equality decisions to a supplied `IEqualityComparer<T>`.

**Note**: No new implementation tasks — the comparer parameter is already part of T002 and T003. This phase adds the test coverage that validates comparer behaviour.

**Independent Test**: Construct a custom `IEqualityComparer<T>` where `Equals` always returns `true` (regardless of values). Pass it to `NotBe` with two distinct values and confirm the assertion fails. Then use a comparer where `Equals` always returns `false` and confirm the assertion passes with identical values.

- [X] T008 [P] [US2] Add comparer tests to `tests/Assertivo.Tests/ObjectAssertionsTests.cs`: `NotBe_WithCustomComparerReportingEqual_Fails` (distinct values, comparer says equal → assertion fails), `NotBe_WithCustomComparerReportingUnequal_Passes` (identical values, comparer says unequal → assertion passes), `NotBe_WithNullComparer_FallsBackToDefault`
- [X] T009 [P] [US2] Add comparer tests to `tests/Assertivo.Tests/NumericAssertionsTests.cs`: same three scenarios as T008 using `int` subjects and a custom `IEqualityComparer<int>`

**Checkpoint**: Custom comparer semantics verified for both `ObjectAssertions<T>` and `NumericAssertions<T>`.

---

## Phase 5: User Story 3 — Null Subject and Unexpected Values (Priority: P2)

**Goal**: Verify that `StringAssertions.NotBe` and `ObjectAssertions<T>.NotBe` (reference-type `T`) handle all null combinations correctly per the spec's null-handling table.

**Note**: No new implementation tasks — null handling falls out naturally from `string.Equals(..., Ordinal)` and `EqualityComparer<T>.Default.Equals`. This phase adds the test coverage that validates all four null × unexpected combinations.

**Independent Test**: Call `NotBe` with a null `string` subject + non-null unexpected (should pass), null + null (should fail), non-null + null (should pass), and non-null + non-null equal (should fail).

- [X] T010 [P] [US3] Add null-handling tests to `tests/Assertivo.Tests/StringAssertionsTests.cs`: `NotBe_WithNullSubjectAndNonNullUnexpected_Passes`, `NotBe_WithNullSubjectAndNullUnexpected_Fails`, `NotBe_WithNonNullSubjectAndNullUnexpected_Passes`
- [X] T011 [P] [US3] Add null-handling tests to `tests/Assertivo.Tests/ObjectAssertionsTests.cs`: `NotBe_WithNullReferenceSubjectAndNullUnexpected_Fails` (using `string?` via `Should<string?>()`)

**Checkpoint**: All null-combination scenarios verified. All three user stories pass independently.

---

## Final Phase: Polish & Cross-Cutting

**Goal**: Ensure the implementation meets all constitution non-functional requirements before the feature is considered complete.

- [X] T012 Build `src/Assertivo/Assertivo.csproj` with `dotnet build src/Assertivo/Assertivo.csproj --no-incremental` and confirm zero compiler warnings — specifically that nullable annotations on `StringAssertions.NotBe(string? unexpected, ...)` are correct and `TreatWarningsAsErrors` is satisfied
- [X] T013 [P] Run `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj` and confirm all new `NotBe` tests pass and zero pre-existing tests regress
- [X] T014 [P] Verify line counts of the three modified source files remain under 300 lines each (constitution III.2): `src/Assertivo/ObjectAssertions.cs`, `src/Assertivo/Numeric/NumericAssertions.cs`, `src/Assertivo/StringAssertions.cs`

> **Performance baseline (I2)**: `NotBe` mirrors `Be`'s allocation profile (zero-alloc happy path, `readonly struct` return). If the ≥ 10M ops/sec target from plan.md requires formal verification, run `dotnet run --project tests/Assertivo.Benchmarks/ -- --filter *NotBe*` after T012 passes.

---

## Dependencies

```
T001 (baseline build)
  └── T002 [P] ─┐
      T003 [P] ─┤ (all 3 implementations complete)
      T004 [P] ─┘
                  └── T005 [P] ─┐
                      T006 [P] ─┤ (core tests in all 3 files)
                      T007 [P] ─┘
                                  ├── T008 [P] ─┐ (US2 comparer tests)
                                  │   T009 [P] ─┘
                                  └── T010 [P] ─┐ (US3 null tests)
                                      T011 [P] ─┘
                                                  └── T012
                                                        ├── T013 [P]
                                                        └── T014 [P]
```

**User story dependencies**: US2 and US3 are independent of each other (different files) and can run in parallel after US1 is complete.

## Parallel Execution Examples

**Phase 3 implementation** (run all three simultaneously):
```
Agent 1: T002 — ObjectAssertions.cs
Agent 2: T003 — NumericAssertions.cs
Agent 3: T004 — StringAssertions.cs
```

**Phase 3 tests** (run all three simultaneously after T002+T003+T004):
```
Agent 1: T005 — ObjectAssertionsTests.cs
Agent 2: T006 — NumericAssertionsTests.cs
Agent 3: T007 — StringAssertionsTests.cs
```

**Phase 4 + Phase 5** (run all four simultaneously after Phase 3):
```
Agent 1: T008 — ObjectAssertionsTests.cs (comparer)
Agent 2: T009 — NumericAssertionsTests.cs (comparer)
Agent 3: T010 — StringAssertionsTests.cs (null)
Agent 4: T011 — ObjectAssertionsTests.cs (null) ← sequential after T008
```

> Note: T008 and T011 both modify `ObjectAssertionsTests.cs`. If running phases 4 and 5 concurrently, complete T008 before starting T011.

## Implementation Strategy

**MVP**: Complete Phase 3 only (T001–T007). All three `NotBe` methods are functional, all core scenarios covered, feature is independently shippable.

**Increment 2**: Add Phase 4 (T008–T009) — comparer test coverage verified.

**Increment 3**: Add Phase 5 (T010–T011) — null handling test coverage verified.

**Done**: Complete Final Phase (T012–T014) — build verified clean, full test suite green, constitution constraints confirmed.

## Summary

| Metric | Value |
|---|---|
| Total tasks | 14 |
| Phase 3 (US1 — MVP) | 6 tasks (3 impl [P] + 3 tests [P]) |
| Phase 4 (US2 — comparer) | 2 test tasks [P] |
| Phase 5 (US3 — null) | 2 test tasks [P] |
| Final Phase | 3 tasks (1 sequential + 2 [P]) |
| Parallel opportunities | 3 (impl), 3 (core tests), 2+2 (US2+US3 tests), 2 (polish) |
| New source files | 0 |
| Modified source files | 3 |
| Modified test files | 3 |
| MVP scope | Phase 3 only (T001–T007) |
