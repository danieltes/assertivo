# Data Model: Collection and Dictionary Null-Guard Assertions

**Branch**: `00021-fix-dict-not-be-null`  
**Date**: 2026-04-26

---

## Overview

This feature adds two assertion methods to two existing assertion types. There are no new entities, no new storage, and no schema changes. The data model describes the behavioral contracts and state transitions of the assertion types as modified.

---

## Entity: `GenericDictionaryAssertions<TKey, TValue>`

**Location**: `src/Assertivo/Collections/GenericDictionaryAssertions.cs`  
**Kind**: `readonly struct`  
**Namespace**: `Assertivo.Collections`

### Fields (existing, unchanged)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `Subject` | `IEnumerable<KeyValuePair<TKey, TValue>>` | yes | The dictionary under test |
| `Expression` | `string` | yes | Caller argument expression (for diagnostics) |

### Methods (delta — new additions)

#### `BeNull(string because = "", params object[] becauseArgs)`

| Attribute | Value |
|-----------|-------|
| Return type | `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>` |
| Pre-condition | None (explicitly handles null subject) |
| Passing condition | `Subject is null` |
| Failing condition | `Subject is not null` |
| Failure expected | `"<null>"` |
| Failure actual | `MessageFormatter.FormatValue(Subject)` |
| Side effects | None |

#### `NotBeNull(string because = "", params object[] becauseArgs)`

| Attribute | Value |
|-----------|-------|
| Return type | `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>` |
| Pre-condition | None (explicitly handles null subject) |
| Passing condition | `Subject is not null` |
| Failing condition | `Subject is null` |
| Failure expected | `"not <null>"` |
| Failure actual | `"<null>"` |
| Side effects | None |

### State transitions

```
Subject state      BeNull()       NotBeNull()
─────────────────────────────────────────────
null               PASS → chain   FAIL → throw
non-null           FAIL → throw   PASS → chain
```

---

## Entity: `GenericCollectionAssertions<T>`

**Location**: `src/Assertivo/Collections/GenericCollectionAssertions.cs`  
**Kind**: `readonly struct`  
**Namespace**: `Assertivo.Collections`

### Fields (existing, unchanged)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `Subject` | `IEnumerable<T>` | yes | The collection under test |
| `Expression` | `string` | yes | Caller argument expression (for diagnostics) |

### Methods (delta — new additions)

#### `BeNull(string because = "", params object[] becauseArgs)`

| Attribute | Value |
|-----------|-------|
| Return type | `AndConstraint<GenericCollectionAssertions<T>>` |
| Pre-condition | None (explicitly handles null subject) |
| Passing condition | `Subject is null` |
| Failing condition | `Subject is not null` |
| Failure expected | `"<null>"` |
| Failure actual | `MessageFormatter.FormatValue(Subject)` |
| Side effects | None |

#### `NotBeNull(string because = "", params object[] becauseArgs)`

| Attribute | Value |
|-----------|-------|
| Return type | `AndConstraint<GenericCollectionAssertions<T>>` |
| Pre-condition | None (explicitly handles null subject) |
| Passing condition | `Subject is not null` |
| Failing condition | `Subject is null` |
| Failure expected | `"not <null>"` |
| Failure actual | `"<null>"` |
| Side effects | None |

### State transitions

```
Subject state      BeNull()       NotBeNull()
─────────────────────────────────────────────
null               PASS → chain   FAIL → throw
non-null           FAIL → throw   PASS → chain
```

---

## Validation Rules

| Rule | Applies to | Description |
|------|-----------|-------------|
| No mutation | Both types | Methods MUST NOT mutate Subject or Expression |
| Null-safe | Both types | Both methods MUST NOT call `GuardNull()` — they handle null intentionally |
| `because` optional | Both types | Default `""` means no reason appended to failure message |
| `becauseArgs` optional | Both types | `params object[]` allows formatted reason strings |

---

## Dependencies (unchanged)

```
GenericDictionaryAssertions<K,V>
  └── MessageFormatter          (static; Fail, FormatValue)
  └── AndConstraint<T>          (readonly struct; wraps parent for chaining)
  └── AssertionConfiguration    (via MessageFormatter.Fail → ReportFailure)

GenericCollectionAssertions<T>
  └── MessageFormatter          (same)
  └── AndConstraint<T>          (same)
  └── AssertionConfiguration    (same)
```

No new dependencies are introduced.
