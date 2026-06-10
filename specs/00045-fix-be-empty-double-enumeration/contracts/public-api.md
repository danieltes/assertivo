# Public API Contract: BeEmpty

## Method Signature (unchanged)

```csharp
public AndConstraint<GenericCollectionAssertions<T>> BeEmpty(
    string because = "",
    params object[] becauseArgs)
```

**Declared on**: `GenericCollectionAssertions<T>` (`src/Assertivo/Collections/GenericCollectionAssertions.cs`)

## Contract

| Scenario | Precondition | Postcondition |
|----------|--------------|---------------|
| Empty collection | Subject is a non-null `IEnumerable<T>` that yields 0 elements | Returns `AndConstraint` for chaining; no exception |
| Non-empty collection | Subject is a non-null `IEnumerable<T>` that yields ≥ 1 element | Throws `AssertionFailedException` with `Expected = "an empty collection"`, `Actual = "a collection with N item(s)"` |
| Null subject | Subject is `null` | Throws `AssertionFailedException` with `Expected = "a collection"`, `Actual = "<null>"` |

## Enumeration Guarantee (new — enforced by fix)

The subject `IEnumerable<T>` is enumerated **exactly once**, regardless of collection type. Safe for non-replayable sequences (LINQ-to-database queries, `Channel<T>` readers, custom iterators with side effects).

## Failure Message Format

```
Expected: an empty collection
Actual:   a collection with {count} item(s)
```

The `{count}` value is captured from the single enumeration and accurately reflects the element count even for non-replayable sequences.
