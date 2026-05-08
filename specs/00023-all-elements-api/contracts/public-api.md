# Public API Contract: AllSatisfy First-Class All-Elements Assertions

**Feature**: 00023-all-elements-api  
**Date**: 2026-05-08  
**Namespace**: `Assertivo.Collections`  
**Type**: `GenericCollectionAssertions<T>`

---

## Public Methods

### 1. Existing canonical overload (retained, behavior strengthened)

```csharp
public AndConstraint<GenericCollectionAssertions<T>> AllSatisfy(
    Action<T> inspector,
    string because = "",
    params object[] becauseArgs)
```

### 2. New index-aware overload (additive)

```csharp
public AndConstraint<GenericCollectionAssertions<T>> AllSatisfy(
    Action<T, int> inspector,
    string because = "",
    params object[] becauseArgs)
```

---

## Terminology

- **Assertion-related exception**: `AssertionFailedException` (or subtype) produced by Assertivo assertion APIs.
- **Non-assertion exception**: Any exception type not covered by the assertion-related definition above.
- **Framework assertion exception**: Exception type surfaced by the active test-framework adapter from the standard assertion failure pipeline; falls back to `AssertionFailedException` when no framework adapter is active.

---

## Behavioral Contract

| Condition | Required behavior |
|-----------|-------------------|
| `Subject` is null | Fail through assertion failure pipeline (not null-reference crash). |
| `inspector` is null | Throw `ArgumentNullException` immediately. |
| Inspector throws assertion-related exception for an element | Capture as element failure with index + message. |
| Inspector throws non-assertion exception for an element | Capture as element failure with index + exception type + message. |
| Source enumerable throws during iteration | Stop immediately and rethrow original exception unchanged. |
| No element failures | Return `AndConstraint<GenericCollectionAssertions<T>>`. |
| One or more inspector failures | Surface one framework assertion exception via standard pipeline after aggregation (fallback `AssertionFailedException`). |

---

## Aggregated Diagnostics Contract

When element failures exist, diagnostics MUST satisfy all of the following:

1. Preserve enumeration order in detailed entries and index reporting.
2. Include total failed count.
3. Include all failing indices.
4. Include full per-failure details for first 50 failures.
5. Render failing indices adaptively:
   - Explicit ordered list when count <= 100.
   - Ordered range-compressed list when count > 100.
    - Segment grammar: `N` (singleton) or `Start-End` (inclusive contiguous range).
    - Segments MUST be ascending and non-overlapping.
    - Singleton values MUST be rendered as `N` (not `N-N`).
    - Combined rendered segments MUST preserve complete failing-index coverage with no loss.
6. Apply `because` and `becauseArgs` to the final failure output.

---

## Scope and Compatibility

- Canonical naming remains `AllSatisfy`.
- Predicate-only `OnlyContain(Func<T, bool>)` is not introduced by this feature.
- Async per-element callback APIs are not introduced by this feature.
- Existing dispatch paths resolving to `GenericCollectionAssertions<T>` remain supported:
  - `IEnumerable<T>`
  - `IReadOnlyList<T>`
  - `IReadOnlyCollection<T>`
  - `List<T>`
  - `T[]`
