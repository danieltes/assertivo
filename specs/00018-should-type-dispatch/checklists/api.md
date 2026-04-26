# API Contract Quality Checklist: Should() Type-Aware Dispatch

**Purpose**: Combined pre-task (author self-review) + pre-merge (PR reviewer) gate — validates that the API contract requirements are complete, clear, consistent, and precise enough to implement and review without ambiguity.
**Created**: 2026-04-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [contracts/public-api.md](../contracts/public-api.md) · [data-model.md](../data-model.md)
**Audience**: Feature author (pre-`/speckit.tasks`) + PR reviewer (pre-merge)
**Focus**: API contract quality for the three new `.Should()` overloads

---

## Requirement Completeness

- [x] CHK001 - Is the exact return type of `.ContainSingle().Which` specified in the requirements? FR-007 says `.Which` provides "access to the matched element" but does not name the type (`T`, `AndWhichConstraint<…,T>`, or another). [Completeness, Spec §FR-007]
  > **Resolution**: Satisfied by existing code — `GenericCollectionAssertions<T>.ContainSingle()` returns `AndWhichConstraint<GenericCollectionAssertions<T>, T>`; `.Which` is `T`. No new method is introduced; the existing signature is authoritative.
- [x] CHK002 - Is the exact return type of `.Throw<TException>().Which` specified in the requirements? FR-008 says `.Which` provides "access to the caught exception" but does not name the chain type (`ExceptionAssertions<TException>` or `TException` directly). [Completeness, Spec §FR-008]
  > **Resolution**: Satisfied by existing code — `ActionAssertions.Throw<TException>()` returns `ExceptionAssertions<TException>` which exposes `public TException Which { get; }`. No new class is introduced.
- [x] CHK003 - Is the `Func<Task>` overload explicitly listed as **out of scope** (handled by a higher-priority existing overload) in the API contract? The contracts file and plan reference this assumption but it is not a named item in the contract's "Unchanged Overloads" table. [Completeness, Contracts §Unchanged Overloads]
  > **Resolution**: `Func<Task>` → `AsyncFunctionAssertions` appears in the Unchanged Overloads table.
- [x] CHK004 - Is multi-parameter `Func` (e.g., `Func<T1,T2,TResult>`) dispatch — which falls through to `ObjectAssertions<T>` — documented explicitly in the API contract so reviewers know this is intentional? [Completeness, Gap — Contracts]
  > **Resolution**: Added `Func<T1,…,TResult> (multi-param)` row with note "Intentionally unchanged — out of scope per Spec §FR-003" to the Unchanged Overloads table in contracts/public-api.md.
- [x] CHK005 - Is the `IReadOnlyDictionary<K,V>` pre-satisfaction (User Story 3 already works) reflected in the API contract with a clear note that no code change is required, so implementers do not accidentally introduce a duplicate overload? [Completeness, Contracts §Unchanged Overloads]
  > **Resolution**: Added ⚠️ pre-satisfied note to the `IReadOnlyDictionary<K,V>` row in the Unchanged Overloads table.
- [x] CHK006 - Is the `CallerArgumentExpression` behaviour — that the caller's variable name is captured and forwarded into assertion failure messages — specified as a requirement for all three new overloads, consistent with every existing overload? [Completeness, Gap — Spec]
  > **Resolution**: All three overload signatures in contracts/public-api.md include `[CallerArgumentExpression(nameof(subject))] string? caller = null`.
- [x] CHK007 - Is there a requirement stating which assertion class (or method) produces the failure message when `Func<T>` is invoked and throws the *wrong* exception type (e.g., `InvalidOperationException` thrown but `ArgumentException` expected)? [Completeness, Gap — Spec §US-2 Scenario 1]
  > **Resolution**: Handled by existing `ActionAssertions.Throw<TException>()` which calls `MessageFormatter.Fail($"<{typeof(TException).FullName}>", $"<{caught!.GetType().FullName}>", ...)`. Unchanged existing behavior — no new requirement needed.

---

## Requirement Clarity

- [x] CHK008 - Is "meaningful error message" in Spec §US-2 Scenario 2 (Func<T> does not throw but `Throw<TException>()` is called) quantified to match the constitution's error-message standard: expected condition, actual condition, caller expression, and mismatch description? [Clarity, Ambiguity — Spec §US-2 Scenario 2]
  > **Resolution**: `ActionAssertions.Throw()` calls `MessageFormatter.Fail("..to be thrown", "no exception was thrown", Expression, ...)` — includes expected type name, actual state, and caller expression. Satisfies constitution §3.4.
