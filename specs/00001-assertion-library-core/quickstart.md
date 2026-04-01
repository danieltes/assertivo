# Quickstart: Assertivo

**Time to first assertion: < 60 seconds**

## 1. Install the package

```bash
dotnet add package Assertivo
```

## 2. Add the using directive

```csharp
using Assertivo;
```

## 3. Write your first assertion

```csharp
[Fact]
public void My_first_assertion()
{
    int result = 2 + 2;
    result.Should().Be(4);
}
```

## 4. Run

```bash
dotnet test
```

---

## More Examples

### String assertions

```csharp
string name = "Assertivo";
name.Should().Contain("Assert");
name.Should().NotBeNullOrEmpty();
```

### Collection assertions

```csharp
var items = new List<string> { "a", "b", "c" };
items.Should().HaveCount(3);
items.Should().Contain("b");
items.Should().BeEquivalentTo("c", "a", "b"); // order-independent
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

```csharp
Func<Task> act = async () => { await Task.Delay(1); throw new InvalidOperationException(); };
await act.Should().ThrowAsync<InvalidOperationException>();
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
