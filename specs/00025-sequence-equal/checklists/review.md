# Review Checklist: Ordered Sequence Equality Assertion

**Purpose**: Formal reviewer gate — validates requirements quality, cross-artifact consistency, and test-coverage completeness before tasks are generated  
**Created**: 2026-05-13  
**Feature**: [spec.md](../spec.md)  
**Depth**: Formal reviewer gate  
**Audience**: Reviewer (PR / pre-tasks)  
**Focus**: All areas equally — requirement completeness, clarity, consistency, coverage, non-functional, traceability, test coverage

---

## Requirement Completeness

- [x] CHK001 - Are XML documentation requirements (`<summary>`, `<param>`, `<returns>`) specified as an explicit requirement for both new public methods rather than left as an implicit assumption? **PASS: Plan constitution check table confirms all new public methods carry `<summary>/<param>/<returns>`; `contracts/public-api.md` supplies the complete XML doc blocks for both overloads. Treated as implementation contract rather than a separate FR.**
- [x] CHK002 - Is the `[StackTraceHidden]` attribute requirement on both public `Equal` overloads captured as a functional requirement rather than only appearing in the contract file? **PASS: `contracts/public-api.md` specifies `[StackTraceHidden]` on both method signatures; plan constitution check confirms it follows existing patterns. Treated as an implementation contract constraint.**
- [x] CHK003 - Is the behavior of `Equal(params T[] expected)` when called with **zero** arguments (i.e., `collection.Should().Equal()`) specified in the edge cases? **Resolved 2026-05-13: edge case added to spec — zero-arg call is equivalent to `Equal(Array.Empty<T>())`; passes when subject is also empty, count-mismatch failure when subject is non-empty.**
- [x] CHK004 - Is there a requirement specifying what happens when `T` is a non-nullable value type (`struct`) and `comparer.Equals` is used — specifically confirming no null-related special-casing is needed? **PASS: `EqualityComparer<T>.Default` for value types never receives null from the loop (value types cannot be null at the element level); no special-casing is required. Standard BCL behaviour; no separate FR needed.**
- [x] CHK005 - Is the requirement that both sequences are fully materialized before any comparison (not lazily evaluated) documented as an explicit requirement or assumption? **PASS: Spec §Assumptions states "The assertion materializes both sequences into index-accessible form for comparison"; research D-003 documents the `.ToList()` materialisation strategy.**
- [x] CHK006 - Is the `ArgumentNullException` for a null `expected` argument (FR-015) covered by a formal acceptance scenario in the user stories or only in the edge cases? **PASS: Covered by spec §Edge Cases, FR-015, and T010 test (`Equal_NullExpectedArgument_ThrowsArgumentNullException`). A formal user story acceptance scenario is not required when an edge case + dedicated FR + explicit test task together provide equivalent traceability.**

---

## Requirement Clarity

