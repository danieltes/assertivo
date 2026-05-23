# Public API Contract: Task Assertions

**Feature**: 00026-task-should-assertions  
**Date**: 2026-05-17  
**Namespace**: `Assertivo` (extension method) / `Assertivo.Exceptions` (struct)  
**Files**: `src/Assertivo/Should.cs` (modified), `src/Assertivo/Exceptions/TaskAssertions.cs` (new)

---

## New Overload in `ShouldExtensions` (Additive — No Breaking Changes)

### `Task?.Should()`

```csharp
/// <summary>Returns a <see cref="TaskAssertions"/> for the specified task subject.</summary>
public static TaskAssertions Should(
    this Task? subject,
    [CallerArgumentExpression(nameof(subject))] string? caller = null)
    => new(subject, caller);
```

| Property | Value |
|----------|-------|
| Return type | `TaskAssertions` |
| Null subject | Accepted; null is forwarded into `TaskAssertions`; null guard fires inside `ThrowAsync` |
| Breaking change | None |
| Overload priority | Beats `ObjectAssertions<T>` fallback for `Task` and `Task<T>` subjects |
| Coexistence | Does not interfere with `Func<Task>.Should()` — distinct types, no shared implicit conversion |

---

## New Type: `TaskAssertions`

```csharp
namespace Assertivo.Exceptions;

/// <summary>
/// Assertions for a <see cref="Task"/> subject that is already started.
/// Obtain an instance via <c>task.Should()</c>.
/// </summary>
public readonly struct TaskAssertions
{
    internal TaskAssertions(Task? subject, string? expression) { ... }

    /// <summary>Gets the task under test.</summary>
    public Task? Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the task faults with an exception of type
    /// <typeparamref name="TException"/> or a subtype when awaited.
    /// Unwraps <see cref="AggregateException"/> with a single inner exception.
    /// </summary>
    /// <typeparam name="TException">The expected exception type (exact or base).</typeparam>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>
    /// A task resolving to <see cref="ExceptionAssertions{TException}"/> for
    /// inspecting the caught exception via <c>.Which</c>.
    /// </returns>
    [StackTraceHidden]
    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(
        string because = "", params object[] becauseArgs)
        where TException : Exception
    { ... }
}
```

---

## `ThrowAsync<TException>` — Failure Conditions

### Null subject

```
Expected: task to be non-null
Actual:   task was null
Expression: {caller expression}
[Because: {reason} — if provided]
```

### No exception thrown (task completed successfully)

```
Expected: <{typeof(TException).FullName}> to be thrown
Actual:   no exception was thrown
Expression: {caller expression}
[Because: {reason} — if provided]
```

### Wrong exception type

```
Expected: <{typeof(TException).FullName}>
Actual:   <{target.GetType().FullName}>: {target.Message}
Expression: {caller expression}
[Because: {reason} — if provided]
```

*`target` is the unwrapped exception (inner of single-inner `AggregateException` if applicable).*

---

## `AggregateException` Unwrapping Rule

Identical to `AsyncFunctionAssertions.ThrowAsync`:

- If `caught is AggregateException agg && agg.InnerExceptions.Count == 1`:
  `target = agg.InnerExceptions[0]` — type-match and `.Which` use `target`
- Otherwise: `target = caught` — no unwrapping; `AggregateException` itself is
  type-checked against `TException`

---

## Unchanged Types

| Type | File | Status |
|------|------|--------|
| `AsyncFunctionAssertions` | `src/Assertivo/Exceptions/AsyncFunctionAssertions.cs` | Unmodified |
| `ExceptionAssertions<TException>` | `src/Assertivo/Exceptions/ExceptionAssertions.cs` | Unmodified |
| `ActionAssertions` | `src/Assertivo/Exceptions/ActionAssertions.cs` | Unmodified |
