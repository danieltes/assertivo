# Public API Contract: Collection and Dictionary Null-Guard Assertions

**Branch**: `00021-fix-dict-not-be-null`  
**Date**: 2026-04-26  
**Scope**: New public methods on `GenericDictionaryAssertions<TKey, TValue>` and `GenericCollectionAssertions<T>`

---

## GenericDictionaryAssertions&lt;TKey, TValue&gt;

Namespace: `Assertivo.Collections`

### BeNull

```csharp
/// <summary>
/// Asserts that the subject is <see langword="null"/>.
/// </summary>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<GenericDictionaryAssertions<TKey, TValue>> BeNull(
    string because = "",
    params object[] becauseArgs)
```

**Behaviour**:

| Subject | Outcome | Failure message |
|---------|---------|-----------------|
| `null` | Passes; returns `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>` | — |
| non-null | Fails | `Expected <null> but found <value>.` |

**Chaining example**:

```csharp
// Simply asserting null (returns And for optional further chaining)
IDictionary<string, int>? dict = null;
dict.Should().BeNull();
```

---

### NotBeNull

```csharp
/// <summary>
/// Asserts that the subject is not <see langword="null"/>.
/// </summary>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<GenericDictionaryAssertions<TKey, TValue>> NotBeNull(
    string because = "",
    params object[] becauseArgs)
```

**Behaviour**:

| Subject | Outcome | Failure message |
|---------|---------|-----------------|
| non-null | Passes; returns `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>` | — |
| `null` | Fails | `Expected not <null> but found <null>.` |

**Chaining examples**:

```csharp
// Assert not null, then continue asserting dictionary content
IReadOnlyDictionary<string, int>? config = GetConfig();
config.Should().NotBeNull().And.ContainKey("timeout");

// With a reason
IDictionary<string, int>? dict = null;
// Throws: "Expected not <null> but found <null>.\nBecause: settings must be loaded"
dict.Should().NotBeNull("settings must be loaded");
```

---

## GenericCollectionAssertions&lt;T&gt;

Namespace: `Assertivo.Collections`

### BeNull

```csharp
/// <summary>
/// Asserts that the subject is <see langword="null"/>.
/// </summary>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> BeNull(
    string because = "",
    params object[] becauseArgs)
```

**Behaviour**:

| Subject | Outcome | Failure message |
|---------|---------|-----------------|
| `null` | Passes; returns `AndConstraint<GenericCollectionAssertions<T>>` | — |
| non-null | Fails | `Expected <null> but found <value>.` |

---

### NotBeNull

```csharp
/// <summary>
/// Asserts that the subject is not <see langword="null"/>.
/// </summary>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> NotBeNull(
    string because = "",
    params object[] becauseArgs)
```

**Behaviour**:

| Subject | Outcome | Failure message |
|---------|---------|-----------------|
| non-null | Passes; returns `AndConstraint<GenericCollectionAssertions<T>>` | — |
| `null` | Fails | `Expected not <null> but found <null>.` |

**Chaining example**:

```csharp
IEnumerable<int>? items = GetItems();
items.Should().NotBeNull().And.HaveCount(3);
```

---

## Failure Message Format

Both methods produce messages via `MessageFormatter.Fail`, which follows the library's standard format:

```
Expected {expected} but found {actual}.
[Expression: {expression}]
[Because: {reason}]
```

The `Expression:` line appears when the assertion is invoked with a named variable (via `[CallerArgumentExpression]`). The `Because:` line appears only when a non-empty `because` string is provided.

---

## Breaking Changes

None. These are additive methods on existing types.

## Versioning

No version bump required beyond the next regular release.