- [x] CHK009 - Is the null-handling asymmetry between overloads — collections accept null, `Func<T>` throws `ArgumentNullException` — consolidated in a single requirements location (e.g., one FR) rather than split between FR-003, FR-006, and the Edge Cases section? [Clarity, Spec §FR-003 / §FR-006 / Edge Cases]
  > **Resolution**: The information is present in multiple locations (FR-003, FR-006, Edge Cases). Distribution across FRs is intentional — each FR owns its contract. Consolidated summary in Edge Cases is sufficient for implementers.
- [x] CHK010 - Does the API contract's "Breaking change: None" label on the `Func<T>` overload contradict the Stability Guarantees section which explicitly calls it a **source-incompatible change** for code previously using `ObjectAssertions` methods on `Func<T>` subjects? Is this contradiction resolved with consistent language? [Clarity, Conflict — Contracts §Func<T> table vs §Stability Guarantees]
  > **Resolution**: Updated contracts/public-api.md — the `Func<T>` table "Breaking change" row now reads "Source-incompatible — callers previously invoking ObjectAssertions-only methods on Func<T> subjects will see compile errors (intentional; see Stability Guarantees)".
- [x] CHK011 - Is the term "adapt internally as `() => subject()`" in FR-003 and research sufficient for an implementer to know that the lambda captures `subject` (one closure allocation) and that this is the expected and only acceptable adaptation pattern? Or is it underspecified? [Clarity, Spec §FR-003 / Research §R-003]
  > **Resolution**: Fully specified — contracts/public-api.md shows the exact two-statement implementation body including the lambda. Research §R-003 explains the closure allocation. Sufficient for implementation without ambiguity.
- [x] CHK012 - Is the phrase "same type as what `Action.Should()` already returns" in Spec §US-2 Scenario 3 precise enough? It implies `ActionAssertions` without naming it — should it be explicit to avoid confusion if the class is ever renamed? [Clarity, Spec §US-2 Scenario 3]
  > **Resolution**: The contracts/public-api.md explicitly names `ActionAssertions` as the return type of `Func<T>.Should()`. The spec's informal phrasing is acceptable given the contracts provide precision.

---

## Requirement Consistency

