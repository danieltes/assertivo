# API Checklist: Add BeGreaterThan and BeLessThanOrEqualTo to NumericAssertions

**Purpose**: PR reviewer gate — validates that requirements are precise, complete, and consistent enough to approve and merge this change
**Created**: 2026-06-15
**Feature**: [spec.md](../spec.md) · [contracts/public-api.md](../contracts/public-api.md)

## API Contract Requirements Quality

- [x] CHK001 - Are the parameter signatures (type, nullability, default values, and order) for both new methods fully specified? [Completeness, Spec §FR-001, FR-004]
- [x] CHK002 - Is the return type (`AndConstraint<NumericAssertions<T>>`) specified for both new methods, consistent with the existing `BeGreaterThanOrEqualTo` / `BeLessThan` pair? [Consistency, Spec §FR-011]
- [x] CHK003 - Is optional `IComparer<T>` injection specified for **both** new methods, not just one? [Consistency, Spec §FR-001, FR-004]
- [x] CHK004 - Are the `because` and `becauseArgs` parameters specified for both methods, consistent with the existing pair? [Completeness, Spec §FR-010]
- [x] CHK005 - Is the strict vs. inclusive semantics distinction between the four methods made explicit? Specifically: does the spec state that `BeGreaterThan` fails when subject equals the threshold (strict `>`), while `BeLessThanOrEqualTo` passes when subject equals the threshold (inclusive `<=`)? [Clarity, Spec §FR-002, FR-006, Edge Cases]
- [x] CHK006 - Are the generic constraint requirements documented — confirming that no constraints beyond the class-level `where T : struct, IComparable<T>, IEquatable<T>` are needed? [Completeness, Spec Assumptions]

## Failure Semantics & Message Requirements Quality

- [x] CHK007 - Is the exact failure message format specified for `BeGreaterThan`? (`"a value greater than {threshold}"`) [Clarity, Spec §FR-008]
- [x] CHK008 - Is the exact failure message format specified for `BeLessThanOrEqualTo`? (`"a value less than or equal to {threshold}"`) [Clarity, Spec §FR-008]
- [x] CHK009 - Are the failure message formats for the two new methods consistent with the established format used by `BeGreaterThanOrEqualTo` and `BeLessThan`? [Consistency, Spec §FR-008]
- [x] CHK010 - Is the requirement to include the actual subject value in failure messages specified for both methods? [Completeness, Spec §FR-009]
- [x] CHK011 - Is the caller expression inclusion in failure messages specified as a requirement? [Completeness, Spec §SC-002]
- [x] CHK012 - Is the uniqueness of failure messages across all four comparison methods addressed? Could `BeGreaterThan` and `BeLessThanOrEqualTo` produce messages that are indistinguishable from each other or from the existing pair in any failure scenario? [Clarity, Spec §SC-002]
- [x] CHK013 - Is the null-comparer fallback behavior specified — confirming that passing `null` as the comparer falls back to `Comparer<T>.Default` without throwing? [Completeness, Spec §FR-007, Edge Cases]

## Test Scenario Requirements Quality

- [x] CHK014 - Does the spec define the three distinct test scenarios for `BeGreaterThan`: subject strictly greater (pass), subject equal to threshold (fail), subject strictly less (fail)? [Completeness, Spec User Story 1, AC #1–3]
- [x] CHK015 - Does the spec define the two distinct passing scenarios for `BeLessThanOrEqualTo`: subject strictly less than threshold, and subject equal to threshold? [Completeness, Spec User Story 2, AC #1–2]
- [x] CHK016 - Does the spec define the failing scenario for `BeLessThanOrEqualTo` where subject strictly exceeds the threshold? [Completeness, Spec User Story 2, AC #3]
- [x] CHK017 - Is a custom comparer scenario specified for `BeGreaterThan`, with a concrete example of how the custom comparer inverts the expected result? [Coverage, Spec User Story 1, AC #4]
- [x] CHK018 - Is a custom comparer scenario specified for `BeLessThanOrEqualTo`? [Coverage, Spec User Story 3]
- [x] CHK019 - Are `because` phrase scenarios specified for both methods, with assertions on the failure message content? [Coverage, Spec User Story 1 AC #5, User Story 2 AC #4]
- [x] CHK020 - Are the acceptance scenarios for `BeGreaterThan` and `BeLessThanOrEqualTo` specific enough to derive exact test assertions — i.e., do they state which values appear in the failure message? [Clarity, Spec User Story 1 AC #2, User Story 2 AC #3]
- [x] CHK021 - Is the `AndConstraint` chaining requirement covered by at least one acceptance scenario that chains through a new method (e.g., `.BeGreaterThan(x).And.BeLessThan(y)`)? [Coverage, Spec §SC-004, FR-011]
- [x] CHK022 - Are the acceptance criteria for both methods testable independently from each other — could a reviewer verify `BeGreaterThan` is correct without implementing `BeLessThanOrEqualTo`? [Completeness, Spec User Story 1–2]

## Targeted Constitutional Compliance

- [x] CHK023 - Is the XML documentation comment requirement for both new public methods explicitly stated in the spec or plan? [Completeness, Plan Constitution Check]
- [x] CHK024 - Is the `[StackTraceHidden]` decoration requirement explicitly captured as a functional requirement, not just an implementation note? [Completeness, Spec §FR-012]

## Dependencies & Assumptions Quality

- [x] CHK025 - Is the assumption that no new source files or structural changes are required documented and clearly scoped? [Completeness, Spec Assumptions]
- [x] CHK026 - Is the assumption that existing `MessageFormatter` infrastructure is sufficient documented and substantiated (not just asserted)? [Completeness, Spec Assumptions]
- [x] CHK027 - Is the out-of-scope declaration for negation variants (`Not.BeGreaterThan`, `Not.BeLessThanOrEqualTo`) explicit and traceable to the constitution's incremental negation allowance? [Completeness, Spec Assumptions]
- [x] CHK028 - Are the coverage threshold constraints (93% line / 90% branch) reflected in the acceptance criteria, and is the Coverlet always-throw instrumentation caveat acknowledged for `MessageFormatter.Fail` call sites? [Completeness, Plan §Technical Context]

## Notes

- Check items off as completed: `[x]`
- A failing item means the spec/plan needs updating before the PR is approved, not that the implementation is wrong
- Items marked `[Gap]` identify requirements that may be missing entirely — verify whether they are intentionally out of scope
- All 28 items require a requirements-level answer; if the answer is "that's handled in the implementation," the spec needs a corresponding requirement added
