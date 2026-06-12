# Data Model: Float/Double/Decimal Should Dispatch

**Feature**: `00049-float-double-decimal-should`  
**Date**: 2026-06-10

## Overview

This feature introduces no new types or entities. The existing `NumericAssertions<T>` already models the full assertion context for any `struct` implementing `IComparable<T>` and `IEquatable<T>`. The change is purely in the dispatch layer.

## Existing Types Used

### `NumericAssertions<T>` — unchanged

| Member | Kind | Description |
|--------|------|-------------|
| `T Subject` | Property | The value under assertion |
| `string? Expression` | Property (internal) | Caller expression captured via `[CallerArgumentExpression]` |
| `Be(T, ...)` | Method | Equality assertion |
| `NotBe(T, ...)` | Method | Inequality assertion |
| `BeGreaterThanOrEqualTo(T, ...)` | Method | Range assertion — lower bound inclusive |
| `BeLessThan(T, ...)` | Method | Range assertion — upper bound exclusive |

**Type constraint**: `T : struct, IComparable<T>, IEquatable<T>`

`float`, `double`, and `decimal` all satisfy this constraint unchanged. No new fields, methods, or validation rules are added to this type.

### `ShouldExtensions` — 3 new overloads added

The dispatch table after this feature:

| Subject type | Returns |
|---|---|
| `bool` | `BooleanAssertions` |
| `int` | `NumericAssertions<int>` |
| `long` | `NumericAssertions<long>` |
| `float` | `NumericAssertions<float>` ← **new** |
| `double` | `NumericAssertions<double>` ← **new** |
| `decimal` | `NumericAssertions<decimal>` ← **new** |
| `string?` | `StringAssertions` |
| `Action` | `ActionAssertions` |
| `IEnumerable<T>?` / collections | `GenericCollectionAssertions<T>` |
| `IDictionary<TK,TV>?` / dictionaries | `GenericDictionaryAssertions<TK,TV>` |
| `Task?` / `Task<T>?` | `TaskAssertions` / `TaskAssertions<T>` |
| `IAsyncEnumerable<T>?` | `AsyncEnumerableAssertions<T>` |
| `T` (any other) | `ObjectAssertions<T>` |

After this feature, `float`, `double`, and `decimal` are no longer routed to the generic `ObjectAssertions<T>` fallback.
