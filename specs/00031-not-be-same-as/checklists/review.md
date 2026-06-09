# Full Review Gate Checklist: NotBeSameAs

**Purpose**: Dual-purpose gate — author pre-implementation self-review and peer PR reviewer validation. Covers API contract precision, test coverage adequacy, and BeSameAs/NotBeSameAs symmetry.
**Created**: 2026-06-08
**Feature**: [spec.md](../spec.md) | [contracts/public-api.md](../contracts/public-api.md) | [quickstart.md](../quickstart.md)

---

## API Contract Precision

- [x] CHK001 - Is the method signature fully specified including parameter types, defaults, and return type? [Completeness, Spec §FR-001]
- [x] CHK002 - Is the parameter name `unexpected` (rather than `expected`) documented and its semantic distinction from `BeSameAs`'s `expected` explained? [Clarity, Spec §FR-001]
- [x] CHK003 - Is the value-type guard's exact exception message specified verbatim, including the `{typeof(T).Name}` interpolation? [Clarity, Spec §FR-004]
- [x] CHK004 - Is the guard ordering (value-type check runs before `ReferenceEquals`) explicitly specified? [Completeness, Spec §FR-004]
- [x] CHK005 - Are both failure message components (`Expected` = `"not the same reference"`, `Actual` = `"same reference"`) specified precisely and unambiguously? [Clarity, Spec §FR-003]
- [x] CHK006 - Is the `because`/`becauseArgs` formatting contract documented for all three input variants: empty string, plain reason, and format-string-with-args? [Completeness, Spec §FR-005]
- [x] CHK007 - Is the fluent chaining contract specified — that the return type enables `.And` continuation? [Completeness, contracts/public-api.md §Chaining Contract]
- [x] CHK008 - Is the `[StackTraceHidden]` attribute requirement called out? [Completeness, Gap]
- [x] CHK009 - Is the deliberate absence of a comparer parameter justified and documented? [Clarity, research.md §Decision 5]
- [x] CHK010 - Does the contract specify that `ReferenceEquals` is used and document why alternative comparison mechanisms are excluded? [Clarity, research.md §Decision 1]

---

## Test Coverage Adequacy

