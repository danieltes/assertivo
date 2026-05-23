# Design Quality Checklist: Should() Entry Point for Task Subjects

**Purpose**: Validate the completeness, clarity, consistency, and measurability of
requirements across spec + plan + research + contracts, from both author and PR-reviewer
perspectives. Includes regression requirement items (FR-011 / SC-002).
**Created**: 2026-05-17
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [research.md](../research.md) | [contracts/task-assertions.md](../contracts/task-assertions.md)
**Depth**: Thorough | **Audience**: Author self-review + PR reviewer

---

## Requirement Completeness

- [x] CHK001 - Are all 5 distinct `ThrowAsync` code paths (null subject, no exception thrown, `AggregateException` unwrap, type match, wrong type) each covered by at least one named FR? [Completeness, Spec §FR-001–FR-010]
- [x] CHK002 - Does FR-001 document why `Task?` (nullable) is used at the `Should()` entry point — explicitly establishing that null detection is intentionally deferred to `ThrowAsync`? [Completeness, Spec §FR-001]
- [x] CHK003 - Are the exact fields required in each failure message fully enumerated in the corresponding FR — specifically FR-004 (no exception), FR-005 (wrong type), and FR-010 (null subject) — rather than only in the contracts document? [Completeness, Spec §FR-004, §FR-005, §FR-010]
- [x] CHK004 - Is it explicitly specified that the `[CallerArgumentExpression]` label originates from the `Should()` call (not from `ThrowAsync`) — making SC-005 verifiable without reading the contracts? [Completeness, Spec §FR-001, §SC-005]
- [x] CHK005 - Is AOT-compatibility documented as a formal requirement in the spec, or does it appear only as an incidental gate in the plan's Constitution Check? [Completeness, Gap — visible in plan.md but absent from spec.md]
- [x] CHK006 - Is `Task<T>` coverage via inheritance stated as a formal acceptance scenario or requirement, rather than only as an informal assumption? [Completeness, Spec §Assumptions, Gap]

---

## Requirement Clarity

- [x] CHK007 - Is "already started" clarified to cover all four task states at `Should()` call time — pending (in-flight), faulted, cancelled, and ran-to-completion — rather than implying tasks must be completed? [Clarity, Spec §Edge Cases]
- [x] CHK008 - Is the `AggregateException` unwrapping condition stated precisely as "exactly one inner exception", and does the spec explicitly address the zero-inner case (`InnerExceptions.Count == 0`) — or is it only implied by the "exactly one" language? [Clarity, Spec §FR-003, §Edge Cases]
- [x] CHK009 - Does FR-003 explicitly state that after unwrapping, the inner exception is used for *both* type-matching AND for populating `.Which` — not just one of the two? [Clarity, Spec §FR-003]
- [x] CHK010 - Is the term "subtype" in FR-002 precisely defined to mean the C# is-a relationship, confirming that exact type match (`TException` itself) is also included? [Clarity, Spec §FR-002]
- [x] CHK011 - Does FR-005 specify whether the type name and `.Message` text in the wrong-type failure message reflect the *unwrapped* `target` exception or the original `caught` exception — especially for the path where `AggregateException` was unwrapped? [Clarity, Spec §FR-005, Research R-004]
- [x] CHK012 - Is the scope of FR-006 (`because`/`becauseArgs` threading) defined across all four named failure paths individually, or does its phrasing risk being interpreted as applying to only the primary failure case? [Clarity, Spec §FR-006]

---

## Requirement Consistency

- [x] CHK013 - Do the `AggregateException` unwrapping semantics in FR-003 include an explicit cross-reference to the existing `AsyncFunctionAssertions` behaviour (FR-020), preventing silent divergence during implementation? [Consistency, Spec §FR-003, §Assumptions]
- [x] CHK014 - Is the `ExceptionAssertions<TException>` return type stated in FR-007 consistent with the clarification entry (Session 2026-05-17, Q1-A), the contracts document, and the research code sketch (R-005) — do all four sources agree? [Consistency, Spec §FR-007, Clarifications, Research R-005]
- [x] CHK015 - Are the null-guard descriptions in FR-010 ("from `ThrowAsync`") and the edge-case section ("check is performed up front") reconciled with a single unambiguous explanation — do they refer to the same implementation point? [Consistency, Spec §FR-010, §Edge Cases]
- [x] CHK016 - Does the exact null-subject failure message text (`"task was null"`) appear consistently across FR-010, SC-004, and the contracts document — or is it defined in only one place? [Consistency, Spec §FR-010, §SC-004, Contracts]
- [x] CHK017 - Does FR-011 and the overload-resolution assumption in `§Assumptions` use the same definition of "conflict" / "interfere" — or could one be read as covering compile-time resolution while the other covers only runtime behaviour? [Consistency, Spec §FR-011, §Assumptions]

