# Quickstart Validation Guide: Float/Double/Decimal Should Dispatch

**Feature**: `00049-float-double-decimal-should`  
**Date**: 2026-06-10

## Prerequisites

- .NET 10 SDK installed
- Solution builds cleanly: `dotnet build Assertivo.slnx`

## Validation Scenarios

### 1. Compile-time: New overloads resolve to NumericAssertions<T>

After implementation, the following expressions must compile without error:

```csharp
NumericAssertions<float>   _ = (1.5f).Should();
NumericAssertions<double>  _ = (3.14).Should();
NumericAssertions<decimal> _ = (9.99m).Should();
```

Previously these produced a compile error because `.Should()` returned `ObjectAssertions<T>`.

### 2. Run tests

```bash
dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj --no-build
```

**Expected**: All tests pass including the new `float`/`double`/`decimal` test cases in `NumericAssertionsTests.cs` and dispatch-type tests in `ShouldDispatchTests.cs`.

### 3. Happy-path assertions — must pass silently

```csharp
(3.14).Should().BeGreaterThanOrEqualTo(0.0);         // double
(3.14).Should().BeLessThan(4.0);                     // double
(1.5f).Should().BeGreaterThanOrEqualTo(1.0f);        // float
(1.5f).Should().BeLessThan(2.0f);                    // float
(9.99m).Should().BeGreaterThanOrEqualTo(0.00m);      // decimal
(9.99m).Should().BeLessThan(10.00m);                 // decimal
(3.14).Should().Be(3.14);                            // double equality
(9.99m).Should().NotBe(10.00m);                      // decimal inequality
```

### 4. Failure-path assertions — must throw AssertionFailedException with correct message

```csharp
// double range failure
Assert.Throws<AssertionFailedException>(() =>
    (5.0).Should().BeLessThan(1.0));
// Message must contain: "5.0", "1.0"

// float range failure
Assert.Throws<AssertionFailedException>(() =>
    (0.5f).Should().BeGreaterThanOrEqualTo(1.0f));
// Message must contain: "0.5", "1.0"

// decimal equality failure
Assert.Throws<AssertionFailedException>(() =>
    (9.99m).Should().Be(10.00m));
// Message must contain: "9.99", "10.00"
```

### 5. Caller expression is captured

```csharp
double result = 5.0;
var ex = Assert.Throws<AssertionFailedException>(() =>
    result.Should().BeLessThan(1.0));
Assert.Contains("result", ex.Message);
```

### 6. Chaining works

```csharp
(3.14).Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(4.0);
(9.99m).Should().NotBe(0.00m).And.BeLessThan(10.00m);
```

## Run coverage check

```bash
dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage
```

Expected: Coverage for `Should.cs` and `NumericAssertions.cs` remains above 90%.

## References

- Public API contract: [contracts/public-api.md](contracts/public-api.md)
- Dispatch table: [data-model.md](data-model.md)
- Spec: [spec.md](spec.md)
