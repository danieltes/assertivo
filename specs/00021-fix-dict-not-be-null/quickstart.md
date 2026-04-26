# Quickstart: Collection and Dictionary Null-Guard Assertions

**Branch**: `00021-fix-dict-not-be-null`  
**Date**: 2026-04-26

---

## What changed

`GenericDictionaryAssertions<TKey, TValue>` and `GenericCollectionAssertions<T>` now expose `BeNull()` and `NotBeNull()`, matching the same methods already available on `ObjectAssertions<T>`.

Before this change, calling `.Should().NotBeNull()` on a dictionary or collection produced a compiler error. After this change, it compiles, runs, and chains.

---

## Dictionary — common patterns

```csharp
using Assertivo;
using Assertivo.Collections;

// 1. Assert that a dictionary is not null
IReadOnlyDictionary<string, int>? config = LoadConfig();
config.Should().NotBeNull();

// 2. Chain a null-guard directly into a content assertion
IReadOnlyDictionary<string, int>? settings = GetSettings();
settings.Should().NotBeNull().And.ContainKey("timeout");

// 3. Assert that a dictionary IS null
IDictionary<string, int>? cleared = null;
cleared.Should().BeNull();

// 4. Include a reason in the failure message
IDictionary<string, int>? missing = null;
// Failure: "Expected not <null> but found <null>.\nBecause: settings must be loaded before use"
missing.Should().NotBeNull("settings must be loaded before use");
```

---

## Collection — common patterns

```csharp
// 1. Assert that a collection is not null
IEnumerable<string>? tags = GetTags();
tags.Should().NotBeNull();

// 2. Chain a null-guard into a count assertion
IEnumerable<int>? ids = FetchIds();
ids.Should().NotBeNull().And.HaveCount(3);

// 3. Assert that a collection IS null
IEnumerable<string>? empty = null;
empty.Should().BeNull();

// 4. Include a reason
IEnumerable<int>? results = null;
// Failure: "Expected not <null> but found <null>.\nBecause: results must be populated"
results.Should().NotBeNull("results must be populated");
```

---

## Failure message examples

**`NotBeNull()` on a null dictionary/collection**:
```
Expected not <null> but found <null>.
Expression: config
```

**`NotBeNull()` with a reason**:
```
Expected not <null> but found <null>.
Expression: settings
Because: settings must be loaded before use
```

**`BeNull()` on a non-null dictionary**:
```
Expected <null> but found System.Collections.Generic.Dictionary`2[System.String,System.Int32].
Expression: dict
```

---

## Testing the new methods

Add to `DictionaryAssertionsTests.cs`:

```csharp
[Fact]
public void NotBeNull_NonNullDictionary_Passes()
{
    IReadOnlyDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
    dict.Should().NotBeNull();
}

[Fact]
public void NotBeNull_NullDictionary_Fails()
{
    IReadOnlyDictionary<string, int>? dict = null;
    var ex = Assert.Throws<AssertionFailedException>(() => dict.Should().NotBeNull());
    Assert.Equal("not <null>", ex.Expected);
    Assert.Equal("<null>", ex.Actual);
}

[Fact]
public void NotBeNull_Chaining_ContainKey()
{
    IReadOnlyDictionary<string, int> dict = new Dictionary<string, int> { ["key"] = 99 };
    dict.Should().NotBeNull().And.ContainKey("key");
}

[Fact]
public void NotBeNull_WithBecause_IncludesReasonInMessage()
{
    IReadOnlyDictionary<string, int>? dict = null;
    var ex = Assert.Throws<AssertionFailedException>(
        () => dict.Should().NotBeNull("the config must be loaded"));
    Assert.Contains("the config must be loaded", ex.Message);
}

[Fact]
public void BeNull_NullDictionary_Passes()
{
    IDictionary<string, int>? dict = null;
    dict.Should().BeNull();
}

[Fact]
public void BeNull_NonNullDictionary_Fails()
{
    IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
    var ex = Assert.Throws<AssertionFailedException>(() => dict.Should().BeNull());
    Assert.Equal("<null>", ex.Expected);
}
```

Add equivalent tests to `CollectionAssertionsTests.cs` using `IEnumerable<int>` / `List<int>`.
