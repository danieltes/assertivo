# Tasks: Assertion Library Core

**Input**: Design documents from `/specs/00001-assertion-library-core/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included — SC-002 requires every assertion method to have at least one positive, one negative, and one edge-case test. SC-007 requires 95% line / 90% branch coverage.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Library source**: `src/Assertivo/`
- **Test project**: `tests/Assertivo.Tests/`
- Paths based on plan.md project structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution creation, project initialization, build configuration

- [X] T001 Create Assertivo.sln solution file with src/Assertivo/ and tests/Assertivo.Tests/ directory structure
- [X] T002 [P] Configure src/Assertivo/Assertivo.csproj with net10.0 TFM, PackageId Assertivo, IsPackable, IsAotCompatible, GenerateDocumentationFile, Nullable enable, ImplicitUsings enable, TreatWarningsAsErrors
- [X] T003 [P] Configure tests/Assertivo.Tests/Assertivo.Tests.csproj with net10.0, xUnit packages (xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk), and ProjectReference to src/Assertivo/Assertivo.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core engine types that ALL assertion methods depend on — failure reporting, message formatting, constraint types

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 [P] Implement AssertionFailure readonly struct with Expected, Actual, Expression, Reason, Message properties in src/Assertivo/AssertionFailure.cs
- [X] T005 [P] Implement AssertionFailedException class inheriting Exception with Expected, Actual, Expression, Reason properties in src/Assertivo/AssertionFailedException.cs
- [X] T006 Implement AssertionConfiguration static class with volatile static Action\<AssertionFailure\> ReportFailure delegate defaulting to throw AssertionFailedException in src/Assertivo/AssertionConfiguration.cs
- [X] T007 [P] Implement MessageFormatter internal static class for failure message construction (expected/actual/expression/reason formatting) and because-reason string.Format handling in src/Assertivo/MessageFormatter.cs
- [X] T008 [P] Apply [StackTraceHidden] attribute to all internal failure-path methods in AssertionConfiguration, MessageFormatter, and assertion method internals (FR-023)
- [X] T009 [P] Implement AndConstraint\<TAssertions\> readonly struct with And property in src/Assertivo/Primitives/AndConstraint.cs
- [X] T010 [P] Implement AndWhichConstraint\<TAssertions, TSubject\> readonly struct with And and Which properties in src/Assertivo/Primitives/AndWhichConstraint.cs

**Checkpoint**: Foundation ready — assertion type implementation can now begin

---

## Phase 3: User Story 1 — Write Simple Value Assertions (Priority: P1) 🎯 MVP

**Goal**: A developer can assert value equality for primitives (int, long, string, bool) and null/not-null checks using `.Should().Be()`, `.Should().BeTrue()`, `.Should().BeNull()`, etc.

**Independent Test**: Assert primitive values against expected results and verify both passing and failing paths produce correct outcomes and messages.

### Implementation for User Story 1

- [X] T011 [P] [US1] Implement ObjectAssertions\<T\> readonly struct with Be (optional IEqualityComparer\<T\>), BeSameAs (runtime guard: throw InvalidOperationException for value types), BeNull, NotBeNull returning AndConstraint in src/Assertivo/ObjectAssertions.cs
- [X] T012 [P] [US1] Implement BooleanAssertions readonly struct with BeTrue, BeFalse returning AndConstraint in src/Assertivo/BooleanAssertions.cs
- [X] T013 [P] [US1] Scaffold NumericAssertions\<T\> readonly struct (where T : struct, IComparable\<T\>, IEquatable\<T\>) with Be method (optional IEqualityComparer\<T\>) returning AndConstraint in src/Assertivo/Numeric/NumericAssertions.cs
- [X] T014 [P] [US1] Scaffold StringAssertions readonly struct with Be method returning AndConstraint in src/Assertivo/StringAssertions.cs
- [X] T015 [US1] Create ShouldExtensions static class with all 9 Should() overloads (bool, int, long, string?, Action, Func\<Task\>, IEnumerable\<T\>, IEnumerable\<KeyValuePair\<TKey,TValue\>\>, T fallback) using [CallerArgumentExpression] in src/Assertivo/Should.cs

### Tests for User Story 1

- [X] T016 [P] [US1] Write ObjectAssertions tests: Be pass/fail with default and custom comparer, Be(null) equivalent to BeNull(), BeSameAs reference equality (value-type guard throws InvalidOperationException), BeNull/NotBeNull, because reason formatting, expression capture in failure messages in tests/Assertivo.Tests/ObjectAssertionsTests.cs
- [X] T017 [P] [US1] Write BooleanAssertions tests: BeTrue pass/fail, BeFalse pass/fail, because reason in failure messages in tests/Assertivo.Tests/BooleanAssertionsTests.cs
- [X] T018 [P] [US1] Write MessageFormatter and infrastructure tests: because string.Format with becauseArgs, expression capture via CallerArgumentExpression, AssertionFailure struct content, AssertionConfiguration default behavior in tests/Assertivo.Tests/MessageFormatterTests.cs

**Checkpoint**: User Story 1 is fully functional — primitives can be asserted with .Should().Be(), .BeTrue(), .BeNull(), etc.

---

## Phase 4: User Story 2 — Assert String Properties (Priority: P1)

**Goal**: A developer can verify string content with Contain, NotContain, NotBeNullOrEmpty, and BeEmpty using ordinal case-sensitive comparison.

**Independent Test**: Assert string subjects against contain/not-contain/empty/null-or-empty and verify pass, fail, and failure message content.

### Implementation for User Story 2

- [X] T019 [US2] Add Contain, NotContain, NotBeNullOrEmpty, BeEmpty methods using StringComparison.Ordinal to StringAssertions in src/Assertivo/StringAssertions.cs

### Tests for User Story 2

- [X] T020 [US2] Write StringAssertions tests: Be pass/fail, Contain pass/fail, NotContain pass/fail with because, NotBeNullOrEmpty (non-empty/null/empty), BeEmpty, null subject behavior in tests/Assertivo.Tests/StringAssertionsTests.cs

**Checkpoint**: String assertions complete — developers can verify string content, containment, and emptiness

---

## Phase 5: User Story 8 — Chain Assertions Fluently (Priority: P1)

**Goal**: A developer can chain multiple assertions on the same subject using `.And`, or drill into a nested subject using `.Which`.

**Independent Test**: Write multi-step `.And` chains on value assertions implemented in US1/US2 and verify each link executes correctly.

**Note**: Full acceptance scenarios (ContainSingle().Which, Throw().And, BeOfType().Which) require US4, US6, US9 — validated in Phase 12 (Polish).

### Tests for User Story 8

- [X] T021 [US8] Write fluent chaining tests: .And multi-step chains for BooleanAssertions, StringAssertions, NumericAssertions, ObjectAssertions; verify each chain link executes its assertion in tests/Assertivo.Tests/ChainingTests.cs

**Checkpoint**: Basic .And chaining verified for all P1 assertion types

---

## Phase 6: User Story 3 — Assert Numeric Comparisons (Priority: P2)

**Goal**: A developer can verify numeric values with BeGreaterThanOrEqualTo and BeLessThan comparison assertions.

**Independent Test**: Assert int and long values against comparison methods with both passing and failing inputs.

### Implementation for User Story 3

- [X] T022 [US3] Add BeGreaterThanOrEqualTo (optional IComparer\<T\>) and BeLessThan (optional IComparer\<T\>) returning AndConstraint to NumericAssertions\<T\> in src/Assertivo/Numeric/NumericAssertions.cs

### Tests for User Story 3

- [X] T023 [US3] Write NumericAssertions tests: Be pass/fail for int and long, BeGreaterThanOrEqualTo (equal/greater/less), BeLessThan (less/equal), custom comparer, because reason in tests/Assertivo.Tests/NumericAssertionsTests.cs

**Checkpoint**: Numeric assertions complete — value equality and comparison both functional

---

## Phase 7: User Story 4 — Assert Collection Contents (Priority: P2)

**Goal**: A developer can verify collection count, element presence, emptiness, order-independent equivalence, single-element drill-down, and predicate satisfaction.

**Independent Test**: Assert List\<T\> and IEnumerable\<T\> with HaveCount, Contain, BeEmpty, ContainSingle, BeEquivalentTo, AllSatisfy.

### Implementation for User Story 4

- [X] T024 [US4] Implement GenericCollectionAssertions\<T\> readonly struct with null guard, HaveCount, Contain (optional IEqualityComparer\<T\>), BeEmpty returning AndConstraint in src/Assertivo/Collections/GenericCollectionAssertions.cs
- [X] T025 [US4] Add ContainSingle parameterless and predicate overloads returning AndWhichConstraint\<GenericCollectionAssertions\<T\>, T\> with 0/1/many element validation in src/Assertivo/Collections/GenericCollectionAssertions.cs
- [X] T026 [US4] Add BeEquivalentTo (IEnumerable\<T\> with optional IEqualityComparer\<T\>, and T[] overload with comparer/because) with order-independent frequency-aware comparison in src/Assertivo/Collections/GenericCollectionAssertions.cs
- [X] T027 [US4] Add AllSatisfy with per-element failure reporting and vacuous truth on empty collection in src/Assertivo/Collections/GenericCollectionAssertions.cs

### Tests for User Story 4

- [X] T028 [US4] Write GenericCollectionAssertions tests: HaveCount pass/fail, Contain with default and custom comparer, BeEmpty, ContainSingle (0/1/many elements, predicate 0/1/many matches), BeEquivalentTo (matching/mismatched/duplicates/order), AllSatisfy (pass/partial-fail/empty), null subject, .Which typed access from ContainSingle, because reason in tests/Assertivo.Tests/CollectionAssertionsTests.cs

**Checkpoint**: Collection assertions complete — count, containment, equivalence, single-element drill-down all functional

---

## Phase 8: User Story 6 — Assert Synchronous Exceptions (Priority: P2)

**Goal**: A developer can verify an Action throws a specific exception type and inspect exception properties via .Which and assert on message via .WithMessage().

**Independent Test**: Wrap throwing and non-throwing actions in .Should().Throw\<T\>() and verify exception type matching, .Which drill-down, and .WithMessage().

### Implementation for User Story 6

- [X] T029 [P] [US6] Implement ExceptionAssertions\<TException\> readonly struct with Which, And (self-reference), WithMessage (ordinal case-sensitive substring match) returning AndConstraint in src/Assertivo/Exceptions/ExceptionAssertions.cs
- [X] T030 [US6] Implement ActionAssertions readonly struct with Throw\<TException\> matching exact type and subtypes (FR-019), returning ExceptionAssertions\<TException\> in src/Assertivo/Exceptions/ActionAssertions.cs

### Tests for User Story 6

- [X] T031 [P] [US6] Write ExceptionAssertions tests: WithMessage substring pass/fail, WithMessage empty string, .Which access, .And chaining, because reason in tests/Assertivo.Tests/ExceptionAssertionsTests.cs
- [X] T032 [US6] Write ActionAssertions tests: Throw exact type, Throw subclass, Throw wrong type, no throw, .Which.Message drill-down, .Which.PropertyName drill-down, because reason in tests/Assertivo.Tests/ActionAssertionsTests.cs

**Checkpoint**: Synchronous exception assertions complete — throw verification and exception inspection functional

---

## Phase 9: User Story 5 — Assert Dictionary Keys (Priority: P3)

**Goal**: A developer can verify a dictionary contains a specific key and drill into the value via .Which.

**Independent Test**: Assert IDictionary\<K,V\> and IReadOnlyDictionary\<K,V\> subjects against ContainKey.

### Implementation for User Story 5

- [X] T033 [US5] Implement GenericDictionaryAssertions\<TKey, TValue\> readonly struct with null guard, ContainKey returning AndWhichConstraint\<GenericDictionaryAssertions\<TKey,TValue\>, TValue\> with available-keys-in-failure-message in src/Assertivo/Collections/GenericDictionaryAssertions.cs

### Tests for User Story 5

- [X] T034 [US5] Write GenericDictionaryAssertions tests: ContainKey pass/fail for IDictionary and IReadOnlyDictionary, failure message shows available keys, .Which value access, null subject, because reason in tests/Assertivo.Tests/DictionaryAssertionsTests.cs

**Checkpoint**: Dictionary assertions complete — key presence and value drill-down functional

---

## Phase 10: User Story 7 — Assert Asynchronous Exceptions (Priority: P3)

**Goal**: A developer can verify a Func\<Task\> throws a specific exception when awaited, with AggregateException unwrapping.

**Independent Test**: Wrap async lambdas in await .Should().ThrowAsync\<T\>() and verify exception capture and .Which drill-down.

### Implementation for User Story 7

- [X] T035 [US7] Implement AsyncFunctionAssertions readonly struct with ThrowAsync\<TException\> returning Task\<ExceptionAssertions\<TException\>\>, matching exact type and subtypes, unwrapping AggregateException inner exception (FR-020) in src/Assertivo/Exceptions/AsyncFunctionAssertions.cs

### Tests for User Story 7

- [X] T036 [US7] Write AsyncFunctionAssertions tests: ThrowAsync pass/fail, AggregateException unwrap, no throw, .Which access, .Which chaining to Should(), because reason in tests/Assertivo.Tests/AsyncFunctionAssertionsTests.cs

**Checkpoint**: Async exception assertions complete — async throw verification with unwrapping functional

---

## Phase 11: User Story 9 — Type Assertions (Priority: P3)

**Goal**: A developer can verify a runtime object is of an exact type (not inheritance) and drill into the typed object via .Which.

**Independent Test**: Assert objects against BeOfType\<T\>() verifying exact-match and mismatch behavior.

### Implementation for User Story 9

- [X] T037 [US9] Add BeOfType\<TTarget\> with exact type match (not inheritance) returning AndWhichConstraint\<ObjectAssertions\<T\>, TTarget\> to ObjectAssertions\<T\> in src/Assertivo/ObjectAssertions.cs

### Tests for User Story 9

- [X] T038 [US9] Write BeOfType tests: exact type match, inheritance mismatch, .Which typed access for further .Should() chains, because reason in tests/Assertivo.Tests/ObjectAssertionsTests.cs

**Checkpoint**: Type assertions complete — exact type verification and typed drill-down functional

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Cross-story verification, documentation, and final validation

- [X] T039 [P] Add XML documentation comments to all public types and members across src/Assertivo/
- [X] T040 [P] Add Coverlet to tests/Assertivo.Tests/Assertivo.Tests.csproj and configure dotnet test to collect coverage and fail below 95% line / 90% branch for src/Assertivo/ (SC-007)
- [X] T041 [P] Complete fluent chaining integration tests for US8 full scenarios: ContainSingle().Which.Property.Should().Be(), Throw\<T\>().And, BeOfType\<T\>().Which.Should() in tests/Assertivo.Tests/ChainingTests.cs
- [X] T042 Validate quickstart.md end-to-end: create minimal xUnit test project, add Assertivo reference, write first assertion, run dotnet test, confirm pass
- [X] T043 [P] Validate SC-005: create minimal console app referencing only Assertivo (no xUnit/NUnit/MSTest), exercise value/string/collection assertions, run via dotnet run, confirm assertions pass and failures throw AssertionFailedException
- [X] T044 [P] Validate SC-006: add a [MemoryDiagnoser] BenchmarkDotNet spot-check in tests/Assertivo.Benchmarks/ verifying Should().Be(42) happy path is zero-allocation (Gen0=0, Allocated=0 B)
- [X] T045 Verify solution builds with zero warnings under TreatWarningsAsErrors and passes all tests via dotnet test

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — **BLOCKS all user stories**
- **US1 (Phase 3)**: Depends on Foundational — first story, scaffolds assertion files for later stories
- **US2 (Phase 4)**: Depends on US1 (extends StringAssertions.cs created in Phase 3)
- **US8 (Phase 5)**: Depends on US1 + US2 (tests chaining on implemented assertions)
- **US3 (Phase 6)**: Depends on US1 (extends NumericAssertions.cs created in Phase 3)
- **US4 (Phase 7)**: Depends on Phase 2 only (new file: GenericCollectionAssertions.cs)
- **US6 (Phase 8)**: Depends on Phase 2 only (new files: ExceptionAssertions.cs, ActionAssertions.cs)
- **US5 (Phase 9)**: Depends on Phase 2 only (new file: GenericDictionaryAssertions.cs)
- **US7 (Phase 10)**: Depends on US6 (reuses ExceptionAssertions\<T\> from Phase 8)
- **US9 (Phase 11)**: Depends on US1 (extends ObjectAssertions.cs created in Phase 3)
- **Polish (Phase 12)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup)
    │
Phase 2 (Foundational)
    │
    ├── Phase 3 (US1: Value Assertions) ← MVP
    │       │
    │       ├── Phase 4 (US2: String Properties)
    │       │       │
    │       │       └── Phase 5 (US8: Chaining verification)
    │       │
    │       ├── Phase 6 (US3: Numeric Comparisons)
    │       │
    │       └── Phase 11 (US9: Type Assertions)
    │
    ├── Phase 7 (US4: Collections) ─── can start after Phase 2
    │
    ├── Phase 8 (US6: Sync Exceptions) ─── can start after Phase 2
    │       │
    │       └── Phase 10 (US7: Async Exceptions)
    │
    └── Phase 9 (US5: Dictionary Keys) ─── can start after Phase 2
            │
            └── Phase 12 (Polish) ← after ALL stories complete
```

