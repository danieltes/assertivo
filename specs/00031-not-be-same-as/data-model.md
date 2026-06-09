# Data Model: NotBeSameAs — Object Reference Inequality Assertion

**Date**: 2026-06-08
**Branch**: `00031-not-be-same-as`

## Overview

No new entities are introduced by this feature. `NotBeSameAs` is a pure method addition to an existing type. The data model below documents the relevant existing entities and how the new method fits within them.

---

## Entity: ObjectAssertions\<T\>

**Location**: `src/Assertivo/ObjectAssertions.cs`
**Kind**: `readonly struct`
**Namespace**: `Assertivo`

| Member | Type | Description |
|--------|------|-------------|
| `Subject` | `T` | The value under assertion, captured at construction time |
| `Expression` | `string?` | Caller expression string, captured via `[CallerArgumentExpression]` in `Should()` |
| `Be(...)` | `AndConstraint<ObjectAssertions<T>>` | Value equality assertion |
| `NotBe(...)` | `AndConstraint<ObjectAssertions<T>>` | Value inequality assertion |
| `BeSameAs(...)` | `AndConstraint<ObjectAssertions<T>>` | Reference equality assertion |
| **`NotBeSameAs(...)`** | `AndConstraint<ObjectAssertions<T>>` | **New** — Reference inequality assertion |
| `BeNull(...)` | `AndConstraint<ObjectAssertions<T>>` | Null check assertion |
| `NotBeNull(...)` | `AndConstraint<ObjectAssertions<T>>` | Non-null check assertion |
| `BeOfType<TTarget>(...)` | `AndWhichConstraint<ObjectAssertions<T>, TTarget>` | Exact type assertion |

### NotBeSameAs method signature

```
NotBeSameAs(object? unexpected, string because = "", params object[] becauseArgs)
  → AndConstraint<ObjectAssertions<T>>
```

**Parameter constraints**:
- `unexpected`: Any reference type value or `null`; value types are rejected at runtime via guard
- `because`: Optional human-readable failure reason; supports `string.Format`-style placeholders when `becauseArgs` is non-empty
- `becauseArgs`: Format arguments for `because`; may be empty array

**State transitions / guard logic**:

```
invoke NotBeSameAs(unexpected, because, becauseArgs)
  ├─ if typeof(T).IsValueType
  │    └─ throw InvalidOperationException (guard — no comparison performed)
  ├─ if ReferenceEquals(Subject, unexpected) == true
  │    └─ MessageFormatter.Fail(...)  →  throws AssertionFailedException
  └─ else
       └─ return AndConstraint<ObjectAssertions<T>>(this)  [pass]
```

---

## Entity: AndConstraint\<TAssertions\>

**Location**: `src/Assertivo/Primitives/AndConstraint.cs`
**Kind**: `readonly struct`
**Namespace**: `Assertivo.Primitives`

| Member | Type | Description |
|--------|------|-------------|
| `And` | `TAssertions` | The parent assertion context, enabling continued chaining |

**Relationship to NotBeSameAs**: `NotBeSameAs` returns `new AndConstraint<ObjectAssertions<T>>(this)` on the passing path, preserving the assertion context for `.And` chaining.

---

## Entity: AssertionFailedException

**Location**: `src/Assertivo/AssertionFailedException.cs`
**Kind**: `class` (exception)

| Property | Type | Set to |
|----------|------|--------|
| `Expected` | `string` | `"not the same reference"` |
| `Actual` | `string` | `"same reference"` |
| `Expression` | `string?` | Caller expression string from `Subject` |
| `Reason` | `string` | Formatted `because` string |
| `Message` | `string` | Full human-readable failure message |

**Populated by**: `MessageFormatter.Fail("not the same reference", "same reference", Expression, because, becauseArgs)`

---

## Entity: InvalidOperationException (value-type guard)

**Standard BCL type** — thrown when `typeof(T).IsValueType` is `true`.

| Property | Value |
|----------|-------|
| `Message` | `"NotBeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality."` |

---

## Validation Rules

| Rule | Enforced by |
|------|-------------|
| Value-type subjects are rejected | `typeof(T).IsValueType` guard at runtime |
| `null`/`null` fails the assertion | `ReferenceEquals(null, null) == true` |
| `null` subject / non-null unexpected passes | `ReferenceEquals(null, nonNull) == false` |
| Non-null subject / `null` unexpected passes | `ReferenceEquals(obj, null) == false` |
| Two distinct instances pass | `ReferenceEquals(new object(), new object()) == false` |
| Same reference fails | `ReferenceEquals(obj, obj) == true` |
