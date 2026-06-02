# Data Model: Predicate-Based Collection Containment

**Feature**: `00029-contain-predicate`  
**Phase**: 1 - Design  
**Date**: 2026-06-02

This feature introduces no new persisted entities. It is an additive API behavior on an existing assertion type.

## Affected Type

### `GenericCollectionAssertions<T>` (`Assertivo.Collections`)

**New member**:

```csharp
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> Contain(
    Func<T, bool> predicate,
    string because = "",
    params object[] becauseArgs)
```

## Conceptual Runtime Model

### Assertion Invocation

- **Subject**: `IEnumerable<T>?`
- **Predicate**: `Func<T, bool>`
- **Reason**: `because` + `becauseArgs`
- **Output**: `AndConstraint<GenericCollectionAssertions<T>>` on pass, assertion failure or `ArgumentNullException` on invalid input

## Validation Rules

- `predicate` MUST be non-null, otherwise throw `ArgumentNullException`.
- `Subject` MUST be non-null for assertion evaluation, enforced by existing `GuardNull()`.
- Assertion passes if at least one element satisfies `predicate`.
- Assertion fails if zero elements satisfy `predicate`.
- Failure output must preserve reason formatting when `because` is supplied.

## State Transitions

1. **Start**: `Contain(predicate, because, becauseArgs)` invoked.
2. **Guard Predicate**: null -> `ArgumentNullException`.
3. **Guard Subject**: null -> assertion failure via `GuardNull()`.
4. **Evaluate**: iterate collection until first match.
5. **Pass Path**: first match found -> return `AndConstraint<GenericCollectionAssertions<T>>`.
6. **Fail Path**: enumeration ends with no match -> `MessageFormatter.Fail(...)`.

## Invariants

- Fluent chain composability remains intact (`.And`).
- No new global state or external dependency.
- Behavior remains deterministic for empty collections (always fail unless a match exists, which is impossible).