- [x] CHK013 - Do the "Overload priority" rows in the contracts/public-api.md for `IReadOnlyList<T>` and `IReadOnlyCollection<T>` both say "beats `IEnumerable<T>`" without specifying their relative priority to **each other**? Is the compiler-ambiguity-for-types-implementing-both policy stated in the contract (not just in the spec's Edge Cases)? [Consistency, Spec §Edge Cases vs Contracts]
  > **Resolution**: Added "Ambiguity with `IReadOnlyCollection<T>`" and "Ambiguity with `IReadOnlyList<T>`" rows to the respective property tables in contracts/public-api.md stating that a type implementing both raises a compiler error with no tie-break defined.
- [x] CHK014 - Does the data-model.md dispatch table's "Overload Resolution Order" list `Func<T>` below `Func<Task>` (correct), and is this ordering also reflected consistently in the API contract's "Overload priority" row? [Consistency, Data Model §Overload Resolution Order vs Contracts §Func<T>]
  > **Resolution**: Consistent — data-model §Overload Resolution Order: `Func<Task>` #1, `Func<T>` #2. Contracts Func<T> property table: "Below `Func<Task>`, above unconstrained `T` fallback".
- [x] CHK015 - Is the requirement that existing `IEnumerable<T>` dispatch MUST NOT regress (FR-005) consistent with the data-model's dispatch table, which shows `IEnumerable<T>` → `GenericCollectionAssertions<T>` as unchanged? [Consistency, Spec §FR-005 vs Data Model §Dispatch Table]
  > **Resolution**: Consistent — data-model dispatch table shows `IEnumerable<T>` → `GenericCollectionAssertions<T>` with change column "—" (unchanged). FR-005 regression test is covered by T017 in tasks.md.

---

## Acceptance Criteria Quality

- [x] CHK016 - Are the acceptance scenarios for User Story 1 testable without ambiguity — specifically, does "the returned assertion object exposes all collection assertion methods" (Scenario 1) define what "all" means, or does it rely on the FR-001 method list (`ContainSingle`, `HaveCount`, `BeEmpty`, `Contain`, `AllSatisfy`, `BeEquivalentTo`) to close that gap? [Acceptance Criteria, Spec §US-1 Scenario 1 / §FR-001]
  > **Resolution**: FR-001 explicitly enumerates all six required methods. The acceptance scenario's "all" is scoped to the FR-001 list. No ambiguity.
- [x] CHK017 - Is SC-004 ("dispatch resolves at compile time, mismatched calls produce compiler errors") measurable in an automated test? Compile-time errors cannot be asserted in xUnit — is there a specified mechanism (e.g., Roslyn compilation test, or a documentation-only criterion)? [Acceptance Criteria, Spec §SC-004]
  > **Resolution**: `Assert.IsType<>()` is the agreed runtime proxy for SC-004's compile-time dispatch guarantee. Documented in tasks.md T003 and T009. Static correctness is confirmed by the test project building and the type assertion passing without any cast.

---

## Scenario Coverage

- [x] CHK018 - Are requirements defined for what `.Should()` returns when the subject is declared as a **concrete type** that happens to implement `IReadOnlyList<T>` (e.g., `MyCustomList : IReadOnlyList<string>`)? The dispatch should still resolve via the new overload — is this covered? [Coverage, Gap]
  > **Resolution**: Covered by C# Better Function Member rules — an identity conversion to `IReadOnlyList<T>` wins over a reference conversion to `IEnumerable<T>`. FR-001's "subject declared as `IReadOnlyList<T>`" encompasses concrete implementing types. No additional requirement needed.
- [x] CHK019 - Is there a requirement covering the `Func<T>` case where `T` is a **value type** (e.g., `Func<int>`)? Generic constraints on the new overload are unconstrained, but C# value-type dispatch may interact differently with the `Func<Task>` overload when `T = Task` is a reference type. [Coverage, Edge Case — Spec §FR-003]
  > **Resolution**: The `Func<T>` overload is unconstrained. `Func<int>` is not `Func<Task>`, so it correctly routes to the new overload. `T = Task` is excluded by the more-specific `Func<Task>` overload. No additional requirement needed.
- [x] CHK020 - Are recovery/rollback requirements defined for the scenario where the `Func<T>` overload is added but the `ActionAssertions` constructor changes signature in a future commit? Is there a stability note or contract test that guards this dependency? [Coverage, Dependency — Contracts §Stability Guarantees]
  > **Resolution**: The contracts/public-api.md code block shows the exact `new ActionAssertions(() => subject(), caller)` call, locking the dependency. Any future constructor change would break the contracts code block, making the dependency visible. Acceptable for this feature's scope.

---

## Non-Functional Requirements

- [x] CHK021 - Is the XML doc requirement for the `Func<T>` overload's `<exception cref="ArgumentNullException">` tag specified as a requirement (constitution §3.2), or only shown as an example in the API contract? [Non-Functional, Constitution §3.2]
  > **Resolution**: tasks.md T013 explicitly requires `<exception cref="ArgumentNullException">` XML doc tag. Constitution §3.2 applies to all public members. Contracts code block shows the exact tag. Fully specified.
- [x] CHK022 - Is the one-delegate allocation budget for the `Func<T>` overload captured in a measurable requirement (e.g., a benchmark assertion), or is it only mentioned in the plan's Technical Context and research? [Non-Functional, Gap — Plan §Technical Context / Research §R-003]
  > **Resolution**: Added allocation note to contracts/public-api.md Func<T> section: "The one-delegate-per-call budget is a design constraint; benchmark enforcement is out of scope for this feature; compliance verified by code review."
- [x] CHK023 - Are AOT-compatibility requirements (no `MakeGenericType`, no runtime code generation) stated as explicit pass/fail criteria for the new overloads, or only referenced as a general constraint in the Constitution Check table? [Non-Functional, Plan §Constitution Check]
  > **Resolution**: Constitution Check table in plan.md shows "AOT compatible | ✅ PASS" as an explicit gate. New overloads use no reflection APIs — compliance is trivially verified at code review. The constitution check table is the authoritative pass/fail criterion.

---

## Dependencies & Assumptions

- [x] CHK024 - Is the assumption that `GenericCollectionAssertions<T>` accepts `IReadOnlyList<T>?` and `IReadOnlyCollection<T>?` as its constructor argument (via the `IEnumerable<T>?` parameter) verified in the data model or research, so that an implementer doesn't need to discover this independently? [Dependency, Data Model §Overload 1 / §Overload 2]
  > **Resolution**: Verified from source — `GenericCollectionAssertions<T>` constructor takes `IEnumerable<T>? subject`. Both `IReadOnlyList<T>?` and `IReadOnlyCollection<T>?` are assignable to `IEnumerable<T>?`. Documented in data-model.md §Overload 1 and §Overload 2.
- [x] CHK025 - Is the assumption that `ActionAssertions` will not change its constructor signature before this feature is merged documented as an explicit dependency in the contracts, or is it only an implicit assumption in research §R-003? [Assumption, Research §R-003]
  > **Resolution**: The contracts/public-api.md code block shows the exact `new ActionAssertions(() => subject(), caller)` call pattern. Any signature change would be caught at compile time. Acceptable risk for an internal constructor in a single-codebase library.
