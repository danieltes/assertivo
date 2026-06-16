# Quickstart Validation Guide: Numeric Comparison Methods

**Feature**: 00050-numeric-comparisons  
**Branch**: `00050-numeric-comparisons`

## Prerequisites

- .NET 10 SDK
- Repository cloned and on branch `00050-numeric-comparisons`

## Run All Tests

```powershell
dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj
```

All existing tests must continue to pass. New tests for `BeGreaterThan` and `BeLessThanOrEqualTo` must also pass.

## Validate Each New Method

### BeGreaterThan — passing case

```csharp
5.Should().BeGreaterThan(3);  // passes
```

### BeGreaterThan — boundary (equal to threshold must fail)

```csharp
// Should throw AssertionFailedException
Assert.Throws<AssertionFailedException>(() => 3.Should().BeGreaterThan(3));
```

### BeLessThanOrEqualTo — passing cases

```csharp
3.Should().BeLessThanOrEqualTo(5);  // passes
5.Should().BeLessThanOrEqualTo(5);  // passes (equal to threshold)
```

### BeLessThanOrEqualTo — failing case

```csharp
// Should throw AssertionFailedException
Assert.Throws<AssertionFailedException>(() => 6.Should().BeLessThanOrEqualTo(5));
```

### Chaining

```csharp
42.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(100);
```

### Failure message format

```csharp
var ex = Assert.Throws<AssertionFailedException>(() => 2.Should().BeGreaterThan(3));
// ex.Message contains "a value greater than 3" and "2"
```

```csharp
var ex = Assert.Throws<AssertionFailedException>(() => 6.Should().BeLessThanOrEqualTo(5));
// ex.Message contains "a value less than or equal to 5" and "6"
```

### Because phrase

```csharp
var ex = Assert.Throws<AssertionFailedException>(
    () => 2.Should().BeGreaterThan(3, because: "the result must exceed the minimum"));
// ex.Message contains "the result must exceed the minimum"
```

## Coverage Check

Run with coverage to confirm thresholds are maintained:

```powershell
dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj /p:CollectCoverage=true /p:Threshold=93,90 /p:ThresholdType=line,branch
```

Expected: all thresholds pass.

## API Contract Reference

See [contracts/public-api.md](contracts/public-api.md) for the full method signatures and semantics.
