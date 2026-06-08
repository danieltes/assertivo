# PR Review Checklist: NotBeEmpty Assertion

**Purpose**: PR reviewer gate — validates API design requirements quality and test specification completeness for both `StringAssertions.NotBeEmpty` and `GenericCollectionAssertions<T>.NotBeEmpty`
**Created**: 2026-06-07
**Reviewed**: 2026-06-07
**Feature**: [spec.md](../spec.md) | [contracts/public-api.md](../contracts/public-api.md) | [research.md](../research.md)
**Audience**: PR reviewer
**Scope**: API design requirements + test specification requirements

---

## API Requirements Completeness

- [x] CHK001 - Are return types specified for both the string and collection variants of `NotBeEmpty`? [Completeness, Contracts §Chaining Contract]
- [x] CHK002 - Are the `because` and `becauseArgs` parameters documented for both method variants? [Completeness, Spec §FR-008]
- [x] CHK003 - Is the `[StackTraceHidden]` decoration requirement captured for both methods, not just one? [Completeness, Spec §FR-009]
- [x] CHK004 - Are null-subject behaviors defined explicitly for BOTH variants (string and collection), not inferred from the other? [Completeness, Spec §FR-002, §FR-005]
- [x] CHK005 - Is the requirement to update the public API contract documentation (FR-010) specific enough to identify which artifacts must change? [Completeness, Contracts §Documentation Update Scope]
- [x] CHK006 - Is the chaining behavior requirement (`AndConstraint<T>`) stated for both methods independently? [Completeness, Spec §FR-001, §FR-004]
- [x] CHK007 - Are XML documentation requirements included in the documentation update scope for both new method implementations? [Completeness, Contracts §Documentation Update Scope]

## API Requirements Clarity

- [x] CHK008 - Is "not empty" for strings defined unambiguously as "not `""`" — excluding null and whitespace-only strings from the failure condition? [Clarity, Spec §FR-002, §Edge Cases]
- [x] CHK009 - Is "not empty" for collections defined unambiguously as having at least one element (>0) — excluding null subjects from the pass condition? [Clarity, Spec §FR-006]
- [x] CHK010 - Is the boundary between `NotBeEmpty` and `NotBeNullOrEmpty` explicitly documented so that the distinction is reviewable without consulting the implementation? [Clarity, Spec §Assumptions]
- [x] CHK011 - Is the exact `actual` value in the string failure message specified precisely (`"\"\""` via `FormatValue`, not a plain description)? [Clarity, Contracts §1 Failure Message Contract]
- [x] CHK012 - Is the null-guard contract for collections described precisely enough to distinguish it from a value-equality failure? [Clarity, Contracts §Null-Guard Contract]
- [x] CHK013 - Is the enumeration strategy (single-pass `Any()`, not `Count()`) captured as a behavioral requirement rather than a private implementation detail? [Clarity, Research §Decision 6, Contracts §Enumeration Contract]
- [x] CHK014 - Is the asymmetric null behavior (string null passes; collection null fails) explained and justified in accessible language, not just stated as a fact? [Clarity, Research §Decision 1, §Decision 2]

## API Requirements Consistency

- [x] CHK015 - Does the null-string behavior align with the established `NotContain` precedent (null subject passes for negation assertions)? [Consistency, Research §Decision 1]
- [x] CHK016 - Does the collection null-guard contract use the same expected/actual pair (`"a collection"` / `"<null>"`) as all other collection assertions? [Consistency, Contracts §Null-Guard Contract]
- [x] CHK017 - Are `because`/`becauseArgs` parameter names and semantics identical to those on existing methods (`BeEmpty`, `NotContain`, etc.)? [Consistency, Contracts §Because Formatting Contract]
- [x] CHK018 - Is the `AndConstraint<T>` return type consistent with the return type of all other assertion methods that do not narrow the subject? [Consistency, Contracts §Chaining Contract]

## Failure Message Contract Quality

- [x] CHK019 - Are the expected and actual strings for the string failure case machine-checkable (i.e., can a test assert `ex.Expected == "a non-empty string"` deterministically)? [Measurability, Contracts §Machine-Checkable Diagnostic Requirements]
- [x] CHK020 - Are the expected and actual strings for the collection failure case machine-checkable (`ex.Expected == "a non-empty collection"`, `ex.Actual == "an empty collection"`)? [Measurability, Contracts §Machine-Checkable Diagnostic Requirements]
- [x] CHK021 - Is the because-whitespace edge case (whitespace-only reason → no `Because:` line) specified for both methods, not just one? [Completeness, Contracts §Because Formatting Contract] — Note: corrected by analysis (F1 remediation); whitespace-only reasons ARE included (`Because:    `) per `IsNullOrEmpty` behavior; contract updated to reflect this; applies equally to both methods
- [x] CHK022 - Is the `Expression:` line behavior (present when caller metadata available; absent otherwise) specified with enough precision to be testable? [Clarity, Contracts §Machine-Checkable Diagnostic Requirements]

## Test Scenario Requirements Completeness

