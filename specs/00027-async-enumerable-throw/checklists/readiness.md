# Pre-Tasks Readiness Checklist: ThrowAsync for IAsyncEnumerable<T>

**Purpose**: Validate that the full planning artifact set (spec, plan, research, data-model, contracts, quickstart) is implementation-ready before running `/speckit.tasks`. Tests requirements quality — not implementation behaviour.
**Created**: 2026-05-27
**Feature**: [spec.md](../spec.md)
**Audience**: Author (pre-tasks gate)
**Depth**: Comprehensive — all artifact layers

---

## DisposeAsync Handling — MANDATORY GATE

> These items gate forward progress. Resolve all before running `/speckit.tasks`.

- [X] CHK001 - Does FR-003 conflict with research decision R-001? FR-003 mandates "drain the enumerable using `await foreach`" but R-001 mandates the **manual enumerator pattern** instead. Is this a contradiction in the planning artifacts that must be corrected before implementation? **[Conflict, Spec §FR-003, Research §R-001]**
- [X] CHK002 - Is the `DisposeAsync`-exception-preservation behaviour specified with enough precision to derive exactly one correct implementation without consulting research? Can an implementer derive the required behaviour from the spec alone? **[Clarity, Spec §Edge Cases, Research §R-001]**
- [X] CHK003 - Does the spec define what happens when `DisposeAsync` throws and there was **no** prior enumeration exception — i.e., should the disposal exception propagate normally in that case? **[Completeness, Spec §Edge Cases]**
- [X] CHK004 - Is the `catch when (caught is not null)` exception-filter pattern mandated by a spec requirement, or is it only a research implementation detail? Could a conforming implementation use a different pattern and still satisfy the spec? **[Clarity, Research §R-001]**
- [X] CHK005 - Are acceptance tests for the `DisposeAsync`-exception-discard path (post-enumeration-exception) explicitly required in SC-001, or are they only implied by the edge cases section? **[Coverage, Spec §SC-001, §Edge Cases]**

---

## Requirement Completeness

- [X] CHK006 - Do FR-001 through FR-012 collectively cover every acceptance scenario in User Stories 1–3 with no scenario left unaddressed by at least one FR? **[Completeness, Spec §FR-001–FR-012, §User Stories]**
- [X] CHK007 - Are all 7 edge cases in the spec mapped to at least one FR? Is there a traceability gap between the edge cases section and the functional requirements? **[Completeness, Spec §Edge Cases, §FR-001–FR-012]**
- [X] CHK008 - Is a requirement defined for the case where `GetAsyncEnumerator()` itself throws before `MoveNextAsync()` is ever called? This path is distinct from a `MoveNextAsync()` throw. **[Completeness, Gap]**
- [X] CHK009 - Are XML documentation requirements for all public members of `AsyncEnumerableAssertions<T>` stated in the spec or plan, or only inferred from the constitution? **[Completeness, Plan §Constitution Check]**
- [X] CHK010 - Is the `ConfigureAwait(false)` requirement for enumerator calls stated in a functional requirement (FR), or does it appear only in research (R-001)? **[Completeness, Spec §FR-003, Research §R-001]**

---

## Requirement Clarity

- [X] CHK011 - Is "draining all items" in the no-exception path (FR-005) defined with an unambiguous termination condition — specifically, does "completion" mean `MoveNextAsync()` returning `false` with no exception thrown? **[Clarity, Spec §FR-005, §Edge Cases]**
- [X] CHK012 - Is the term "source" (used as the canonical domain noun in failure message templates) formally defined in the spec or contracts, or is it only established in research (R-003)? **[Clarity, Spec §FR-005, Research §R-003]**
- [X] CHK013 - Is the `ExceptionAssertions<TException>` return type requirement in FR-008 precise enough to rule out a new derived or wrapper type? Does "the same type" mean the same CLR type, or is a structural equivalent acceptable? **[Clarity, Spec §FR-008]**
- [X] CHK014 - Are the test coverage thresholds (≥93% line, ≥90% branch) stated in the spec or plan, or only assumed from the constitution? Is SC-001 precise enough to specify this? **[Clarity, Spec §SC-001, Plan §Technical Context]**
- [X] CHK015 - Is the boundary between "subtype match passes" (FR-002) and "wrong type fails" (FR-006) defined with enough precision — specifically, is an `is` check (reference type equality via inheritance) the mandated test, versus `==` or `GetType()`? **[Clarity, Spec §FR-002, §FR-006]**

---

## Requirement Consistency

