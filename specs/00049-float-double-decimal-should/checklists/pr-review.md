# PR Review Checklist: Float/Double/Decimal Should Dispatch

**Purpose**: Validate requirements quality for API contract and test coverage before merge — tests the spec, not the implementation
**Created**: 2026-06-10
**Feature**: [spec.md](../spec.md) | [contracts/public-api.md](../contracts/public-api.md)
**Audience**: PR reviewer
**Scope**: New float, double, decimal overloads only

---

## API Contract Requirements Quality

- [x] CHK001 Are the three new `Should()` overload signatures (subject type, return type, optional caller parameter) completely and unambiguously specified for each of `float`, `double`, and `decimal`? [Completeness, contracts/public-api.md]
- [x] CHK002 Is the `[CallerArgumentExpression]` parameter requirement explicitly stated for each new overload, not just implied by analogy with `int`/`long`? [Completeness, Spec §FR-008]
- [x] CHK003 Is the return type `NumericAssertions<T>` explicitly documented for all three overloads, or does the spec rely on the reader inferring it? [Clarity, Spec §FR-001, FR-002, FR-003]
- [x] CHK004 Is the "no fallthrough to `ObjectAssertions<T>`" requirement stated precisely enough that a reviewer can verify it from the PR diff without reading the full codebase? [Clarity, Spec §FR-007]
- [x] CHK005 Are the failure message requirements specific enough that two reviewers would independently agree on whether a given message is correct? [Clarity, Spec §FR-009]
- [x] CHK006 Is the failure message format requirement consistent between `spec.md` and `contracts/public-api.md`, with no contradictory or divergent examples? [Consistency, Spec §SC-003, contracts/public-api.md]
- [x] CHK007 Are XML documentation requirements for each new public overload explicitly called out, or is this left as an implicit convention? [Completeness, Spec §FR-008, constitution §III.2] — captured in plan.md constitution check and tasks.md T003
- [x] CHK008 Does the spec enumerate which assertion methods (`Be`, `NotBe`, `BeGreaterThanOrEqualTo`, `BeLessThan`) must be available for the new types, rather than using an open-ended "all methods"? [Clarity, Spec §FR-006] — FR-006 names them parenthetically; contracts/public-api.md enumerates all four exhaustively
- [x] CHK009 Is the method chaining requirement (`And`) for the new types stated explicitly, or only present in quickstart examples without a formal requirement? [Completeness, quickstart.md, Spec §User Story 1] — documented in Spec Assumptions as structurally guaranteed by `AndConstraint<NumericAssertions<T>>` return type
- [x] CHK010 Is the "no changes to `NumericAssertions<T>` internals" constraint stated as a verifiable acceptance criterion, not merely an assumption? [Measurability, Spec §SC-005] — SC-005 states it; tasks.md T004 adds explicit diff check

---

## Test Requirements Quality

- [x] CHK011 Are happy-path test scenarios defined for `BeGreaterThanOrEqualTo` and `BeLessThan` for all three new types (`float`, `double`, `decimal`) individually, not just as a group? [Completeness, Spec §FR-004, FR-005, FR-010] — tasks.md T005/T006/T007 each cover one type
- [x] CHK012 Are failure-path test scenarios defined for range assertions for all three new types, with expected exception type and message content specified? [Completeness, Spec §FR-010] — tasks.md T008/T009/T010 each specify method names and assert message contains actual value and threshold
- [x] CHK013 Are dispatch type verification test scenarios specified — i.e., does the spec require asserting that `.Should()` on each new type returns `NumericAssertions<T>` rather than `ObjectAssertions<T>`? [Coverage, Spec §User Story 3, SC-002] — US3 scenarios 1–3; tasks.md T016
- [x] CHK014 Is the caller expression capture test scenario explicitly required — does the spec mandate a test confirming the caller variable name appears in the failure message? [Coverage, Spec §FR-008, FR-009] — tasks.md T011 covers all three types after remediation
- [x] CHK015 Are `Be` and `NotBe` test scenarios required for the new types, not only the range methods that were the reported gap? [Completeness, Spec §FR-006, User Story 2] — US2 acceptance scenarios; tasks.md T012–T015
- [x] CHK016 Are custom comparer (`IEqualityComparer<T>`, `IComparer<T>`) test scenarios for the new types either explicitly required or explicitly excluded from this feature? [Coverage, contracts/public-api.md] — explicitly excluded in Spec Assumptions: comparer logic is in unchanged `NumericAssertions<T>`; deferred
- [x] CHK017 Is the test coverage threshold (90% line coverage) stated as a verifiable gate rather than aspirational guidance? [Measurability, Spec §SC-004, constitution §IV.1] — SC-004 references constitution minimum; tasks.md T019
- [x] CHK018 Does the spec or plan require that test method naming follows the project convention (`MethodName_Scenario_ExpectedOutcome`), ensuring the new tests are discoverable alongside existing numeric tests? [Consistency, constitution §IV.2] — tasks.md Notes + all task descriptions use explicit names following the convention

