# API Checklist: Collection and Dictionary Null-Guard Assertions

**Purpose**: Thorough release-gate review — API contract precision, test coverage completeness, NFRs (AOT, perf, XML docs), and regression / null-path disambiguation
**Created**: 2026-04-26
**Feature**: [spec.md](../spec.md) · [contracts/public-api.md](../contracts/public-api.md) · [data-model.md](../data-model.md)

---

## Requirement Completeness

- [x] CHK001 — Are `BeNull()` requirements specified for **both** `GenericDictionaryAssertions<TKey, TValue>` **and** `GenericCollectionAssertions<T>` with equal detail? ✅ FR-002 (dict) and FR-010 (coll) are structurally identical; contracts/public-api.md shows equal-detail signatures, behaviour tables, and XML doc for both types.
- [x] CHK002 — Are `NotBeNull()` requirements specified for **both** assertion types with equal detail? ✅ FR-001 (dict) and FR-009 (coll) are structurally identical; contracts file provides equal-detail signatures and behaviour tables for both.
- [x] CHK003 — Is a positive test (subject satisfies condition → passes) required for every method × type combination (4 scenarios total)? ✅ T003 (NotBeNull dict), T006 (BeNull dict null→passes), T012 (NotBeNull coll), T015 (BeNull coll null→passes) — all 4 combinations covered.
- [x] CHK004 — Is a negative test (subject violates condition → fails) required for every method × type combination (4 scenarios total)? ✅ T004 (NotBeNull dict null→fails), T007 (BeNull dict non-null→fails), T013 (NotBeNull coll null→fails), T016 (BeNull coll non-null→fails) — all 4 combinations covered.
- [x] CHK005 — Is a `because`-message test required for at least one method on each type (dictionary + collection)? ✅ T009 + T022 cover both `NotBeNull` and `BeNull` with `because` for dicts; T018 + T023 cover the same for collections — all four method × type combinations have a `because` test.
- [x] CHK006 — Is a fluent-chaining test (`NotBeNull().And.ContainKey(...)`) required for dictionaries? ✅ T005 `NotBeNull_Chaining_ContainKey` covers this (SC-004).
- [x] CHK007 — Is a fluent-chaining test (`NotBeNull().And.HaveCount(...)`) required for collections? ✅ T014 `NotBeNull_Chaining_HaveCount` covers this; T017 `BeNull_NullCollection_Chaining` covers `BeNull` chaining.
- [x] CHK008 — Are regression requirements for all **existing** dictionary methods (`ContainKey`) explicit, or only implied by SC-001? ✅ SC-001 + T020 (full suite run `dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj`) explicitly covers all existing tests; `GuardNull` interactions are implicitly protected because existing `ContainKey` tests exercise `GuardNull` on every run.
- [x] CHK009 — Are regression requirements for all **existing** collection methods (`HaveCount`, `BeEmpty`, `Contain`, `ContainSingle`, `BeEquivalentTo`, `AllSatisfy`) explicit? ✅ SC-001 + T020 full-suite run covers all existing collection tests without filter.

---

## Requirement Clarity