- [X] CHK016 - Is the `CancellationToken`-omission rationale in FR-002 ("consistent with `AsyncFunctionAssertions` and `TaskAssertions`") consistent with the assumption in §Assumptions ("test-level timeout control is the caller's responsibility")? Do both tell the same story? **[Consistency, Spec §FR-002, §Assumptions]**
- [X] CHK017 - Are the `AggregateException` boundary conditions in FR-004 (zero-inner and multi-inner NOT unwrapped) consistent with the edge cases section, which states these are "treated as the caught exception itself"? **[Consistency, Spec §FR-004, §Edge Cases]**
- [X] CHK018 - Does the dispatch table in data-model.md (`IAsyncEnumerable<T>?` → `AsyncEnumerableAssertions<T>`) align exactly with what FR-001 and FR-012 require? **[Consistency, Data Model §Dispatch Table, Spec §FR-001, §FR-012]**
- [X] CHK019 - Do the three failure message templates in research (R-003) align exactly with the wording specified in FR-005, FR-006, and FR-011? Are there any discrepancies in expected/actual strings? **[Consistency, Research §R-003, Spec §FR-005, §FR-006, §FR-011]**
- [X] CHK020 - Is the `AggregateException` unwrap logic described in FR-004 consistent with data-model.md's "Code Paths" table — specifically, does the data-model correctly identify when unwrapping occurs before vs. after type-matching? **[Consistency, Spec §FR-004, Data Model §Code Paths]**

---

## Acceptance Criteria Quality

- [X] CHK021 - Is SC-001 ("every acceptance scenario covered by an automated test") measurable? Are acceptance scenarios enumerated and countable so that completeness can be objectively determined? **[Measurability, Spec §SC-001]**
- [X] CHK022 - Is SC-004 ("`.Which` provides the same caught exception instance") verifiable without re-enumerating the source? What specific assertion would confirm object-identity rather than value-equality? **[Measurability, Spec §SC-004]**
- [X] CHK023 - Are the 7 code paths in the data-model dispatch table each traceable to at least one acceptance scenario or edge-case requirement in the spec? **[Traceability, Data Model §Code Paths, Spec §User Stories, §Edge Cases]**

---

## Scenario Coverage

- [X] CHK024 - Is a requirement defined for a sequence that yields zero items before throwing (throw on first `MoveNextAsync`)? This is called out in the edge cases section but is the FR coverage complete for it? **[Coverage, Spec §Edge Cases, §FR-002]**
- [X] CHK025 - Is a requirement or assumption defined for what happens when the async iterator has a `CancellationToken` parameter and the enumeration is started with the default token (no cancellation)? **[Coverage, Spec §Assumptions]**
- [X] CHK026 - Are requirements or explicit out-of-scope statements defined for what happens when `ThrowAsync` is awaited but the outer test is cancelled via test-framework timeout before enumeration completes? **[Coverage, Gap]**
- [X] CHK027 - Are requirements defined for a custom `IAsyncEnumerator<T>` implementation (one not produced by an async iterator method)? The spec's assumption section notes these are "treated the same" — is this stated as a requirement or only as an assumption? **[Coverage, Spec §Assumptions, Gap]**

---

## Non-Functional Requirements

- [X] CHK028 - Is the ≤100 ms per-`ThrowAsync`-call performance target documented in the spec, or only in the plan's Technical Context section? If only in the plan, is it still enforceable as a requirement? **[Completeness, Plan §Technical Context]**
- [X] CHK029 - Is the AOT-compatibility requirement (no reflection, no code generation) stated in a functional or non-functional requirement in the spec, or only derived from the constitution and plan's constitution-check table? **[Completeness, Plan §Constitution Check]**

---

## Dependencies & Assumptions

- [X] CHK030 - Is the assumption that no BCL type currently implements both `IAsyncEnumerable<T>` and `IEnumerable<T>` explicitly documented, and is there a defined fallback if a future runtime introduces such a type? **[Assumption, Research §R-002]**
- [X] CHK031 - Is the dependency on `ExceptionAssertions<TException>`'s internal constructor signature formally documented? If that constructor changes in a future refactor, is there a traceability link to this feature? **[Assumption, Research §R-005]**
- [X] CHK032 - Is the assumption that `[CallerArgumentExpression]` correctly captures the caller's expression through the `Should<T>()` overload — and does not produce an empty or `null` string under any common usage pattern — stated explicitly in the spec or contracts? **[Assumption, Data Model §New Extension Method]**

---

## Cross-Artifact Traceability

- [X] CHK033 - Does each functional requirement (FR-001 through FR-012) trace to at least one entry in the data-model's code path table or type definition table? **[Traceability, Data Model, Spec §FR-001–FR-012]**
- [X] CHK034 - Does each research decision (R-001 through R-006) trace back to a specific FR or edge case? Confirm no research decision exists that is not grounded in a spec requirement. **[Traceability, Research §R-001–R-006, Spec §FR-001–FR-012]**
- [X] CHK035 - Does the contracts document cover all public members defined in the data-model's type table (`Subject`, `Expression`, `ThrowAsync`)? **[Completeness, Contracts, Data Model §Fields/Methods]**
