# Peer Review Checklist: NotBe Inequality Assertion

**Purpose**: Validate requirements quality across all 5 planning artifacts before implementation begins — peer/PR review depth  
**Created**: 2026-05-31  
**Feature**: [spec.md](../spec.md) | [contracts/api.md](../contracts/api.md) | [data-model.md](../data-model.md) | [research.md](../research.md) | [quickstart.md](../quickstart.md)  
**Scope**: All 5 artifacts — spec, research, data-model, contracts, quickstart  
**Audience**: Peer reviewer (PR gate)  
**Risk emphasis**: Cross-type consistency · Non-functional requirements · Test scenario completeness

---

## Requirement Completeness

- [X] CHK001 — Are the three `NotBe` method signatures fully specified — with all optional parameters, their types, and default values — for all three assertion types? [Completeness, Spec §FR-001–FR-003]
- [X] CHK002 — Are the return types for all three `NotBe` methods explicitly stated with the specific generic argument (`AndConstraint<ObjectAssertions<T>>`, `AndConstraint<NumericAssertions<T>>`, `AndConstraint<StringAssertions>`)? [Completeness, contracts/api.md]
- [X] CHK003 — Is the `because`/`becauseArgs` contract fully specified — including what happens when `because` is empty string and `becauseArgs` is non-empty? [Completeness, Spec §FR-007, Edge Cases]
- [X] CHK004 — Is the null-handling behavior of `StringAssertions.NotBe` documented for all four subject × unexpected combinations: null×null, null×non-null, non-null×null, and non-null×non-null? [Completeness, data-model.md, Spec §US3]
- [X] CHK005 — Are the `.Should()` entry points that lead to each assertion type specified, so a reviewer can confirm which overload is reached from each subject type? [Completeness, contracts/api.md]

---

## Requirement Clarity

- [X] CHK006 — Is "prefixed with `not`" in FR-006 specific enough to derive a precise failure message string without consulting implementation code, or does it require a concrete example to eliminate ambiguity? [Clarity, Spec §FR-006]
- [X] CHK007 — Is "ordinal (case-sensitive) string comparison" in FR-009 referenced to a specific BCL enum value (`StringComparison.Ordinal`) rather than described only qualitatively? [Clarity, Spec §FR-009]
- [X] CHK008 — Is the `EqualityComparer<T>.Default` fallback in FR-008 precise enough that an implementer knows exactly at which point in the call the fallback is resolved? [Clarity, Spec §FR-008]
- [X] CHK009 — Does SC-004 ("human-readable and identifies the unexpected value and the actual found value") define what "identifies" means precisely enough to objectively pass or fail a given message string? [Clarity, Spec §SC-004]

---

## Cross-Type Consistency

- [X] CHK010 — Are the `because`/`becauseArgs` parameter names, types, defaults, and formatting semantics specified identically across all three `NotBe` overloads? [Consistency, contracts/api.md, Spec §FR-007]
- [X] CHK011 — Is the failure message template (`Expected not {unexpected} but found {actual}.`) stated consistently across all four artifacts that mention it — spec (FR-006), contracts, data-model, and quickstart? [Consistency, Spec §FR-006, contracts/api.md, data-model.md, quickstart.md]
- [X] CHK012 — Is the absence of an `IEqualityComparer<string>?` parameter on `StringAssertions.NotBe` — while it exists on the other two types — explicitly documented and justified? [Consistency, Spec §Assumptions, contracts/api.md]
- [X] CHK013 — Is the null-handling specification for reference-type `ObjectAssertions<T>.NotBe` as detailed as the specification for `StringAssertions.NotBe` (i.e., does it cover the same four subject × unexpected combinations)? [Consistency, Spec §US3, data-model.md, Gap]
- [X] CHK014 — Is the `comparer: null` fallback to `EqualityComparer<T>.Default` specified with equal precision for both `ObjectAssertions<T>` and `NumericAssertions<T>`, or only for one? [Consistency, Spec §FR-008, contracts/api.md]

---

## Non-Functional Requirements