- [x] CHK011 - Is `NotBeSameAs_WithDifferentReferences_Passes` specified with a concrete two-distinct-heap-objects scenario? [Completeness, Spec §AC-1]
- [x] CHK012 - Is `NotBeSameAs_WithSameReference_Fails` specified to assert all three diagnostic properties: exception type, `Expected`, and `Actual`? [Completeness, Spec §AC-2]
- [x] CHK013 - Is `NotBeSameAs_WithValueType_ThrowsInvalidOperationException` specified to assert the exact guard message text, not just exception type? [Completeness, Spec §AC-3]
- [x] CHK014 - Is `NotBeSameAs_WithNullSubjectAndNullUnexpected_Fails` specified with its null/null `ReferenceEquals` semantics documented explicitly? [Clarity, Spec §AC-4]
- [x] CHK015 - Is `NotBeSameAs_WithNullSubjectAndNonNullUnexpected_Passes` specified to confirm the passing path returns a valid `AndConstraint` (not just that no exception is thrown)? [Completeness, Spec §AC-5]
- [x] CHK016 - Is the `because` reason test specified to assert both the `Reason` property value and the full `Message` string containment? [Completeness, contracts/public-api.md §Machine-Checkable Diagnostic Requirements #4–5]
- [x] CHK017 - Is fluent chaining (`.And.NotBeNull()`) covered in the acceptance criteria or quickstart as a validation scenario? [Coverage, quickstart.md §Fluent Chaining]
- [x] CHK018 - Are all 6 acceptance scenario names directly traceable to the spec's Acceptance Criteria section? [Traceability, Spec §Acceptance Criteria]
- [x] CHK019 - Are all 6 acceptance scenarios individually testable without requiring shared state between test methods? [Measurability, Gap]
- [x] CHK020 - Does the spec reference the project's coverage thresholds (line ≥93%, branch ≥90%) that the new tests must maintain? [Completeness, Spec §SC-004]

---

## BeSameAs / NotBeSameAs Symmetry

- [x] CHK021 - Does the spec explicitly state the guard message differs from `BeSameAs` only in method name (no other textual difference)? [Consistency, Spec §FR-004]
- [x] CHK022 - Is the inversion of failure message values between the two methods documented? (`BeSameAs` uses `"same reference"/"different reference"`; `NotBeSameAs` uses `"not the same reference"/"same reference"`) [Consistency, research.md §Decision 2]
- [x] CHK023 - Does the spec confirm that `BeSameAs` is not modified as part of this change? [Consistency, contracts/public-api.md §Compatibility]
- [x] CHK024 - Is the parameter naming convention difference (`expected` in `BeSameAs` vs. `unexpected` in `NotBeSameAs`) consistently applied across all spec sections? [Consistency, Spec §FR-001]
- [x] CHK025 - Does the spec confirm both methods place the value-type guard before any comparison, with identical guard semantics? [Consistency, research.md §Decision 4]
- [x] CHK026 - Are the return types of both methods confirmed identical (`AndConstraint<ObjectAssertions<T>>`)? [Consistency, research.md §Decision 6]
- [x] CHK027 - Does the global public-api.md list `NotBeSameAs` immediately after `BeSameAs` to communicate the complementary pair visually? [Clarity, specs/00001-assertion-library-core/contracts/public-api.md §ObjectAssertions\<T\>]
- [x] CHK028 - Is the negation-pair relationship (completing `BeSameAs`/`NotBeSameAs` per constitution §VII.2) explicitly referenced in the contracts? [Traceability, contracts/public-api.md §Compatibility]

---

## Requirement Completeness

- [x] CHK029 - Are all 7 functional requirements (FR-001 through FR-007) present and individually non-empty with distinct testable outcomes? [Completeness, Spec §FR]
- [x] CHK030 - Are all 4 success criteria measurable and each mapped to at least one functional requirement? [Completeness, Spec §SC]
- [x] CHK031 - Is the Documentation Update Scope (4 required artifacts) completely enumerated: feature contract, global public-api.md, quickstart, XML doc comments? [Completeness, contracts/public-api.md §Documentation Update Scope]
- [x] CHK032 - Are the spec's assumptions validated against known .NET 10 behavior (e.g., boxing semantics for `typeof(T).IsValueType`, `ReferenceEquals` null-safety guarantee)? [Completeness, Spec §Assumptions]

---

## Scenario and Edge Case Coverage

- [x] CHK033 - Are all null-semantics scenarios individually specified: null/null (fails), null/non-null (passes), non-null/null (passes)? [Coverage, Spec §FR-006, §FR-007]
- [x] CHK034 - Is the boxing behavior addressed — that a value-type subject would be boxed when passed as `object?`, making the guard necessary to prevent a silently-passing but meaningless assertion? [Coverage, research.md §Decision 4]
- [x] CHK035 - Are recovery and rollback scenarios explicitly identified as out of scope (no state mutation occurs in a pure assertion method)? [Coverage, Gap — intentional exclusion]
- [x] CHK036 - Is the behavior when `becauseArgs` is an explicitly empty array (vs. omitted) specified as equivalent to the default? [Edge Case, Spec §FR-005]
- [x] CHK037 - Are concurrency and thread-safety requirements explicitly noted as not applicable (inherent in `readonly struct` semantics)? [Coverage, Gap — intentional]

---

## Non-Functional Requirements

- [x] CHK038 - Is AOT compatibility of the new method addressed — confirming no reflection beyond `typeof(T).IsValueType` is used? [Completeness, plan.md §Constitution Check]
- [x] CHK039 - Is `Nullable` annotation correctness specified — `object?` for the `unexpected` parameter allowing explicit null? [Completeness, Spec §FR-001]
- [x] CHK040 - Is `TreatWarningsAsErrors` compliance addressed — confirming no new nullable or other warnings are introduced? [Completeness, plan.md §Constitution Check]
- [x] CHK041 - Is the XML documentation requirement specified with all four required tags: `<summary>`, `<param name="unexpected">`, `<param name="because">`, `<param name="becauseArgs">`, `<returns>`, `<exception cref="InvalidOperationException">`? [Completeness, contracts/public-api.md §XML Docs]

---

## Notes

- Check items off as completed: `[x]`
- For dual-purpose use: authors should complete this before first implementation commit; reviewers should re-verify any unchecked items at PR review time.
- Items marked `[Gap]` indicate checks for the *absence* of a requirement — verify that the omission is intentional and documented.
- Reference tags: `[Completeness]` = requirement is present; `[Clarity]` = requirement is unambiguous; `[Consistency]` = no contradictions; `[Coverage]` = all cases addressed; `[Traceability]` = source linkable; `[Measurability]` = objectively verifiable.