### Parallel Opportunities

After Phase 2 (Foundational) completes, the following groups can proceed in parallel:
- **Group A**: US1 → US2 → US8 → US3, US9 (sequential due to shared files)
- **Group B**: US4 (new file, independent)
- **Group C**: US6 → US7 (sequential due to ExceptionAssertions dependency)
- **Group D**: US5 (new file, independent)

### Within Each User Story

1. Implementation tasks before test tasks
2. Struct creation before methods that depend on those structs
3. All methods use MessageFormatter + AssertionConfiguration from Phase 2
4. Should.cs (T014) runs after all assertion type scaffolds exist

### Parallel Example: Phase 3 (User Story 1)

```
# These implementation tasks can run in parallel (different files):
T011: ObjectAssertions<T> in src/Assertivo/ObjectAssertions.cs
T012: BooleanAssertions in src/Assertivo/BooleanAssertions.cs
T013: NumericAssertions<T> in src/Assertivo/Numeric/NumericAssertions.cs
T014: StringAssertions in src/Assertivo/StringAssertions.cs

# Then wire up entry points (depends on assertion types above):
T015: ShouldExtensions in src/Assertivo/Should.cs

# All tests can run in parallel (different files):
T016: ObjectAssertionsTests.cs
T017: BooleanAssertionsTests.cs
T018: MessageFormatterTests.cs
```

