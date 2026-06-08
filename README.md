# Assertivo

A fluent, strongly-typed assertion library for .NET with a `.Should()` entry-point pattern, zero-allocation happy paths, and AOT compatibility.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Target Framework](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
![Build Status](https://github.com/danieltes/assertivo/actions/workflows/dotnet.yml/badge.svg)

![Assertivo Logo](assertivo_logo_200x200.png)

## Installation

```bash
dotnet add package Assertivo
```

## Quickstart

Add the using directive:

```csharp
using Assertivo;
```

Write your first assertion:

```csharp
[Fact]
public void My_first_assertion()
{
    int result = 2 + 2;
    result.Should().Be(4);
}
```

Run your tests:

```bash
dotnet test
```

## Examples

### Boolean assertions

```csharp
bool isReady = true;
isReady.Should().BeTrue();
```

### String assertions

```csharp
string name = "Assertivo";
name.Should().Contain("Assert");
name.Should().NotBeEmpty();         // passes for any non-"" value, including null
name.Should().NotBeNullOrEmpty();   // fails on both null and ""
name.Should().NotContain("secret", "credentials must not be logged");
name.Should().NotBe("None");
```

### Numeric assertions

```csharp
int count = 10;
count.Should().BeGreaterThanOrEqualTo(5);
count.Should().BeLessThan(100);
count.Should().NotBe(0);
```

### Collection assertions

```csharp
var items = new List<string> { "a", "b", "c" };
items.Should().HaveCount(3);
items.Should().Contain("b");
items.Should().BeEquivalentTo("c", "a", "b"); // order-independent
items.Should().NotBeEmpty();

var scores = new List<int> { 10, 20, 30 };
scores.Should().AllSatisfy(score => score.Should().BeGreaterThanOrEqualTo(10));
scores.Should().AllSatisfy((score, index) => score.Should().Be((index + 1) * 10));
```

### Predicate collection containment

```csharp
var orders = new[]
{
    new { Id = 1, Status = "Pending" },
    new { Id = 2, Status = "Shipped" }
};

// Passes when at least one element matches
orders.Should().Contain(o => o.Status == "Shipped");

// Supports chaining
orders.Should().Contain(o => o.Status == "Shipped").And.HaveCount(2);
```

### Ordered sequence equality

```csharp
// Inline values (params overload)
var result = GetSortedIds();
result.Should().Equal(10, 20, 30);

// IEnumerable overload with reason
var output = pipeline.Process(input);
output.Should().Equal(expectedOutput, because: "pipeline output must be deterministic");

// Custom comparer
var names = new[] { "Alice", "BOB" };
names.Should().Equal(new[] { "alice", "bob" }, comparer: StringComparer.OrdinalIgnoreCase);
```

### Drill-down with `.Which`

```csharp
var users = new List<User> { new("Alice") };
users.Should().ContainSingle()
    .Which.Name.Should().Be("Alice");
```

### Exception assertions

```csharp
Action act = () => throw new ArgumentNullException("param");
act.Should().Throw<ArgumentNullException>()
    .Which.ParamName.Should().Be("param");
```

### Async exception assertions

Assert directly on a `Task` — no lambda wrapper required:

```csharp
Task task = repository.SaveAsync(entity);
await task.Should().ThrowAsync<DbException>();
```

Chain further assertions on the caught exception via `.Which`:

```csharp
Task<User> task = service.GetUserAsync(id);
var ex = await task.Should().ThrowAsync<NotFoundException>();
ex.Which.ResourceId.Should().Be(id);
```

Pass a `because` reason to provide context in CI output:

```csharp
await task.Should().ThrowAsync<TimeoutException>(
    "because the call must fail fast when the upstream is unavailable");
```

The `Func<Task>` entry point is still supported for cases where you need to capture the subject lazily:

```csharp
Func<Task> act = async () => { await Task.Delay(1); throw new InvalidOperationException(); };
await act.Should().ThrowAsync<InvalidOperationException>();
```

### Async iterator assertions

Assert directly on an `IAsyncEnumerable<T>` — no `await foreach`, no `try/catch`, no lambda wrapper:

```csharp
IAsyncEnumerable<int> stream = repository.StreamOrdersAsync(customerId);
await stream.Should().ThrowAsync<UnauthorizedException>();
```

Chain further assertions on the caught exception via `.Which`:

```csharp
IAsyncEnumerable<Event> events = feed.ReadAsync();
var ex = await events.Should().ThrowAsync<StreamClosedException>();
ex.Which.Reason.Should().Be("server disconnected");
```

Pass a `because` reason to provide context in CI output:

```csharp
await stream.Should().ThrowAsync<RateLimitException>(
    "because the quota for this tenant is exhausted");
```

`AggregateException` with a single inner exception is automatically unwrapped — the same behaviour as the `Task` and `Func<Task>` entry points:

```csharp
// AggregateException(InvalidOperationException) → passes as InvalidOperationException
await stream.Should().ThrowAsync<InvalidOperationException>();
```

### Custom comparers

```csharp
result.Should().Be(expected, StringComparer.OrdinalIgnoreCase);
items.Should().BeEquivalentTo(expected, myCustomComparer);
```

### `because` reasons

```csharp
logs.Should().NotContain("secret-key", "credentials must not be logged");
```

## Build

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Restore and build

```bash
dotnet build
```

### Run tests

```bash
dotnet test
```

Tests are run with code coverage (Cobertura format). Coverage output is written to `tests/Assertivo.Tests/TestResults/`.

### Run tests with coverage report

```bash
dotnet test /p:CollectCoverage=true
```

### Build the NuGet package

```bash
dotnet pack src/Assertivo/Assertivo.csproj --configuration Release
```

The `.nupkg` is placed in `src/Assertivo/bin/Release/`.

### Run benchmarks

Benchmarks require a Release build and must be run outside the test runner:

```bash
dotnet run --project tests/Assertivo.Benchmarks --configuration Release
```

Results are written to `BenchmarkDotNet.Artifacts/results/`. The happy-path `Should().Be()` benchmark measures zero heap allocation at ~0.18 ns per call.

## Packaging

Shared NuGet packaging configuration lives in [`Directory.Build.props`](Directory.Build.props) at the repository root. All projects inherit these defaults.

- **Non-packable by default** — only projects with `<IsPackable>true</IsPackable>` produce packages.
- **Opt in** — add `<IsPackable>true</IsPackable>` to a project's `.csproj` to enable packaging.
- **Override metadata** — set any packaging property (e.g. `<Description>`) in the project's `.csproj` to replace the shared default.
- **Build packages** — run `dotnet pack -c Release` at the solution root.
