# Quickstart: ThrowAsync Support for IAsyncEnumerable<T> Subjects

**Feature**: 00027-async-enumerable-throw  
**Date**: 2026-05-27

This guide shows how to use `AsyncEnumerableAssertions<T>` to assert that async
iterators throw expected exceptions during enumeration.

---

## Basic Usage — Assert an Async Iterator Throws

```csharp
// Async iterator under test
async IAsyncEnumerable<int> Source()
{
    yield return 1;
    throw new InvalidOperationException("mid-stream failure");
}

// Assert it throws the expected type
await Source().Should().ThrowAsync<InvalidOperationException>();
```

The assertion drains the enumerable until the exception is thrown. Yielded
values are discarded; only the fault state matters.

---

## Subtype Matching

```csharp
// Passes: ArgumentNullException is a subtype of ArgumentException
async IAsyncEnumerable<string> NullKeySource()
{
    yield return "ok";
    throw new ArgumentNullException("key");
}

await NullKeySource().Should().ThrowAsync<ArgumentException>();
```

---

## Chaining Further Assertions with `.Which`

After asserting the exception type, use `.Which` to inspect the caught exception:

```csharp
var result = await Source()
    .Should()
    .ThrowAsync<InvalidOperationException>();

result.Which.Message.Should().Contain("mid-stream failure");
```

Or inline (note the extra parentheses required to await before accessing `.Which`):

```csharp
(await Source()
    .Should()
    .ThrowAsync<InvalidOperationException>())
    .Which.Message.Should().Contain("mid-stream failure");
```

---

## AggregateException Unwrapping

When the enumerable faults with an `AggregateException` wrapping exactly one inner
exception, the inner exception is extracted and matched:

```csharp
async IAsyncEnumerable<int> WrappedSource()
{
    yield return 1;
    throw new AggregateException(new InvalidOperationException("inner fault"));
}

var result = await WrappedSource()
    .Should()
    .ThrowAsync<InvalidOperationException>();

// result.Which is the unwrapped InvalidOperationException
result.Which.Message.Should().Contain("inner fault");
```

---

## Contextual Failure Messages with `because`

When the assertion fails, include a reason phrase so CI output is self-documenting:

```csharp
// When source completes without throwing, the failure message will include the reason:
// "Expected source to throw InvalidOperationException but found no exception was thrown.
//  Expression: PipelineSource()
//  Because: because mid-stream failures are required by the streaming contract"
await PipelineSource()
    .Should()
    .ThrowAsync<InvalidOperationException>(
        "because mid-stream failures are required by the streaming contract");
```

---

## Failure Examples

### No exception thrown

```
Expected source to throw InvalidOperationException but found no exception was thrown.
Expression: Source()
```

### Wrong exception type

```
Expected InvalidOperationException but found ArgumentException: key cannot be null.
Expression: Source()
```

### Null subject

```
Expected source to be non-null but found source was null.
Expression: source
```

---

## What is Out of Scope

- **Assertions on yielded values** (e.g., element equality, sequence length) — use
  the collection assertion API or a future `IAsyncEnumerable<T>` value-assertion feature.
- **Cancellation token forwarding** — `ThrowAsync` drains with the default token;
  test-level timeout control is the caller's responsibility (e.g., `xUnit`'s
  `[Fact(Timeout = 5000)]` or `Task.WhenAny` with `Task.Delay`).
- **`NotThrowAsync`** — not in scope for this feature; may be added in a future
  increment.