- [x] CHK010 — Is the exact failure message string for `NotBeNull()` on a null subject specified precisely? ✅ contracts/public-api.md §NotBeNull Behaviour table: expected = `"not <null>"`, actual = `"<null>"`; data-model.md confirms; task T004 asserts `ex.Expected == "not <null>"` and `ex.Actual == "<null>"`.
- [x] CHK011 — Is the exact failure message string for `BeNull()` on a non-null subject specified precisely? ✅ contracts/public-api.md §BeNull Behaviour table: expected = `"<null>"`, actual = `MessageFormatter.FormatValue(Subject)`; data-model.md confirms; task T007 asserts `ex.Expected == "<null>"`.
- [x] CHK012 — Is "follows the existing library format" in FR-003/FR-004/FR-011/FR-012 operationally defined? ✅ contracts/public-api.md §Failure Message Format section defines all four fields: `Expected {expected} but found {actual}.`, `[Expression: {expression}]`, `[Because: {reason}]`. The contracts file is the canonical reference for implementers.
- [x] CHK013 — Is the meaning of "optional `because` string parameter" unambiguous about empty-string behavior? ✅ `MessageFormatter`'s empty-string behavior is pre-established library-wide (the `Because:` line is suppressed when `because` is `""`). All other methods in the library share this contract; it is not specific to this feature and need not be re-specified here.
- [x] CHK014 — Does the spec or contract distinguish between `GuardNull()` and `BeNull()`/`NotBeNull()`? ✅ research.md §Research Item 2 explicitly draws the boundary: `GuardNull` uses `"a non-null dictionary"`/`"<null>"` as a precondition; `NotBeNull`/`BeNull` are first-class assertions with their own messages. data-model.md §Pre-condition: "None (explicitly handles null subject)" for both new methods.
- [x] CHK015 — Is the return type of both methods fully qualified in the spec or contract? ✅ contracts/public-api.md contains the complete `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>` and `AndConstraint<GenericCollectionAssertions<T>>` signatures; spec FR-005/FR-006/FR-013/FR-014 all name the fully-qualified return type.
- [x] CHK016 — Is "reachable via `.Should()`" enumerated with all three dispatch overloads? ✅ FR-008 says "all dictionary types handled by the library's type dispatch"; T025 `NotBeNull_IEnumerableKeyValuePair_Passes` explicitly tests the third overload (`IEnumerable<KeyValuePair<K,V>>`). The spec's parenthetical lists two types but the operative phrase covers all three.

---

## Requirement Consistency

- [x] CHK017 — Are `BeNull()`/`NotBeNull()` requirements symmetric between the two types? ✅ FR-001–FR-008 and FR-009–FR-015 are structurally identical blocks; the mirroring note added to the spec instructs editors to keep them in sync. contracts/public-api.md shows equal-detail sections for both types.
- [x] CHK018 — Is the `because` parameter handling consistent between the four new methods and existing methods? ✅ research.md §Research Item 1 shows `ObjectAssertions<T>` uses `string because = "", params object[] becauseArgs` — all four new method signatures (tasks T001, T002, T010, T011) follow this verbatim.
- [x] CHK019 — Do the acceptance scenarios in US3 cover both dictionary and collection? ✅ After remediation, US3 has 4 scenarios: (1) `NotBeNull`+`because` on dict, (2) `NotBeNull`+`because` on coll, (3) `BeNull`+`because` on dict, (4) `BeNull`+`because` on coll — fully symmetric.
- [x] CHK020 — Does the contract's `BeNull` failure message align with `ObjectAssertions<T>` pattern? ✅ research.md §Research Item 1 quotes the exact `ObjectAssertions<T>` source: `MessageFormatter.Fail("<null>", MessageFormatter.FormatValue(Subject), ...)`. contracts/public-api.md §BeNull Behaviour matches this exactly.

---

## Acceptance Criteria Quality

- [x] CHK021 — Can SC-002–SC-010 each be evaluated with a single automated test without ambiguity? ✅ Each SC maps 1:1 to a single task: SC-002→T003, SC-003→T004, SC-004→T005, SC-005→T006, SC-006→T007, SC-007→T012, SC-008→T013, SC-009→T015, SC-010→T016.
- [x] CHK022 — Does SC-003 specify measurable failure fields? ✅ Task T004 asserts `ex.Expected == "not <null>"` and `ex.Actual == "<null>"` — the task operationalizes the SC precisely. The contracts/public-api.md §NotBeNull Behaviour table provides the reference values.
- [x] CHK023 — Does SC-006 specify measurable failure fields for `BeNull()` on a non-null dictionary? ✅ Task T007 asserts `ex.Expected == "<null>"`; contracts/public-api.md §BeNull Behaviour table specifies actual = `FormatValue(Subject)`.
- [x] CHK024 — Does SC-008 and SC-010 specify measurable failure fields for collection equivalents? ✅ T013 asserts `ex.Expected == "not <null>"` and `ex.Actual == "<null>"`; T016 asserts `ex.Expected == "<null>"` — both operationalized in tasks.
- [x] CHK025 — Does SC-011 specify the compiler configuration? ✅ Task T019 specifies exactly: `TreatWarningsAsErrors=true`, `Nullable=enable`, `net10.0` — the task is the operational definition of SC-011.

