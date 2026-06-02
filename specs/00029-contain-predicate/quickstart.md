# Quickstart: `Contain(predicate)` for Collections

**Feature**: `00029-contain-predicate`  
**Date**: 2026-06-02

Use `Contain(predicate)` when you need to assert that at least one element in a collection matches a condition.

## Basic Usage

```csharp
var orders = new[]
{
    new { Id = 1, Status = "Pending" },
    new { Id = 2, Status = "Shipped" }
};

orders.Should().Contain(o => o.Status == "Shipped");
```

This passes because at least one element matches.

## Multiple Matches Still Pass

```csharp
new[] { 2, 4, 6, 8 }.Should().Contain(x => x % 2 == 0);
```

Any single match is sufficient.

## No Match Fails with Clear Message

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    new[] { 1, 3, 5 }.Should().Contain(x => x % 2 == 0));

Assert.Contains("no element matched the predicate", ex.Actual);
```

## Empty Collection Fails

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    Array.Empty<int>().Should().Contain(x => x > 0));
```

## Null Subject Uses Standard Null Message

```csharp
IEnumerable<int>? values = null;
var ex = Assert.Throws<AssertionFailedException>(() =>
    values.Should().Contain(x => x > 0));

Assert.Equal("a collection", ex.Expected);
Assert.Equal("<null>", ex.Actual);
```

## Null Predicate Throws

```csharp
Func<int, bool>? predicate = null;

Assert.Throws<ArgumentNullException>(() =>
    new[] { 1, 2, 3 }.Should().Contain(predicate!));
```

## Chaining

```csharp
new[] { 1, 2, 3 }
    .Should()
    .Contain(x => x == 2)
    .And.HaveCount(3);
```

## With `because`

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    new[] { "Draft", "Pending" }
        .Should()
        .Contain(s => s == "Published", "because a published item is required"));

Assert.Contains("because a published item is required", ex.Message);
```

## With formatted `becauseArgs`

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    new[] { "Draft", "Pending" }
        .Should()
        .Contain(s => s == "Published", "because status must be {0}", "Published"));

Assert.Contains("because status must be Published", ex.Message);
```

## With empty or whitespace `because`

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    new[] { 1, 3, 5 }.Should().Contain(x => x % 2 == 0, "   "));

Assert.DoesNotContain("Because:", ex.Message);
```