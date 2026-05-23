# Quickstart: Should() Entry Point for Task Subjects

**Feature**: 00026-task-should-assertions  
**Date**: 2026-05-17

This guide shows what changes for callers once the new `Task.Should()` overload is in place.

---

## Basic Usage — Assert a Task Throws

### Before (required lambda wrapping)

```csharp
// Must wrap in Func<Task> to defer execution — adds noise
Func<Task> act = () => service.DoAsync();
await act.Should().ThrowAsync<InvalidOperationException>();
```

### After (direct Task assertion)

```csharp
// Task already in hand — no lambda needed
await service.DoAsync().Should().ThrowAsync<InvalidOperationException>();
```

The assertion awaits the task and passes when it faults with
`InvalidOperationException` (or any subtype).

---

## Subtype Matching

```csharp
// Passes: ArgumentNullException is a subtype of ArgumentException
Task faultingTask = repository.FindAsync(null);
await faultingTask.Should().ThrowAsync<ArgumentException>();
```

---

## Chaining Further Assertions with `.Which`

```csharp
var result = await service.ProcessAsync("bad-input")
    .Should()
    .ThrowAsync<InvalidOperationException>();

// .Which exposes the caught exception
result.Which.Message.Should().Contain("bad-input");
```

Or inline:

```csharp
(await service.ProcessAsync("bad-input")
    .Should()
    .ThrowAsync<InvalidOperationException>())
    .Which.Message.Should().Contain("bad-input");
```

---

## AggregateException Unwrapping

When a task faults with an `AggregateException` wrapping exactly one inner
exception, the inner exception is extracted and matched:

```csharp
// Task internally faults via Task.WhenAll with a single failure
Task multiTask = RunSingleFailingTask();

var result = await multiTask.Should().ThrowAsync<InvalidOperationException>();
// result.Which is the InvalidOperationException (unwrapped from AggregateException)
result.Which.Message.Should().Contain("inner failure message");
```

---

## Contextual Failure Messages with `because`

```csharp
await service.ValidateAsync(input)
    .Should()
    .ThrowAsync<ArgumentException>("because null input is not allowed by the contract");
```

When the assertion fails, the `because` phrase appears in the
`AssertionFailedException` message:

```
Expected <System.ArgumentException> to be thrown but no exception was thrown.
Expression: service.ValidateAsync(input)
Because: because null input is not allowed by the contract
```

---

## Failure Examples

### No exception thrown

```
Expected <System.InvalidOperationException> to be thrown
but found: no exception was thrown
Expression: service.DoAsync()
```

### Wrong exception type

```
Expected <System.ArgumentNullException>
but found: <System.InvalidOperationException>: Value cannot be null. (Parameter 'key')
Expression: cache.GetAsync(null)
```

### Null task subject

```
Expected: task to be non-null
but found: task was null
Expression: service.GetPendingTaskAsync()
```

---

## Already-Captured Task Variables

The entry point works with tasks captured before the assertion line:

```csharp
Task pendingTask = service.StartOperationAsync();
// ... some other setup ...

await pendingTask.Should().ThrowAsync<TimeoutException>();
```

The task's already-recorded outcome is observed — no re-execution occurs.

---

## `Task<T>` Subjects

`Task<T>` inherits from `Task`, so the same entry point works without any cast:

```csharp
Task<string> fetchTask = api.FetchNameAsync(invalidId);
await fetchTask.Should().ThrowAsync<KeyNotFoundException>();
```

The return value of type `T` is not surfaced; only the fault state is asserted.

---

## What Is Not Covered (Out of Scope)

- **`NotThrowAsync`**: Not part of this feature. Use `Func<Task>.Should().NotThrowAsync()` for now.
- **`Task<T>` result assertion**: The `T` return value is discarded. Asserting the returned value is a separate feature.
