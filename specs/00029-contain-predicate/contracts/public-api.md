# Public API Contract: Predicate-Based Collection Containment

**Feature**: `00029-contain-predicate`  
**Phase**: 1 - Design  
**Date**: 2026-06-02  
**Namespace**: `Assertivo.Collections`

## New Overload on `GenericCollectionAssertions<T>`

```csharp
/// <summary>
/// Asserts that the collection contains at least one element matching <paramref name="predicate"/>.
/// </summary>
/// <param name="predicate">The match predicate. Must not be <see langword="null"/>.</param>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="predicate"/> is <see langword="null"/>.
/// </exception>
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> Contain(
    Func<T, bool> predicate,
    string because = "",
    params object[] becauseArgs);
```

## Behavior Contract

| Scenario | Outcome |
|---|---|
| Subject is non-null and at least one element matches predicate | Passes and returns `AndConstraint<GenericCollectionAssertions<T>>` |
| Subject is non-null and no elements match predicate | Fails assertion with no-match message |
| Subject is empty | Fails assertion with no-match message |
| Subject is null | Fails assertion via canonical collection null-guard contract (`expected = "a collection"`, `actual = "<null>"`) |
| Predicate is null | Throws `ArgumentNullException` before subject evaluation |
| `because` supplied on failure | Reason text is included in final failure message |

## Evaluation and Enumeration Contract

- Evaluation is single-pass and forward-only.
- Enumeration stops at the first matching element.
- No second pass or materialization is required for pass/fail determination.
- If the predicate throws, the exception propagates unchanged.
- Side-effectful predicates are permitted; side effects occur according to actual invocation count before early exit or sequence end.

## Because Formatting Contract

| Input Form | Expected Behavior |
|---|---|
| `because = ""` | No `Because:` line is emitted |
| `because = "   "` | Treated as not supplied; no `Because:` line is emitted |
| `because = "because {0}", becauseArgs = ["X"]` | `Because: because X` line is emitted |

## Failure Message Contract (No Match)

When evaluation completes with zero matches, the failure is emitted through `MessageFormatter.Fail` with:

- Expected: `a collection containing a matching element`
- Actual: `no element matched the predicate`
- Expression: caller expression when available
- Because: appended when non-empty

Representative output:

```
Expected a collection containing a matching element but found no element matched the predicate.
Expression: orders
Because: a shipped order should exist
```

### Machine-Checkable Diagnostic Requirements

For no-match failures, tests MUST be able to assert all of the following:

1. `Expected` includes `a collection containing a matching element`.
2. `Actual` includes `no element matched the predicate`.
3. `Expression:` line is present when caller expression metadata is available.
4. `Because:` line is present only when a non-empty/non-whitespace reason is supplied.

## Chaining Contract

The overload returns `AndConstraint<GenericCollectionAssertions<T>>`, enabling fluent continuation:

```csharp
orders.Should().Contain(o => o.Status == "Shipped").And.HaveCount(3);
```

## Compatibility

- Additive API change only.
- Existing `Contain(T expected, IEqualityComparer<T>? comparer = null, ...)` overload remains unchanged.
- `ContainSingle(predicate)` remains the exact-one-match API and is not modified.

## Traceability Matrix (FR-001..FR-010, SC-001..SC-005)

| Requirement | Contract Coverage | Success Criteria Link |
|---|---|---|
| FR-001 | New overload signature + Behavior Contract (at least one match passes) | SC-001, SC-002 |
| FR-002 | Signature includes `because`/`becauseArgs` + Because Formatting Contract | SC-001, SC-003 |
| FR-003 | Failure Message Contract (No Match) | SC-001, SC-003 |
| FR-004 | Behavior Contract null-subject row | SC-001, SC-003 |
| FR-005 | Behavior Contract empty-subject row | SC-001, SC-003 |
| FR-006 | Chaining Contract return type and example | SC-001, SC-004 |
| FR-007 | Behavior Contract one-or-more match semantics | SC-001 |
| FR-008 | Documentation Update Scope section | SC-002 |
| FR-009 | Machine-Checkable Diagnostic Requirements + referenced test obligations | SC-001..SC-005 |
| FR-010 | Behavior Contract predicate-null row (throw before subject evaluation) | SC-001 |

## Documentation Update Scope

Completion requires all of the following to be updated consistently:

1. `specs/00029-contain-predicate/contracts/public-api.md` (this contract).
2. `specs/00029-contain-predicate/quickstart.md` (usage and failure examples).
3. XML documentation comments for the public overload in `src/Assertivo/Collections/GenericCollectionAssertions.cs`.

## Terminology

- **Matching element**: an element `e` for which `predicate(e)` returns `true`.
- **Matching predicate**: shorthand for "predicate evaluated to true for at least one element".
- **No match**: sequence completed with zero matching elements.