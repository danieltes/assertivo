# Data Model: Should() Type-Aware Dispatch

**Feature**: 00018-should-type-dispatch  
**Date**: 2026-04-25

---

## Overview

This feature adds no new domain entities, storage schemas, or assertion classes.
The "data model" for this feature is the **dispatch table** — the mapping from
subject type to assertion type — and the **state carried by each affected type**.

---

## Dispatch Table (Complete — Before vs. After)

| Subject declared type | Before (current) | After (this feature) | Change |
|-----------------------|-----------------|----------------------|--------|
| `bool` | `BooleanAssertions` | `BooleanAssertions` | — |
| `int` | `NumericAssertions<int>` | `NumericAssertions<int>` | — |
| `long` | `NumericAssertions<long>` | `NumericAssertions<long>` | — |
| `string?` | `StringAssertions` | `StringAssertions` | — |
| `Action` | `ActionAssertions` | `ActionAssertions` | — |
| `Func<Task>` | `AsyncFunctionAssertions` | `AsyncFunctionAssertions` | — |
| `Func<T>` (single param, non-Task) | `ObjectAssertions<Func<T>>` ❌ | `ActionAssertions` ✅ | **NEW** |
| `T[]` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `List<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IEnumerable<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IReadOnlyList<T>` | `ObjectAssertions<IReadOnlyList<T>>` ❌ | `GenericCollectionAssertions<T>` ✅ | **NEW** |
| `IReadOnlyCollection<T>` | `ObjectAssertions<IReadOnlyCollection<T>>` ❌ | `GenericCollectionAssertions<T>` ✅ | **NEW** |
| `IEnumerable<KeyValuePair<K,V>>` | `GenericDictionaryAssertions<K,V>` | `GenericDictionaryAssertions<K,V>` | — |
| `IDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | `GenericDictionaryAssertions<K,V>` | — |
| `IReadOnlyDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` ✅ | `GenericDictionaryAssertions<K,V>` | — (pre-satisfied) |
| `T` (fallback) | `ObjectAssertions<T>` | `ObjectAssertions<T>` | — |

---

## New Extension Method Overloads

### Overload 1 — `IReadOnlyList<T>`

```
Type:          Static extension method on ShouldExtensions
File:          src/Assertivo/Should.cs
Signature:     Should<T>(this IReadOnlyList<T>? subject, string? caller) → GenericCollectionAssertions<T>
Null subject:  Accepted; forwarded to GenericCollectionAssertions<T>(null, caller)
Constraints:   None beyond T (unconstrained, matching GenericCollectionAssertions<T>)
AOT-safe:      Yes — no reflection
```

### Overload 2 — `IReadOnlyCollection<T>`

```
Type:          Static extension method on ShouldExtensions
File:          src/Assertivo/Should.cs
Signature:     Should<T>(this IReadOnlyCollection<T>? subject, string? caller) → GenericCollectionAssertions<T>
Null subject:  Accepted; forwarded to GenericCollectionAssertions<T>(null, caller)
Constraints:   None beyond T
AOT-safe:      Yes
```

### Overload 3 — `Func<T>`

```
Type:          Static extension method on ShouldExtensions
File:          src/Assertivo/Should.cs
Signature:     Should<T>(this Func<T> subject, string? caller) → ActionAssertions
Null subject:  Rejected — ArgumentNullException.ThrowIfNull(subject) called immediately
Adaptation:    subject is wrapped as () => subject() before passing to ActionAssertions
Allocation:    One Action delegate per call (closure over subject)
Constraints:   T is unconstrained; T must NOT be Task (Func<Task> is handled by a
               higher-priority overload added before this one in resolution order)
AOT-safe:      Yes — lambda is a static closure, no reflection
```

---

## Overload Resolution Order (Relevant Fragment)

C# resolves extension method overloads top-to-bottom within a file only when
specificity is equal. For the three new overloads, specificity ranks as follows
(most specific → least specific):

```
1. Func<Task>             (exact match for async delegates — existing overload, unchanged)
2. Func<T>                (NEW — catches all non-Task single-param Func delegates)
3. IReadOnlyList<T>       (NEW — exact match for IReadOnlyList; beats IEnumerable<T>)
4. IReadOnlyCollection<T> (NEW — exact match for IReadOnlyCollection; beats IEnumerable<T>)
5. IEnumerable<T>         (existing — catches arrays, List<T>, and other enumerables)
6. T (fallback)           (existing — catches all remaining reference/value types)
```

**Placement within `Should.cs`**: The three new overloads MUST be inserted above the
`IEnumerable<T>` overload and below the `Func<Task>` overload so that source file
order matches conceptual specificity (improving readability).

---

## Validation Rules

| Overload | Guard | Behaviour on violation |
|----------|-------|------------------------|
| `IReadOnlyList<T>` | None (nullable) | `BeNull()` assertion on the result |
| `IReadOnlyCollection<T>` | None (nullable) | `BeNull()` assertion on the result |
| `Func<T>` | `ArgumentNullException.ThrowIfNull(subject)` | Throws before `ActionAssertions` is constructed |

---

## State Transitions

None. All three new overloads are stateless factory methods. No persistent state
is introduced. The assertion objects they return (`GenericCollectionAssertions<T>`,
`ActionAssertions`) are existing immutable `readonly struct` types.