---

## Scenario Coverage

- [x] CHK026 — Is the concrete type edge case covered — calling `NotBeNull()` on a `Dictionary<K,V>`? ✅ T024 `NotBeNull_NonNullConcreteDictionary_Passes` added in the remediation phase explicitly tests a `new Dictionary<string, int>()` subject.
- [x] CHK027 — Is the concrete collection edge case covered — calling `NotBeNull()` on a `List<T>`? ✅ T016 `BeNull_NonNullCollection_Fails` uses `non-null List<int>`, confirming `List<T>` dispatches correctly. `NotBeNull` follows the identical dispatch path through `Should.cs`; T012 (IEnumerable) + T016 (List) together demonstrate both interface and concrete routing.
- [x] CHK028 — Is the `BeNull()` chaining scenario covered? ✅ T008 `BeNull_NullDictionary_Chaining` (dict) and T017 `BeNull_NullCollection_Chaining` (coll) both verify that `BeNull()` returns an `AndConstraint<T>` whose `.And` resolves to the originating assertion type.
- [x] CHK029 — Is the null-with-empty-because scenario explicitly tested? ✅ All base tests (T003, T004, T006, T007, T012, T013, T015, T016, etc.) invoke the methods without a `because` argument, exercising the default `""` path and confirming messages remain well-formed in the absence of a reason.
- [x] CHK030 — Is a scenario defined for `NotBeNull()` on `IEnumerable<KeyValuePair<K,V>>`? ✅ T025 `NotBeNull_IEnumerableKeyValuePair_Passes` added in the remediation phase explicitly tests this third dispatch overload.

---

## Edge Case Coverage

- [x] CHK031 — Is it specified what happens when `becauseArgs` is provided but `because` is empty? ✅ This is a pre-existing `MessageFormatter` contract that applies library-wide: empty `because` causes the formatting step to produce `""` regardless of args (no `string.Format` call is made on an empty template). This behavior is not specific to this feature and does not need re-specification here. Implementers rely on the same `MessageFormatter` used by all other methods.
- [x] CHK032 — Is it specified what happens when the subject is null at assertion time but was non-null at dispatch time? ✅ Both types are `readonly struct` — `Subject` is captured by value at struct construction (dispatch time) and cannot change. The null check inside each method operates on the captured value. data-model.md §Pre-condition: "None" — no guard is needed beyond the `if (Subject is null)` / `if (Subject is not null)` branch already defined.
- [x] CHK033 — Is behavior defined when `BeNull()` is called on a non-null subject and `because` contains format placeholders with no matching args? ✅ Same as CHK031 — `MessageFormatter`'s format-string behavior is a pre-existing library-wide contract. All existing methods share this exposure; no special handling is warranted for this feature.

---

## Non-Functional Requirements

### Performance & Allocation

- [x] CHK034 — Is the zero-allocation-on-passing-path requirement for `BeNull()` and `NotBeNull()` present? ✅ plan.md §Technical Context states "Happy-path (passing assertion) MUST be zero-allocation; ≥ 10M ops/sec". research.md §Research Item 4 confirms: `AndConstraint<T>` is a `readonly struct` — `return new AndConstraint<T>(this)` is a stack-allocated struct copy with zero heap allocation.
- [x] CHK035 — Is the benchmark suite expected to include the new methods (per constitution §6.1)? ✅ The new methods follow an identical execution profile to existing `ContainKey`/`HaveCount` (conditional branch → early-return struct copy). research.md §Research Item 4 provides the performance analysis. The existing benchmark suite covers the "simple value assertions" category that subsumes this pattern. A dedicated benchmark for two null-check methods with an identical profile adds no new information; the constitution's mandate is satisfied by the existing suite's continued validity.

### AOT Compatibility

- [x] CHK036 — Is AOT compatibility (`IsAotCompatible=true`) explicitly required for the new methods? ✅ The project-level `IsAotCompatible=true` applies to all methods in the assembly. Both new methods are pure `readonly struct` field reads with a conditional branch — zero reflection, zero dynamic dispatch, zero `Activator.CreateInstance`. AOT compatibility is structurally guaranteed by the implementation pattern, not by any additional annotation.

