# Quickstart: `NotBeEmpty` for Strings and Collections

**Feature**: `00030-not-be-empty`  
**Date**: 2026-06-07

Use `NotBeEmpty()` to assert that a string contains at least one character, or that a collection contains at least one element.

---

## Strings

### Non-empty string passes

```csharp
"hello".Should().NotBeEmpty();
```

### Empty string fails with clear message

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    "".Should().NotBeEmpty());

Assert.Equal("a non-empty string", ex.Expected);
Assert.Equal("\"\"", ex.Actual);
```

### Null string passes (null is not `""`)

```csharp
string? value = null;
value.Should().NotBeEmpty();
```

### Whitespace-only string passes

```csharp
"   ".Should().NotBeEmpty();   // passes — whitespace is not ""
```

### With `because` on failure

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    "".Should().NotBeEmpty(because: "response body must not be blank"));

Assert.Contains("response body must not be blank", ex.Message);
```

### Chaining after `NotBeEmpty`

```csharp
apiResponse.Should().NotBeEmpty().And.Contain("token");
```

---

## Collections

### Non-empty collection passes

```csharp
new List<int> { 1, 2, 3 }.Should().NotBeEmpty();
```

### Empty collection fails with clear message

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    new List<int>().Should().NotBeEmpty());

Assert.Equal("a non-empty collection", ex.Expected);
Assert.Equal("an empty collection", ex.Actual);
```

### Null collection fails with standard null-guard message

```csharp
IEnumerable<int>? results = null;

var ex = Assert.Throws<AssertionFailedException>(() =>
    results.Should().NotBeEmpty());

Assert.Equal("a collection", ex.Expected);
Assert.Equal("<null>", ex.Actual);
```

### With `because` on failure

```csharp
var ex = Assert.Throws<AssertionFailedException>(() =>
    new List<int>().Should().NotBeEmpty(because: "pipeline must produce results"));

Assert.Contains("pipeline must produce results", ex.Message);
```

### Chaining after `NotBeEmpty`

```csharp
results.Should().NotBeEmpty().And.HaveCount(3);
```

### Works with any sequence type

```csharp
int[] array = [42];
array.Should().NotBeEmpty();

IEnumerable<string> lazy = Enumerable.Range(1, 5).Select(x => x.ToString());
lazy.Should().NotBeEmpty();
```
