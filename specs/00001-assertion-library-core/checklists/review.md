# Review Checklist: Assertion Library Core

**Purpose**: Thorough requirements quality review covering API design, performance, and edge case dimensions
**Created**: 2026-03-31
**Feature**: [spec.md](../spec.md)
**Depth**: Thorough | **Audience**: Reviewer (PR/spec review)

## Requirement Completeness

- [ ] CHK001 - Are all 7 assertion categories documented with complete method catalogs, including every overload signature? [Completeness, Spec §FR-001 through FR-009]
- [ ] CHK002 - Are error message format requirements specified per assertion method, or only generically via FR-013? [Completeness, Gap]
- [ ] CHK003 - Are thread-safety requirements (FR-022) specified with concrete semantics — e.g., "no shared mutable state" vs "explicit synchronization"? [Completeness, Spec §FR-022]
- [ ] CHK004 - Are requirements defined for `AssertionConfiguration.ReportFailure` thread-safety when concurrent test runners read/write the delegate? [Completeness, Gap]
- [ ] CHK005 - Are acceptance scenarios defined for the custom comparer injection paths added via clarification (FR-002, FR-004, FR-006)? [Completeness, Gap]
- [ ] CHK006 - Are requirements specified for what `Subject` property exposes on assertion types — read-only, always available, or only on debug? [Completeness, Gap]

## Requirement Clarity

- [ ] CHK007 - Is "zero-allocation happy path" (SC-006) scoped precisely — value assertions only, or also string/boolean/collection pass paths? [Clarity, Spec §SC-006]
- [ ] CHK008 - Is "descriptive exception" (FR-013) defined with a concrete message format template or pattern, not just enumerated fields? [Clarity, Spec §FR-013]
- [ ] CHK009 - Is the `BeSameAs` value-type guard specified as a compile-time error (generic constraint) or a clear runtime error? [Clarity, Spec §Edge Cases]
- [ ] CHK010 - Is "pluggable failure-reporting mechanism" (FR-014) defined with specific extension points, lifecycle, and reset semantics? [Clarity, Spec §FR-014]
- [ ] CHK011 - Is `BeOfType<T>()` exact-match behavior documented for interface types (e.g., `object.Should().BeOfType<IDisposable>()`)? [Clarity, Spec §US-9]
- [ ] CHK012 - Is the `ContainKey` failure message content specified — does "listing available keys" mean all keys, a truncated subset, or a count? [Clarity, Spec §US-5]
- [ ] CHK013 - Is the `Should()` overload resolution priority documented for types implementing multiple matched interfaces (e.g., `Dictionary<K,V>` matching both collection and dictionary overloads)? [Clarity, Spec §data-model, Type Resolution]

## Requirement Consistency

- [ ] CHK014 - Does `StringAssertions.Be` inherit the ordinal default from FR-005, or is case-sensitivity for value equality left unspecified? [Consistency, Spec §FR-005]
- [ ] CHK015 - Are `ExceptionAssertions<T>.And` self-reference semantics consistent with `AndConstraint<T>.And` parent-scope semantics, given both use `.And`? [Consistency, Spec §FR-010, FR-011]
- [ ] CHK016 - Does the dictionary `Should()` overload for `IEnumerable<KeyValuePair<K,V>>` conflict with the generic collection overload when `T` is `KeyValuePair<K,V>`? [Consistency, Spec §data-model]
- [ ] CHK017 - Are `BeEquivalentTo` overloads consistent — does the `params T[]` overload support `because`/`comparer` parameters like the `IEnumerable<T>` overload? [Consistency, Spec §public-api]
- [ ] CHK018 - Is Assumption #2 (.NET Standard 2.0 target) reconciled with the plan's .NET 10 single-TFM decision? [Conflict, Spec §Assumptions vs Plan §Technical Context]

## Acceptance Criteria Quality