### Parallel Example: P2 Stories (after Phase 3 complete)

```
# These stories can run in parallel (different files, no cross-dependencies):
Phase 6 (US3): NumericAssertions.cs (extends existing)
Phase 7 (US4): GenericCollectionAssertions.cs (new file)
Phase 8 (US6): ExceptionAssertions.cs + ActionAssertions.cs (new files)
Phase 9 (US5): GenericDictionaryAssertions.cs (new file)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 — Simple Value Assertions
4. **STOP and VALIDATE**: `dotnet test` passes; primitives can be asserted with `.Should().Be()`, `.BeTrue()`, `.BeNull()`
5. This is a shippable MVP — developers can write basic assertions

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. **US1** → Value assertions work → **MVP!**
3. **US2** → String content assertions work
4. **US8** → Chaining verified for P1 assertions
5. **US3** → Numeric comparison assertions work
6. **US4** → Collection assertions work (biggest single story)
7. **US6** → Synchronous exception assertions work
8. **US5** → Dictionary key assertions work
9. **US7** → Async exception assertions work
10. **US9** → Type assertions work
11. **Polish** → Full chaining integration, XML docs, quickstart validation

### Parallel Team Strategy

With multiple developers after Foundational phase:

1. **Developer A**: US1 → US2 → US8 → US3 → US9 (shared files: ObjectAssertions, StringAssertions, NumericAssertions)
2. **Developer B**: US4 → US5 (collection + dictionary — new files, fully independent)
3. **Developer C**: US6 → US7 (exception assertions — new files, sequential dependency)
4. **All**: Polish phase after stories converge

---

## Notes

- [P] tasks = different files, no task-level dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable at its checkpoint
- Commit after each task or logical group
- All assertion structs are `readonly struct` for zero-allocation happy path (research.md Topic 1)
- All assertion methods accept optional `string because = ""` and `params object[] becauseArgs` (FR-012)
- All equality/comparison methods accept optional comparer parameters per clarification (FR-002, FR-004, FR-006)
- Should.cs scaffolds all 9 overloads in T014 even though some assertion types are not yet implemented — those overloads will compile against scaffold types and gain methods in later phases
- US8 (Fluent Chaining) has no new production code — it's a design property emerging from correct return types; Phase 5 and Phase 12 tasks verify it works
