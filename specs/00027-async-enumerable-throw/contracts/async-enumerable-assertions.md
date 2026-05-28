# API Contract: AsyncEnumerableAssertions<T>

**Feature**: 00027-async-enumerable-throw  
**Date**: 2026-05-27  
**Contract Type**: Public library API surface

---

## Overview

This document defines the complete public API surface introduced by this feature.
It covers the new `.Should()` dispatch overload and the `AsyncEnumerableAssertions<T>`
assertion type.

---

## Extension Method: `ShouldExtensions.Should<T>`

**File**: `src/Assertivo/Should.cs`  
**Namespace**: `Assertivo`

```csharp
/// <summary>
/// Returns an <see cref="AsyncEnumerableAssertions{T}"/> for the specified
/// async enumerable subject.
/// </summary>
public static AsyncEnumerableAssertions<T> Should<T>(
    this IAsyncEnumerable<T>? subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `subject` | `IAsyncEnumerable<T>?` | The enumerable under test. Null is accepted and deferred to `ThrowAsync`. |
| `caller` | `string?` | Automatically populated by the compiler via `[CallerArgumentExpression]`. Do not supply manually. |

### Returns

`AsyncEnumerableAssertions<T>` — a struct on which assertion methods can be chained.

### Dispatch guarantee

An `IAsyncEnumerable<T>?` subject always resolves to this overload at compile
time. It does not fall through to `ObjectAssertions<T>`. No runtime dispatch or
casting occurs.

---

## Assertion Struct: `AsyncEnumerableAssertions<T>`

**File**: `src/Assertivo/Exceptions/AsyncEnumerableAssertions.cs`  
**Namespace**: `Assertivo.Exceptions`

```csharp
/// <summary>
/// Assertions for an <see cref="IAsyncEnumerable{T}"/> subject.
/// Obtain an instance via <c>source.Should()</c>.
/// </summary>
public readonly struct AsyncEnumerableAssertions<T>
```

### Properties

| Property | Type | Visibility | Description |
|----------|------|------------|-------------|
| `Subject` | `IAsyncEnumerable<T>?` | `public` | The enumerable under test. |

### Methods

#### `ThrowAsync<TException>`

```csharp
/// <summary>
/// Asserts that the async enumerable throws an exception of type
/// <typeparamref name="TException"/> or a subtype when enumerated.
/// Unwraps <see cref="AggregateException"/> with a single inner exception.
/// </summary>
/// <typeparam name="TException">The expected exception type (exact or base).</typeparam>
/// <param name="because">An optional reason phrase for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>
/// A task resolving to <see cref="ExceptionAssertions{TException}"/> for
/// inspecting the caught exception via <c>.Which</c>.
/// </returns>
[StackTraceHidden]
public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(
    string because = "", params object[] becauseArgs)
    where TException : Exception
```

**Behaviour**:

1. If `Subject` is null → fails immediately with `"source to be non-null"` / `"source was null"`.
2. Drains the enumerable via manual `GetAsyncEnumerator()` / `MoveNextAsync()` / `DisposeAsync()` pattern with `ConfigureAwait(false)`.
3. If `DisposeAsync()` throws after an iteration exception was already caught, the disposal exception is discarded; the iteration exception is preserved (clarification §2026-05-27 Q1).
4. If no exception is thrown during enumeration → fails with `"source to throw {TypeName}"` / `"no exception was thrown"`.
5. If the caught exception is `AggregateException` with exactly one inner exception → that inner exception is extracted for type-matching.
6. If the (possibly unwrapped) exception is `TException` or a subtype → returns `ExceptionAssertions<TException>` containing the caught exception.
7. Otherwise → fails with `"{TypeName}"` / `"{ActualType}: {ActualMessage}"`.

**No `CancellationToken` parameter** — consistent with `AsyncFunctionAssertions.ThrowAsync`
and `TaskAssertions.ThrowAsync` (clarification §2026-05-27 Q2).

---

## Failure Message Formats

All failures route through `MessageFormatter.Fail` and produce messages in the form:

```
Expected {expected} but found {actual}.
Expression: {subject expression}
Because: {reason}    ← omitted when no because phrase provided
```

### Null subject

```
Expected source to be non-null but found source was null.
Expression: myEnumerable
```

### No exception thrown

```
Expected source to throw InvalidOperationException but found no exception was thrown.
Expression: Source()
```

### Wrong exception type

```
Expected InvalidOperationException but found ArgumentException: key cannot be null.
Expression: Source()
Because: because the contract forbids null keys
```

---

## Usage Examples

```csharp
// Assert throws the expected type
await Source().Should().ThrowAsync<InvalidOperationException>();

// Assert and inspect the caught exception
var result = await Source()
    .Should()
    .ThrowAsync<InvalidOperationException>();
result.Which.Message.Should().Contain("mid-stream failure");

// With contextual because phrase
await Source()
    .Should()
    .ThrowAsync<InvalidOperationException>("because mid-stream failures are required by contract");
```

---

## Compatibility Guarantees

- **Additive only**: No existing overloads are changed or removed.
- **No breaking changes**: Existing call sites compile and behave identically.
- **Chaining parity**: `.Which`, `.And`, `.WithMessage(...)` on the returned
  `ExceptionAssertions<TException>` are identical to those returned by
  `AsyncFunctionAssertions.ThrowAsync` and `TaskAssertions.ThrowAsync`.
