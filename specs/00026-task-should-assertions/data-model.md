# Data Model: Should() Entry Point for Task Subjects

**Feature**: 00026-task-should-assertions  
**Date**: 2026-05-17

---

## Overview

This feature introduces one new production type (`TaskAssertions`) and one new
extension method overload in `ShouldExtensions`. No storage schema changes.
`ExceptionAssertions<TException>` is reused without modification.

---

## Dispatch Table (Complete — Before vs. After)

| Subject declared type | Before | After | Change |
|-----------------------|--------|-------|--------|
| `bool` | `BooleanAssertions` | `BooleanAssertions` | — |
| `int` | `NumericAssertions<int>` | `NumericAssertions<int>` | — |
| `long` | `NumericAssertions<long>` | `NumericAssertions<long>` | — |
| `string?` | `StringAssertions` | `StringAssertions` | — |
| `Action` | `ActionAssertions` | `ActionAssertions` | — |
| `Func<Task>` | `AsyncFunctionAssertions` | `AsyncFunctionAssertions` | — |
| `Func<T>` (single param) | `ActionAssertions` | `ActionAssertions` | — |
| `Task` | `ObjectAssertions<Task>` ❌ | `TaskAssertions` ✅ | **NEW** |
| `Task<T>` | `ObjectAssertions<Task<T>>` ❌ | `TaskAssertions` ✅ | **NEW** (via inheritance) |
| `T[]` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `List<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IEnumerable<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IReadOnlyList<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IReadOnlyCollection<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | `GenericDictionaryAssertions<K,V>` | — |
| `IReadOnlyDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | `GenericDictionaryAssertions<K,V>` | — |
| `T` (fallback) | `ObjectAssertions<T>` | `ObjectAssertions<T>` | — |

---

## New Type: `TaskAssertions`

```
Namespace:     Assertivo.Exceptions
File:          src/Assertivo/Exceptions/TaskAssertions.cs
Kind:          readonly struct
Visibility:    public
AOT-safe:      Yes (no reflection)
```

### Fields / Properties

| Name | Type | Visibility | Description |
|------|------|------------|-------------|
| `Subject` | `Task?` | `public` | The task under test. Null is accepted and deferred to `ThrowAsync`. |
| `Expression` | `string?` | `internal` | Caller expression captured by `[CallerArgumentExpression]` in `Should()`. |

### Methods

| Method | Signature | Returns | Notes |
|--------|-----------|---------|-------|
| `ThrowAsync<TException>` | `async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs) where TException : Exception` | `Task<ExceptionAssertions<TException>>` | `[StackTraceHidden]`; awaits `Subject`; applies AggregateException unwrap; fails on null subject, no-throw, wrong type |

### Code Paths in `ThrowAsync`

| Path | Condition | Outcome |
|------|-----------|---------|
| Null guard | `Subject is null` | `AssertionFailedException` — `actual: "task was null"` |
| No exception | Task completes without fault | `AssertionFailedException` — `actual: "no exception was thrown"` |
| AggregateException unwrap | `caught is AggregateException` with 1 inner | `target` becomes `InnerExceptions[0]`; type-check continues |
| Type match | `target is TException` | Returns `ExceptionAssertions<TException>(typed, Expression)` |
| Wrong type | `target` is not `TException` or subtype | `AssertionFailedException` — `actual: "<FullName>: {target.Message}"` |

---

## Reused Type: `ExceptionAssertions<TException>` (unmodified)

```
Namespace:     Assertivo.Exceptions
File:          src/Assertivo/Exceptions/ExceptionAssertions.cs (unmodified)
```

`ThrowAsync<TException>` returns an instance of the existing
`ExceptionAssertions<TException>`, constructed with
`new ExceptionAssertions<TException>(typed, Expression)`. This reuse is mandated
by clarification Q1 (2026-05-17) and means the post-`ThrowAsync` chaining surface
— `.Which`, `.And`, `.WithMessage(...)` — is identical for both `Task` and
`Func<Task>` assertion paths.

---

## New Extension Method in `ShouldExtensions`

```
Class:    Assertivo.ShouldExtensions
File:     src/Assertivo/Should.cs (modified — one new overload added)
Position: After the existing Func<Task> overload
```

```
Signature:   Should(this Task? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null) → TaskAssertions
Null subject: Accepted; null forwarded into TaskAssertions struct
Breaking:    None — purely additive
Resolution:  Beats ObjectAssertions<T> fallback for Task and Task<T> subjects
             Does not interfere with Func<Task> overload (distinct type)
```

---

## Overload Resolution Notes

A subject declared as `Task<TResult>` binds to `Should(this Task? subject)` via
the `Task<TResult> : Task` implicit reference conversion. Since `Task<TResult>`
is a more specific type than `object` (the unconstrained `T` fallback), and the
implicit conversion to `Task` is more specific than the identity-converting
`T`-fallback, the `Task` overload wins deterministically.

No ambiguity exists with `Func<Task>` because `Task` and `Func<Task>` share no
implicit conversion path in either direction.
