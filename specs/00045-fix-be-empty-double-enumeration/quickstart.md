# Quickstart Validation Guide: Fix BeEmpty Double Enumeration

**Branch**: `00045-fix-be-empty-double-enumeration`  
**Date**: 2026-06-09

## Prerequisites

- .NET 10 SDK
- All existing tests passing on `main` before this fix is applied

## Validation Scenarios

### 1. Existing tests continue to pass

```shell
dotnet test tests/Assertivo.Tests
```

Expected: All tests green, including `BeEmpty_WithEmptyCollection_Passes` and `BeEmpty_WithNonEmptyCollection_Fails`.

---

### 2. Non-replayable sequence — correct failure count

Add this test to `CollectionAssertionsTests.cs`:

```csharp
[Fact]
public void BeEmpty_WithNonReplayableSequence_FailsWithCorrectCount()
{
    // Arrange: an IEnumerable<int> that throws on second enumeration
    var sequence = new ThrowOnSecondEnumerationSequence<int>(new[] { 1, 2, 3 });

    // Act & Assert
    var ex = Assert.Throws<AssertionFailedException>(() => sequence.Should().BeEmpty());
    Assert.Equal("an empty collection", ex.Expected);
    Assert.Equal("a collection with 3 item(s)", ex.Actual);
}
```

Helper class (test project only):

```csharp
private sealed class ThrowOnSecondEnumerationSequence<T> : IEnumerable<T>
{
    private readonly T[] _items;
    private int _enumerationCount;

    public ThrowOnSecondEnumerationSequence(T[] items) => _items = items;

    public IEnumerator<T> GetEnumerator()
    {
        if (++_enumerationCount > 1)
            throw new InvalidOperationException("Sequence cannot be enumerated more than once.");
        return ((IEnumerable<T>)_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Expected: Test passes — no `InvalidOperationException`, failure message contains `"a collection with 3 item(s)"`.

---

### 3. Non-replayable empty sequence — passes

```csharp
[Fact]
public void BeEmpty_WithNonReplayableEmptySequence_Passes()
{
    var sequence = new ThrowOnSecondEnumerationSequence<int>(Array.Empty<int>());
    sequence.Should().BeEmpty(); // must not throw
}
```

Expected: No exception.

---

## Run Command

```shell
dotnet test tests/Assertivo.Tests --logger "console;verbosity=normal"
```
