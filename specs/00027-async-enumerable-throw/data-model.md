# Data Model: ThrowAsync Support for IAsyncEnumerable<T> Subjects

**Feature**: 00027-async-enumerable-throw  
**Date**: 2026-05-27

---

## Overview

This feature introduces one new production type (`AsyncEnumerableAssertions<T>`)
and one new extension method overload in `ShouldExtensions`. No storage schema
changes. `ExceptionAssertions<TException>` is reused without modification.

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
| `Func<T>` | `ActionAssertions` | `ActionAssertions` | — |
| `Task?` | `TaskAssertions` | `TaskAssertions` | — |
| `Task<T>?` | `TaskAssertions` | `TaskAssertions` | — |
| `IAsyncEnumerable<T>?` | `ObjectAssertions<IAsyncEnumerable<T>>` ❌ | `AsyncEnumerableAssertions<T>` ✅ | **NEW** |
| `T[]` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `List<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IEnumerable<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IReadOnlyList<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IReadOnlyCollection<T>` | `GenericCollectionAssertions<T>` | `GenericCollectionAssertions<T>` | — |
| `IDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | `GenericDictionaryAssertions<K,V>` | — |
| `IReadOnlyDictionary<K,V>` | `GenericDictionaryAssertions<K,V>` | `GenericDictionaryAssertions<K,V>` | — |
| `T` (fallback) | `ObjectAssertions<T>` | `ObjectAssertions<T>` | — |

---

## New Type: `AsyncEnumerableAssertions<T>`

```
Namespace:     Assertivo.Exceptions
File:          src/Assertivo/Exceptions/AsyncEnumerableAssertions.cs
Kind:          readonly struct (generic, open type parameter T)
Visibility:    public
AOT-safe:      Yes (manual enumerator; no reflection)
```

### Fields / Properties

| Name | Type | Visibility | Description |
|------|------|------------|-------------|
| `Subject` | `IAsyncEnumerable<T>?` | `public` | The enumerable under test. Null is accepted; deferred to `ThrowAsync`. |
| `Expression` | `string?` | `internal` | Caller expression captured by `[CallerArgumentExpression]` in `Should()`. |

### Methods

| Method | Signature | Returns | Notes |
|--------|-----------|---------|-------|
| `ThrowAsync<TException>` | `async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs) where TException : Exception` | `Task<ExceptionAssertions<TException>>` | `[StackTraceHidden]`; manual enumeration; preserves iteration exception over `DisposeAsync` exception; applies AggregateException unwrap |

### Code Paths in `ThrowAsync`

| Path | Condition | Outcome |
|------|-----------|---------|
| Null guard | `Subject is null` | `AssertionFailedException` — `expected: "source to be non-null"`, `actual: "source was null"` |
| Iteration exception | `MoveNextAsync()` throws any `Exception` | Captured in `caught`; enumeration breaks; disposal attempted |
| DisposeAsync exception discarded | `DisposeAsync()` throws AND `caught is not null` | Disposal exception swallowed via `catch when (caught is not null)` |
| No exception | All `MoveNextAsync()` calls return `false`; no exception | `AssertionFailedException` — `expected: "source to throw {TypeName}"`, `actual: "no exception was thrown"` |
| AggregateException unwrap | `caught is AggregateException` with exactly 1 inner | `target` becomes `InnerExceptions[0]`; continues to type-check |
| Type match | `target is TException` | Returns `new ExceptionAssertions<TException>(typed, Expression)` |
| Wrong type | `target` is not `TException` or subtype | `AssertionFailedException` — `expected: "{TypeName}"`, `actual: "{ActualType}: {ActualMessage}"` |

### Cyclomatic Complexity

`ThrowAsync` has 6 distinct decision points (null, MoveNextAsync throws, DisposeAsync
filter, no exception, AggregateException unwrap, type match). Cyclomatic complexity
is 7 — within the constitution limit of 10. ✅

---

## Reused Type: `ExceptionAssertions<TException>` (unmodified)

```
Namespace:     Assertivo.Exceptions
File:          src/Assertivo/Exceptions/ExceptionAssertions.cs (unmodified)
```

`ThrowAsync<TException>` returns `new ExceptionAssertions<TException>(typed, Expression)`.
This reuse is mandated by FR-008 and ensures `.Which`, `.And`, and `.WithMessage(...)`
chaining is identical across all `ThrowAsync` entry points.

---

## New Extension Method in `ShouldExtensions`

```
Class:    Assertivo.ShouldExtensions
File:     src/Assertivo/Should.cs (modified — one new overload added)
Position: After the Task<TResult> overload; before the IReadOnlyList<T> overload
```

```
Signature:     Should<T>(this IAsyncEnumerable<T>? subject,
                   [CallerArgumentExpression(nameof(subject))] string? caller = null)
               → AsyncEnumerableAssertions<T>
Null subject:  Accepted; null forwarded into AsyncEnumerableAssertions<T>
Breaking:      None — purely additive
Resolution:    Beats ObjectAssertions<T> fallback for IAsyncEnumerable<T> subjects
               Does not interfere with IEnumerable<T> overload (distinct interface, no inheritance)
               Does not interfere with Task/Func<Task>/Action overloads (distinct types)
```

---

## Overload Resolution Notes

`IAsyncEnumerable<T>` does not implement `IEnumerable<T>`. An `IAsyncEnumerable<int>`
variable is an exact interface match for the new overload and has no implicit conversion
to `IEnumerable<T>`, `Task`, `Func<Task>`, or `Action`. Overload resolution is
unambiguous in all cases.

A type that implements both `IAsyncEnumerable<T>` and `IEnumerable<T>` would produce
a compile-time ambiguity between the two `Should<T>` overloads. This is an accepted
limitation documented in the spec's Assumptions section; no BCL type exhibits this
combination.

---

## Failure Message Format Reference

All messages produced by `ThrowAsync` route through `MessageFormatter.Fail` and
generate output in the form:

```
Expected {expected} but found {actual}.
Expression: {expression}
Because: {reason}        ← omitted when no because phrase provided
```

| Failure path | `expected` | `actual` |
|---|---|---|
| Null subject | `source to be non-null` | `source was null` |
| No exception | `source to throw InvalidOperationException` *(example)* | `no exception was thrown` |
| Wrong type | `InvalidOperationException` *(example)* | `ArgumentException: key was null` *(example)* |