- [X] CHK015 — Is the `[StackTraceHidden]` attribute requirement explicitly stated in the spec or data-model for all three new methods, or is it only implied by structural similarity to `Be`? [Non-Functional, data-model.md §Invariants]
- [X] CHK016 — Is the zero-allocation requirement for the passing path explicitly traceable from a spec statement or assumption to a measurable claim — or is it only present in the plan and research? [Non-Functional, plan.md §Constitution Check, research.md §R-5, Gap]
- [X] CHK017 — Are XML `<summary>` documentation requirements for all three new public methods stated as explicit acceptance criteria, rather than left implicit as an implementation convention? [Non-Functional, Spec §Assumptions]
- [X] CHK018 — Is AOT-compatibility of `EqualityComparer<T>.Default` documented as a verified constraint in a discoverable artifact (research or plan), rather than assumed without evidence? [Non-Functional, research.md §R-4, plan.md §Constitution Check]

---

## Test Scenario Coverage

- [X] CHK019 — Are acceptance scenarios for User Story 1 defined for `NumericAssertions<T>` as a type, or do all US1 scenarios reference only `int` and `string` as concrete subjects? [Coverage, Spec §US1, Gap]
- [X] CHK020 — Is a passing `NotBe` scenario defined specifically for `long` subjects, given that `NumericAssertions<long>` is a distinct supported entry point? [Coverage, Spec §US1, contracts/api.md §Entry Points, Gap]
- [X] CHK021 — Are both directions of custom comparer override covered in US2 acceptance scenarios: "values unequal by default but equal by comparer" AND "values equal by default but unequal by comparer"? [Coverage, Spec §US2]
- [X] CHK022 — Is a test scenario defined for `because` with format placeholders and non-empty `becauseArgs` (e.g., `"expected {0} to differ"` with args), or does the spec only cover the simple no-args case? [Coverage, Spec §FR-007, Gap]
- [X] CHK023 — Is the chaining scenario (US1 scenario 6) defined with a concrete chained assertion method rather than described generically as "allows further assertions"? [Coverage, Spec §US1 scenario 6]

---

## Edge Case Coverage

- [X] CHK024 — Is the behavior when a custom comparer throws an exception specified in terms of expected observable outcome (exception type and propagation path), or only described qualitatively as "propagates naturally"? [Edge Cases, Spec §Edge Cases]
- [X] CHK025 — Is the "empty string vs. null" distinction for `StringAssertions.NotBe` — i.e., `NotBe("")` on a null subject — addressed in the spec or data-model's null-handling coverage? [Edge Cases, data-model.md, Gap]

---

## Inter-Artifact Consistency

- [X] CHK026 — Does the failure message example in `quickstart.md` (`Expected not 0 but found 0.`) exactly match the template in `contracts/api.md` (`Expected not {unexpected} but found {actual}.`) in structure and field order? [Consistency, quickstart.md, contracts/api.md]
- [X] CHK027 — Does the null-handling table in `data-model.md` for `StringAssertions` cover all four combinations described in US3's acceptance scenarios, with no row missing? [Consistency, data-model.md, Spec §US3]
- [X] CHK028 — Are the method signatures in `contracts/api.md` and `data-model.md` identical in parameter names, types, defaults, and order — with no silent divergence between the two artifacts? [Consistency, contracts/api.md, data-model.md]

---

## Acceptance Criteria Quality

- [X] CHK029 — Are SC-001 ("100% of equal cases produce a failing assertion") and SC-002 ("100% pass rate for unequal cases") specific enough to be verified independently without subjective judgment? [Measurability, Spec §SC-001–SC-002]
- [X] CHK030 — Does SC-005 ("100% of comparer-influenced outcomes match the comparer's semantics") specify how the comparer's semantics are independently determined, to avoid a circular definition where any implementation vacuously satisfies the criterion? [Measurability, Spec §SC-005]

---

## Notes

- Items marked `[Gap]` indicate potential missing requirements — the reviewer should decide whether to add coverage or explicitly mark as out-of-scope.
- Items marked `[Consistency]` require cross-referencing two or more artifacts side by side.
- Items marked `[Non-Functional]` correspond to constitution-required constraints; failure = merge blocker under project governance.
- Mark items `[x]` as checked; add inline comments for any findings.