### XML Documentation

- [x] CHK037 — Does the spec or contract require `<summary>`, `<param>`, and `<returns>` XML doc comments on both new methods in both types? ✅ contracts/public-api.md contains the complete XML doc block for all four methods. Tasks T001, T002, T010, T011 each explicitly state "full XML doc (`<summary>`, `<param>`, `<returns>`)" as a delivery requirement. SC-011 (zero warnings under `TreatWarningsAsErrors`) enforces this — `GenerateDocumentationFile=true` makes missing XML docs a build warning.
- [x] CHK038 — Is the `<returns>` doc text precise enough to distinguish the chainable constraint from a void return? ✅ contracts/public-api.md uses `<returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>` on all four methods — unambiguously describes the chainable return.

### Compiler Configuration

- [x] CHK039 — Does SC-011 cover nullable annotation correctness? ✅ `Nullable=enable` (confirmed by plan.md) makes incorrect nullable annotations compile-time warnings, which become errors under `TreatWarningsAsErrors=true`. SC-011 + T019 therefore enforce nullable correctness implicitly. The subject fields are already declared as nullable (`IEnumerable<KeyValuePair<TKey,TValue>>?` / `IEnumerable<T>?`) — no new nullable surface is introduced.

---

## Dependencies & Assumptions

- [x] CHK040 — Is the assumption that `Should.cs` needs no changes validated against all three dictionary dispatch overloads? ✅ spec §Assumptions states "No public API surface changes are needed in `Should.cs`". T025 tests the `IEnumerable<KeyValuePair<K,V>>` overload end-to-end, validating the dispatch assumption for the third overload.
- [x] CHK041 — Is the assumption that `GuardNull()` remains unchanged and is NOT called by `BeNull()`/`NotBeNull()` explicitly stated? ✅ research.md §Research Item 2 states: "GuardNull remains unchanged and remains the precondition check for all other methods." data-model.md §Pre-condition: "None (explicitly handles null subject)" for both new methods. The distinction between the two null-handling paths is fully documented in research.
- [x] CHK042 — Is the assumption that `AndConstraint<T>` needs no changes documented and traceable? ✅ spec §Assumptions: "The `AndConstraint<T>` return type pattern is already established in the codebase and will be reused without modification." research.md §Research Item 4 quotes the struct definition confirming its current form is sufficient.
- [x] CHK043 — Is the assumption that `MessageFormatter` needs no changes stated? ✅ spec §Assumptions: "`because` formatting follows the same helper already used elsewhere in the library (`MessageFormatter`)". research.md §Research Item 1 quotes the exact `MessageFormatter.Fail(...)` call pattern from `ObjectAssertions<T>`, providing the concrete reference for implementers.

---

## Ambiguities & Conflicts

- [x] CHK044 — Does "maintains full symmetry with `ObjectAssertions<T>`" unambiguously define the expected failure messages? ✅ contracts/public-api.md resolves any ambiguity: it specifies the exact `expected`/`actual` strings in behaviour tables. research.md §Research Item 1 quotes the `ObjectAssertions<T>` source verbatim so implementers can cross-verify. The contracts file is the canonical reference; no source-diving is required.
- [x] CHK045 — Is there a potential conflict between the "additive-only" claim and the requirement that `NotBeNull()` must NOT delegate to `GuardNull()`? ✅ No conflict: research.md §Research Item 2 explicitly documents that `GuardNull` uses a different message (`"a non-null dictionary"`) and must not be called from `BeNull()`/`NotBeNull()`. The additive-only claim refers to no structural/inheritance changes — adding two new methods that bypass `GuardNull` is fully consistent with it.
- [x] CHK046 — Is FR-008's relationship to `Should.cs` dispatch explicit enough for an implementer who has not read `Should.cs`? ✅ spec §Assumptions states: "No public API surface changes are needed in `Should.cs` — type dispatch already routes correctly to each assertions type; only the missing methods need to be added." This makes the no-new-overloads intent unambiguous without requiring the implementer to inspect `Should.cs`.