---

## Acceptance Criteria Measurability

- [x] CHK019 Can SC-001 ("no compile errors for the three new types") be objectively confirmed by a reviewer without running the project locally? [Measurability, Spec §SC-001] — PR diff shows new overloads; CI build confirms compilation
- [x] CHK020 Can SC-002 ("100% of primitive numeric types resolve to `NumericAssertions<T>`") be verified from the PR diff alone, or does it require inspecting overload resolution at runtime? [Measurability, Spec §SC-002] — diff verifiable by C# overload resolution rules; dispatch tests (T016) provide runtime confirmation
- [x] CHK021 Is SC-003 (failure messages match the `int`/`long` format) stated with enough specificity that two reviewers would independently agree on pass/fail? [Measurability, Spec §SC-003] — contracts/public-api.md provides a concrete format example as reference
- [x] CHK022 Is SC-005 ("no changes to `NumericAssertions<T>`") verifiable by inspecting the PR diff — i.e., is the file path of `NumericAssertions.cs` named so the absence of changes is unambiguous? [Measurability, Spec §SC-005] — tasks.md T004 names `src/Assertivo/Numeric/NumericAssertions.cs` explicitly

---

## Scope, Edge Cases, and Exclusions

- [x] CHK023 Is the NaN and infinity out-of-scope decision documented with enough specificity that a reviewer knows no tests for those values are required — and would not flag their absence? [Clarity, Spec Clarifications 2026-06-10] — documented in both Clarifications and Edge Cases sections
- [x] CHK024 Is the nullable variant exclusion (`float?`, `double?`, `decimal?`) stated in a way that prevents scope creep during implementation or review? [Scope, Spec Assumptions] — stated in Assumptions, Edge Cases, and contracts/public-api.md Out of Scope
- [x] CHK025 Is the exclusion of other numeric types (`short`, `byte`, `uint`, etc.) precise enough that a reviewer would not request them as part of this PR? [Scope, Spec Clarifications 2026-06-10] — Clarifications + Assumptions + contracts/public-api.md Out of Scope all explicit
- [x] CHK026 Does the spec address whether `decimal.MaxValue` / `decimal.MinValue` boundary cases require tests, or is "standard comparison rules" sufficient without a test requirement? [Edge Case, Spec Edge Cases] — Edge Cases section: "follow standard comparison rules; no special handling required" — no test needed

---

## Dependencies and Assumptions

- [x] CHK027 Is the assumption that `NumericAssertions<T>`'s generic constraint already accepts `float`, `double`, and `decimal` stated as a verifiable fact (i.e., can a reviewer confirm it from the existing source without taking it on faith)? [Assumption, Spec Assumptions, research.md] — Spec Assumptions names the interfaces; research.md Decision 1 lists each type's interface implementations
- [x] CHK028 Is the C# 10+ / `[CallerArgumentExpression]` dependency acknowledged and tied to the project's existing use of the attribute in `int`/`long` overloads, rather than stated as a new requirement? [Dependency, Spec Assumptions] — Spec Assumptions explicitly ties it to existing int/long usage

---

## Traceability

- [x] CHK029 Does each functional requirement (FR-001 through FR-010) have at least one corresponding acceptance scenario in the spec's User Scenarios section? [Traceability, Spec §Requirements, §User Scenarios] — FR-001..003 → US3; FR-004..005 → US1; FR-006 → US2; FR-007 → US3 scenario 4; FR-008..009 → US1 scenario 3; FR-010 → all stories
- [x] CHK030 Are the overload signatures in `contracts/public-api.md` directly traceable to specific functional requirements in the spec, with no undocumented additions or omissions? [Traceability, contracts/public-api.md, Spec §FR-001..003] — each signature maps 1:1 to FR-001, FR-002, FR-003
- [x] CHK031 Are the quickstart validation scenarios traceable to specific spec success criteria, rather than being standalone examples with no requirement backing? [Traceability, quickstart.md, Spec §Success Criteria] — Scenario 1 → SC-001/SC-002; Scenario 2 → SC-004; Scenarios 3–4 → SC-003; Scenario 5 → FR-009; Scenario 6 → FR-006 (chaining guaranteed by return types); coverage run → SC-004

---

## Notes

- All 31 items evaluated 2026-06-11. Two items (CHK009, CHK016) initially failed; both resolved by adding assumption entries to spec.md before marking complete.
- Items are numbered sequentially for reference in PR comments