- [x] CHK023 - Is at least one positive (assertion passes) scenario defined in the spec for the string variant? [Completeness, Spec §User Story 1 Scenario 1]
- [x] CHK024 - Is at least one positive scenario defined for the collection variant? [Completeness, Spec §User Story 2 Scenario 1]
- [x] CHK025 - Is at least one negative (assertion fails) scenario for the string variant specific enough to verify `ex.Expected` and `ex.Actual`? [Completeness, Spec §User Story 1 Scenario 2]
- [x] CHK026 - Is at least one negative scenario for the collection variant specific enough to verify `ex.Expected` and `ex.Actual`? [Completeness, Spec §User Story 2 Scenario 2]
- [x] CHK027 - Is the `because`-on-failure scenario defined for both string and collection variants as a distinct, independently testable scenario? [Completeness, Spec §User Story 1 Scenario 3, §User Story 2 Scenario 3]
- [x] CHK028 - Is the null-string scenario (should pass) independently testable without requiring the non-null scenarios? [Completeness, Spec §User Story 1 Scenario 4]
- [x] CHK029 - Is the null-collection scenario (should fail via guard) independently testable and distinguished from an empty-collection failure? [Completeness, Spec §User Story 2 Scenario 4]
- [x] CHK030 - Is a chaining scenario (`.NotBeEmpty().And.X()`) required for both variants in the test specification? [Completeness, Spec §SC-006, Contracts §Chaining Contract]

## Test Scenario Coverage

- [x] CHK031 - Are whitespace-only string scenarios addressed in the test requirements to confirm they pass (whitespace ≠ empty)? [Coverage, Spec §Edge Cases]
- [x] CHK032 - Are lazy-sequence (non-`List<T>`) collection scenarios required to confirm the assertion works beyond concrete list types? [Coverage, Spec §Assumptions, §Edge Cases]
- [x] CHK033 - Is the because-whitespace edge case (whitespace-only reason → no `Because:` line) specified for both methods, not just one? [Coverage, Contracts §Because Formatting Contract] — Note: after F1 remediation, behavior is: whitespace reasons ARE emitted (no suppression); contract is now accurate; no test task needed to verify absence of Because: line since the behavior is inclusion, not suppression
- [x] CHK034 - Are all 4 string acceptance scenarios (Spec §User Story 1) traceable to distinct test requirements? [Traceability, Spec §SC-003]
- [x] CHK035 - Are all 4 collection acceptance scenarios (Spec §User Story 2) traceable to distinct test requirements? [Traceability, Spec §SC-003]
- [x] CHK036 - Is regression coverage for existing `BeEmpty` tests addressed — i.e., does the spec or plan state that no existing tests must be modified? [Coverage, Spec §SC-001]

## Non-Functional Requirements

- [x] CHK037 - Is the zero-allocation happy-path requirement for both methods aligned with constitution §VI.2, and is it stated as a verifiable requirement (not just an aspiration)? [Completeness, Constitution §VI.2] — Covered by constitution §VI.2 mandate ("Simple value assertions MUST be zero-allocation") and plan §Technical Context ("Zero-allocation happy path"); existing BenchmarkDotNet suite covers this per constitution §VI.1
- [x] CHK038 - Is the 100 ms per-test ceiling traceable to a requirement in the spec (SC-003) and to the constitution §IV.2 testing standard? [Traceability, Spec §SC-003, Constitution §IV.2]
- [x] CHK039 - Is the `[StackTraceHidden]` requirement expressed in terms of observable outcome ("test call site is the first visible frame") rather than only as an implementation instruction? [Clarity, Spec §FR-009]
- [x] CHK040 - Is the file-length constraint (new partial file for collection variant) documented as a requirement traceable to a constitution gate, not left as an informal note? [Completeness, Plan §Constitution Check, Constitution §III.2]

## Assumptions & Traceability

- [x] CHK041 - Is the documented assumption that null strings pass `NotBeEmpty` stated in a way that distinguishes it from unintentional omission? [Clarity, Spec §Assumptions]
- [x] CHK042 - Is the assumption that all `IEnumerable<T>` types are supported (not just `List<T>`) elevated to a functional requirement, or is it explicitly documented as a scoping assumption? [Ambiguity, Spec §Assumptions]
- [x] CHK043 - Are all 10 functional requirements (FR-001 through FR-010) traceable to at least one row in the public API contract traceability matrix? [Traceability, Contracts §Traceability Matrix]
- [x] CHK044 - Are all 6 success criteria (SC-001 through SC-006) verifiable without access to implementation source code? [Measurability, Spec §Success Criteria]
- [x] CHK045 - Is the partial-file decision (Research §Decision 5) reflected as a constraint in the plan's Source Code structure, not just in research notes? [Completeness, Plan §Project Structure]

## Notes

- Mark items `[x]` as they pass review
- Add inline findings when an item fails: `[x]` → `[ ] ⚠ <finding>`
- Items marked `[Gap]` or `[Ambiguity]` require spec/plan updates before merge approval
- All 45 items must pass (or be explicitly waived with justification) for PR gate to clear

## Review Summary (2026-06-07)

**Result**: ✓ PASS — 45/45 items pass

**Notable findings addressed during review**:
- CHK021/CHK033: because-whitespace behavior was corrected (F1 from `/speckit-analyze`); contract now accurately reflects `IsNullOrEmpty` semantics
- CHK022: Expression-line testability addressed by adding T022/T023 to tasks.md
- CHK037: Zero-allocation requirement covered transitively via constitution §VI.2 + plan; no new benchmark task required