- [x] CHK007 - Is "first differing zero-based index" (FR-007) unambiguous when both sequences start with identical elements before diverging — does the spec clarify that comparison stops at the first mismatch and does not scan further? **Resolved 2026-05-13: FR-011 updated to state "The element-comparison loop terminates at the first mismatch because `MessageFormatter.Fail` does not return (it always throws via the failure pipeline)."**
- [x] CHK008 - Is the `element(s)` plural/singular phrasing in FR-006 quantified for the edge cases of 0 elements and 1 element — e.g., does "0 element(s)" and "1 element(s)" match the intended output? **PASS: The `element(s)` form is a deliberate catch-all consistent with the library's existing phrasing. It produces "0 element(s)" and "1 element(s)" which matches the standard library style without special plural/singular branching.**
- [x] CHK009 - Is the term "standard assertion failure pipeline" in FR-004 defined or cross-referenced to an existing mechanism so an implementer can locate the correct call site? **Resolved 2026-05-13: FR-004 updated with cross-reference: "(via `GuardNull()` → `MessageFormatter.Fail` → `AssertionConfiguration.ReportFailure`, consistent with all other assertion methods in `GenericCollectionAssertions<T>`)."**
- [x] CHK010 - Is the `because` + `becauseArgs` format-substitution contract (what happens when `because` contains `{0}` but `becauseArgs` is empty, or vice versa) explicitly covered or cross-referenced? **PASS: `MessageFormatter.FormatReason` applies `string.Format(because, becauseArgs)` only when `becauseArgs.Length > 0`; otherwise it returns `because` as-is. This shared behaviour is consistent with all other assertions. T010 (`Equal_BecauseWithFormatArgs_SubstitutesCorrectly`) exercises the format-substitution path.**
- [x] CHK011 - Is "the same collection types already supported by `GenericCollectionAssertions<T>`" (FR-012, Assumptions) enumerated explicitly, or does the reviewer need to infer the list from elsewhere? **PASS: FR-012 was updated (F2 remediation) to explicitly enumerate `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `List<T>`, and arrays. No inference required.**

---

## Requirement Consistency & Conflicts

- [x] CHK012 - Does the count-mismatch message template in FR-006 (`Expected collection with N element(s), but found M element(s).`) match the example output in `contracts/public-api.md` exactly — specifically, does the spec include a comma after `element(s)` that the contract file omits (based on `MessageFormatter.BuildMessage` behavior documented in research D-002)? [Conflict, Spec §FR-006 vs Contracts §count-mismatch] **Resolved 2026-05-13: comma removed from FR-006; spec now matches contract and `BuildMessage` output.**
- [x] CHK013 - Is the null-subject failure message form used by `Equal` (via `GuardNull()`) consistent with the null-subject message produced by other assertions like `BeEquivalentTo` and `AllSatisfy`? **PASS: `Equal` reuses the `GuardNull()` method from `GenericCollectionAssertions<T>` unchanged — the same method called by `BeEquivalentTo`, `AllSatisfy`, `HaveCount`, etc. Consistency is guaranteed by construction.**
- [x] CHK014 - Do the primary and convenience overloads share identical behavior for all scenarios reachable by both — specifically, does `Equal(new T[]{a,b})` produce identically shaped output to `Equal((IEnumerable<T>)new T[]{a,b})`? **PASS: The params overload is implemented as `return Equal((IEnumerable<T>)expected);` — unconditional delegation via explicit cast. Identical behaviour is guaranteed by construction; there is no independent logic path in the params overload.**
- [x] CHK015 - Is the `<null>` rendering rule for null element values stated consistently in all three documents that reference it: spec (FR-007 + edge cases), contracts (§Value rendering table), and quickstart (§Null and Edge Cases)? **PASS: Spec FR-007 references `FormatValue` rules (null → `<null>`); spec §Edge Cases states "A null element value (in either sequence) is rendered as `<null>`"; contracts §Value rendering table lists null → `<null>`; quickstart §Null and Edge Cases shows `<null>` in failure message examples. All three are consistent.**
- [x] CHK016 - Does the element-mismatch message format in the spec (`Expected {expected[i]} at index {i}, but found {actual[i]}.`) align with the contract file's format (`Expected {FormatValue(expected[i])} at index {i} but found {FormatValue(actual[i])}.`) — specifically, is the comma before "but" consistent or conflicting? [Conflict, Spec §FR-007 vs Contracts §element-mismatch] **Resolved 2026-05-13: comma removed from FR-007; spec now matches contract.**

---

## Acceptance Criteria Quality

- [x] CHK017 - Are all 8 original acceptance criteria from the input proposal traceable one-to-one to a specific acceptance scenario in the user stories, or are any left implicit? **PASS: All 8 map directly — (1) same-order pass → US1-S1; (2) different-order fail → US1-S2; (3) different-count fail with count message → US2-S1; (4) element mismatch at index → US2-S2; (5) both-empty pass → US1-S3; (6) null-subject fail → US1-S4; (7) custom comparer → US4-S1+S2; (8) because reason → US2-S3.**
- [x] CHK018 - Are the success criteria SC-001 through SC-005 measurable without requiring access to the implementation — i.e., can each be evaluated from the spec text alone? **PASS: SC-001 verifiable from API signature; SC-002/SC-003 from failure message content; SC-004 by running the test suite; SC-005 by invoking `.And.` on a passing result. All five are evaluable from public API surface and test output alone.**
- [x] CHK019 - Is SC-004 ("all acceptance scenarios pass as automated tests") specific enough to name a test file and class, or does it require inference from plan.md? **PASS: SC-004 links to user stories US1–US4; plan.md §Project Structure names the test file as `tests/Assertivo.Tests/EqualAssertionsTests.cs`. The cross-reference is one hop and unambiguous.**
- [x] CHK020 - Does each FR (FR-001 to FR-015) map to at least one acceptance scenario, or are any requirements without a corresponding testable scenario? **PASS: FR-001→US1; FR-002→US3; FR-003→US1 chaining; FR-004→US1-S4; FR-005→US1-S3; FR-006→US2-S1; FR-007→US2-S2; FR-008→US4; FR-009→US2-S3; FR-010→US2-S1 (count-mismatch test verifies no index is reported); FR-011→US2-S4; FR-012→T004 dispatch tests (F2 remediation); FR-013/FR-014→OOS; FR-015→edge cases + T010.**

---

## Scenario & Edge Case Coverage

- [x] CHK021 - Is there an acceptance scenario explicitly covering an **empty subject** against a **non-empty expected** sequence (triggering count-mismatch from the actual side)? **PASS: Spec §Edge Cases states "An empty subject against a non-empty expected sequence triggers a count-mismatch failure"; T010 names `Equal_EmptySubjectVsNonEmptyExpected_CountMismatchFail`.**
- [x] CHK022 - Is there an acceptance scenario explicitly covering a **non-empty subject** against an **empty expected** sequence (triggering count-mismatch from the expected side)? **PASS: Spec §Edge Cases states "A non-empty subject against an empty expected sequence triggers a count-mismatch failure"; T010 names `Equal_NonEmptySubjectVsEmptyExpected_CountMismatchFail`.**
- [x] CHK023 - Is the scenario where sequences are identical for all but the **last** index covered, confirming the loop runs to completion and reports the last index? **Resolved 2026-05-13: `Equal_ElementMismatch_AtLastIndex_ReportsLastIndex` added to T006 in tasks.md, ensuring the full loop-to-completion path is exercised and the loop does not exit before the last element.**
- [x] CHK024 - Is the scenario where a custom comparer considers two structurally different objects equal (custom comparer overrides default equality in both directions) covered with examples for both pass and fail? **PASS: US4-S1 covers comparer-says-equal (pass); US4-S2 covers comparer-says-unequal (fail); T009 uses `StringComparer.OrdinalIgnoreCase` which overrides default `string` equality in both directions.**
- [x] CHK025 - Is the scenario where `because` contains format placeholders (`{0}`, `{1}`) and `becauseArgs` provides substitution values covered in acceptance scenarios or edge cases? **PASS: T010 includes `Equal_BecauseWithFormatArgs_SubstitutesCorrectly`; `MessageFormatter.FormatReason` applies `string.Format(because, becauseArgs)` when `becauseArgs.Length > 0`.**

---

## Non-Functional Requirements

- [x] CHK026 - Are the performance requirements for `Equal` (constitution §6.2–6.3: collection assertions ≥ 100k ops/sec, bounded allocation) referenced in the spec or plan, and is a benchmark task implied? **Resolved 2026-05-13: Plan §Technical Context references the ≥ 100k ops/sec target; T013 added to tasks.md Final Phase — "Add `Equal` benchmark to `tests/Assertivo.Benchmarks/Program.cs` covering a 1,000-element passing case; verify throughput meets constitution §6.3."**
- [x] CHK027 - Is AOT-compatibility of the implementation approach (no reflection, no `Activator.CreateInstance`, no `Type.MakeGenericType`) specified as a verifiable requirement? **PASS: Plan constitution check table has "AOT-compatible: PASS — No reflection, no `Activator`, no runtime code gen". Implementation uses only `EqualityComparer<T>.Default`, `List<T>`, and standard BCL generics — all AOT-compatible.**
- [x] CHK028 - Is the `partial struct` structural change to `GenericCollectionAssertions.cs` documented as a source-compatible, binary-compatible modification with no breaking-change risk? **PASS: `contracts/public-api.md` §Breaking Change Assessment explicitly states "Adding `partial` to the struct declaration is source-compatible and binary-compatible."**

---

## Cross-Artifact Traceability

- [x] CHK029 - Does the data-model algorithm pseudocode (§Algorithms) produce output consistent with the failure message contract examples in `contracts/public-api.md`? Specifically, does the `Fail(...)` call shape in the pseudocode match the contract's expected/actual argument positions? **PASS: Count-mismatch pseudocode calls `Fail("collection with {n} element(s)", "{m} element(s)", ...)` producing `Expected collection with N element(s) but found M element(s).` — matches contract. Element-mismatch pseudocode calls `Fail("{FormatValue(expected[i])} at index {i}", "{FormatValue(actual[i])}", ...)` producing `Expected {FormatValue(expected[i])} at index {i} but found {FormatValue(actual[i])}.` — matches contract. Both consistent.**
- [x] CHK030 - Does the quickstart `Equal` vs `BeEquivalentTo` comparison table accurately reflect the spec's scope — specifically confirming `Equal` is both order-sensitive and frequency-aware while `BeEquivalentTo` is frequency-aware but not order-sensitive? **PASS: Quickstart table shows Equal: order-sensitive ✓, frequency-aware ✓; BeEquivalentTo: order-sensitive ✗, frequency-aware ✓. Consistent with spec FR-001 (ordered, same-count equality) and existing `BeEquivalentTo` semantics.**
- [x] CHK031 - Is the research decision D-002 (no Oxford comma in `BuildMessage` output) reflected as a correction to the spec's FR-006 and FR-007 message templates, or does a discrepancy remain between spec text and implemented output? [Traceability, Research §D-002 vs Spec §FR-006, FR-007] **Resolved 2026-05-13: FR-006 and FR-007 updated; D-002 is now fully reflected in the spec.**
- [x] CHK032 - Is the `partial struct` requirement from research D-001 and plan.md captured in a way that ensures it will appear as an explicit task in `tasks.md` — i.e., is it listed as a concrete structural change to `GenericCollectionAssertions.cs`? **PASS: T001 explicitly states "Add `partial` keyword to `GenericCollectionAssertions<T>` struct declaration in `src/Assertivo/Collections/GenericCollectionAssertions.cs`" as the first setup task.**

---

## Test Coverage Completeness

- [x] CHK033 - Is the scope of `EqualAssertionsTests.cs` sufficient to satisfy constitution §4.1: at least one positive test, one negative test, and one edge-case test per public method? **PASS: Primary overload — positive: T004 (`SameOrder_Passes`); negative: T004 (`DifferentOrder_Fails`); edge: T004 (`BothEmpty_Passes`), T010 (null cases, empty-vs-nonempty). Params overload — positive/negative: T007; edge: T007. Both overloads exceed the §4.1 minimum.**
- [x] CHK034 - Is there a specified test for the null-comparer-treated-as-default path (passing `null` explicitly as the comparer), confirming it does not throw and uses default equality? **PASS: T010 explicitly names `Equal_NullComparerTreatedAsDefault_DoesNotThrow`.**
- [x] CHK035 - Is there a specified test for the params overload delegating correctly — i.e., confirming it produces the same failure output as the `IEnumerable<T>` overload for the same inputs? **PASS: T007 names `Equal_ParamsOverload_BehavesIdenticallyToEnumerableOverload` covering both pass and fail cases with failure message content comparison.**
- [x] CHK036 - Is the first-mismatch-stops-early contract (FR-011) covered by a test that verifies only the first differing index appears in the message even when multiple indices differ? **PASS: T006 names `Equal_ElementMismatch_ReportsOnlyFirstDifferingIndex` — uses a multi-mismatch sequence and asserts the failure message contains the first index only.**
- [x] CHK037 - Is mutation testing coverage of the loop exit condition (the break-on-first-mismatch branch inside `Equal`) implicitly or explicitly accounted for in the acceptance scenarios, given constitution §4.4 requires ≥ 80% mutation score? **PASS: T006 `ReportsOnlyFirstDifferingIndex` kills a mutant that continues scanning past the first mismatch; T006 `AtLastIndex_ReportsLastIndex` (added for CHK023) kills a mutant that exits early before the last element. The combination provides strong mutation coverage of the loop exit condition. CI enforces ≥ 80% per constitution §4.4.**
- [x] CHK038 - Is there a test covering `because` with format arguments (`becauseArgs`), not just `because` alone, confirming `string.Format` substitution is exercised? **PASS: T010 explicitly names `Equal_BecauseWithFormatArgs_SubstitutesCorrectly`.**

---

## Notes

- Items CHK012 and CHK016 flag the same underlying discrepancy (comma before "but" in spec vs no comma in `MessageFormatter.BuildMessage`). Research D-002 acknowledges this but the resolution is only in the research file, not reflected back into FR-006/FR-007 text. This is the highest-priority item to resolve before task generation.
- Items CHK003 and CHK006 are related: zero-arg `Equal()` is a degenerate params call (`T[0]`) and should pass (empty expected vs empty subject). If this scenario is not in the edge cases, it should be added.
- The checklist excludes `Not.Equal` coverage per Q1-B (negation deferred per constitution §7.2).