- [ ] CHK019 - Can SC-001 ("install to first assertion in <60 seconds") be objectively measured in a CI environment? [Measurability, Spec §SC-001]
- [ ] CHK020 - Is SC-006 ("zero-allocation") testable with a specific benchmark methodology (e.g., BenchmarkDotNet `[MemoryDiagnoser]`)? [Measurability, Spec §SC-006]
- [ ] CHK021 - Are SC-007 coverage targets (95% line, 90% branch) specified with exclusion rules for generated or unreachable code? [Measurability, Spec §SC-007]
- [ ] CHK022 - Is SC-004 ("fluent chains of at least 3 links") scoped to specific assertion categories or required for all 7? [Completeness, Spec §SC-004]
- [ ] CHK023 - Are failure message requirements in SC-003 testable — is the expected message structure defined precisely enough to write negative tests? [Measurability, Spec §SC-003]

## Scenario Coverage

- [ ] CHK024 - Are failure-path acceptance scenarios defined for all 22 assertion methods, or only for a representative subset? [Coverage, Gap]
- [ ] CHK025 - Are acceptance scenarios defined for concurrent assertion execution (FR-022) from parallel test runners? [Coverage, Gap]
- [ ] CHK026 - Are acceptance scenarios defined for `AllSatisfy` when multiple elements fail the predicate (not just one)? [Coverage, Spec §US-4]
- [ ] CHK027 - Are acceptance scenarios defined for `because` parameter formatting with `becauseArgs` (FR-012) across different assertion types? [Coverage, Gap]
- [ ] CHK028 - Are acceptance scenarios defined for the `[CallerArgumentExpression]` expression capture in failure messages? [Coverage, Spec §FR-013]

## Edge Case Coverage

- [ ] CHK029 - Is the behavior specified when `ContainSingle(predicate)` receives a null predicate? [Edge Case, Gap]
- [ ] CHK030 - Is the behavior specified when `BeEquivalentTo` compares two empty collections? [Edge Case, Gap]
- [ ] CHK031 - Is the behavior specified for nested `AggregateException` unwrapping in `ThrowAsync<T>()` (inner exception is itself `AggregateException`)? [Edge Case, Spec §FR-020]
- [ ] CHK032 - Is the behavior specified when `because` format string contains invalid format specifiers with mismatched `becauseArgs`? [Edge Case, Spec §FR-012]
- [ ] CHK033 - Is the behavior specified when `AllSatisfy` inspector throws a non-assertion exception (e.g., `NullReferenceException`)? [Edge Case, Gap]
- [ ] CHK034 - Is the behavior specified when a `Func<Task>` subject returns a null `Task`? [Edge Case, Gap]
- [ ] CHK035 - Is the behavior specified when `[CallerArgumentExpression]` capture is unavailable (e.g., called via reflection or indirect invocation)? [Edge Case, Spec §data-model]

## Non-Functional Requirements — Performance

- [ ] CHK036 - Is the "≥ 10M ops/sec" throughput target traceable to a spec-level requirement or success criterion, or only defined in the plan? [Traceability, Gap]
- [ ] CHK037 - Is the "< 1 KB allocation for collection assertions under 1,000 elements" constraint defined in the spec or only in the plan? [Traceability, Gap]
- [ ] CHK038 - Are AOT compatibility requirements (no reflection, no dynamic code generation) specified in the feature spec, not just inferred from the plan? [Completeness, Gap]
- [ ] CHK039 - Are performance degradation boundaries defined for large collections (e.g., `BeEquivalentTo` with 100K+ elements)? [Coverage, Gap]

## Non-Functional Requirements — API Design

- [ ] CHK040 - Is the API stability contract defined — which public types/methods are guaranteed stable vs experimental? [Completeness, Gap]
- [ ] CHK041 - Is the `[ModuleInitializer]`-based adapter registration lifecycle documented — what happens with multiple adapters or re-registration? [Clarity, Spec §FR-014, data-model]
- [ ] CHK042 - Are namespace assignment rules for assertion types discoverable from the spec (e.g., why `StringAssertions` is in root but `GenericCollectionAssertions<T>` is in `Assertivo.Collections`)? [Clarity, Gap]

## Notes

- Focus areas: API Design + Performance + Edge Cases (all dimensions)
- Depth: Thorough (~42 items)
- Audience: Reviewer (peer PR/spec review)
- Items reference spec.md (FR/US/SC/Edge Cases), plan.md (Technical Context), data-model.md (Type Resolution), and public-api.md (contracts)
- CHK018 flags a known conflict between spec Assumption #2 and the plan's user-directed .NET 10 override — reviewer should confirm the assumption text is updated