---

## Acceptance Criteria Quality

- [x] CHK018 - Can SC-001 ("every acceptance scenario covered by automated tests") be objectively verified — does it define what "covered" means precisely enough for a reviewer to confirm compliance? [Measurability, Spec §SC-001]
- [x] CHK019 - Does SC-003 ("migrate by removing the lambda wrapper alone") define the scope of "identical behaviour" precisely — does it explicitly include `.Which` chaining, `because` threading, and failure message format equivalence? [Measurability, Spec §SC-003]
- [x] CHK020 - Does SC-004 ("four failure conditions") correctly enumerate *distinct* code paths — is "AggregateException with multiple inners" a separate code path, or is it a sub-case of "wrong exception type" that should be labelled accordingly? [Clarity, Spec §SC-004]
- [x] CHK021 - Is SC-005 ("CallerArgumentExpression appears in every failure message") testable without implementation access — does the spec identify at least one concrete expected expression string for a specific test scenario? [Measurability, Spec §SC-005]

---

## Scenario Coverage

- [x] CHK022 - Is the `OperationCanceledException` / task-cancellation path covered by a formal acceptance scenario in a User Story, or only by an informal edge-case note? [Coverage, Spec §Edge Cases, Gap]
- [x] CHK023 - Does User Story 2 (`.Which` chaining) include an acceptance scenario that specifically covers the case where `.Which.Message` reflects the *inner* exception after `AggregateException` unwrapping — not only the direct-throw path? [Coverage, Spec §User Story 2, Scenario 2]
- [x] CHK024 - Is a dispatch-correctness scenario defined that verifies a `Task<TResult>` variable *declared as `Task<TResult>`* (not widened to `Task`) resolves to `TaskAssertions` rather than `ObjectAssertions<Task<TResult>>`? [Coverage, Gap, Spec §Assumptions]

---

## Edge Case Coverage

- [x] CHK025 - Does the spec define behaviour when `ThrowAsync` is called and the `Task` is still in-flight (not yet completed) — is it stated that `ThrowAsync` awaits it normally, or is this left implicit? [Edge Case, Gap, Spec §Edge Cases]
- [x] CHK026 - Is there a defined requirement for what happens when the task faults with an `AggregateException` containing *zero* inner exceptions — is this covered by the "exactly one" condition in FR-003 or is it an undocumented gap? [Edge Case, Gap, Spec §FR-003]
- [x] CHK027 - Does the spec clarify whether a *cancelled* task (`Task.IsCanceled == true`) throws `OperationCanceledException` or `TaskCanceledException` when awaited — and does the edge-case description account for `TaskCanceledException` as a subtype of `OperationCanceledException`? [Edge Case, Clarity, Spec §Edge Cases]
- [x] CHK028 - Is an edge case documented for a `Task<T>` subject — specifically confirming that the `T` return value is silently discarded and does not cause any assertion side-effects? [Edge Case, Spec §Assumptions, Gap]

---

## Non-Functional Requirements

- [x] CHK029 - Are performance requirements for `ThrowAsync` explicitly documented in the spec, or are they only implicitly inherited from the constitution's 100 ms per-test budget? [NFR, Gap]
- [x] CHK030 - Is thread-safety (concurrent `ThrowAsync` calls from parallel test runners) addressed in the spec — even if only by reference to the constitution's immutability principle? [NFR, Gap]

---

## Regression Requirements

- [x] CHK031 - Does FR-011 ("MUST NOT interfere") specify a concrete, verifiable criterion — for example, that the *compile-time* type resolution of `Func<Task>.Should()` is unchanged — rather than only stating a general non-interference goal? [Completeness, Spec §FR-011]
- [x] CHK032 - Does SC-002 ("zero regressions") identify the specific test files or test classes that constitute the regression suite, making the criterion verifiable from the plan's Project Structure? [Measurability, Spec §SC-002, Plan §Project Structure]
