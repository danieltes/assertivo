using Assertivo;
using Assertivo.Collections;
using Assertivo.Primitives;

namespace Assertivo.Tests;

public class DictionaryAssertionsTests
{
    [Fact]
    public void ContainKey_ExistingKey_Passes()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        dict.Should().ContainKey("a");
    }

    [Fact]
    public void ContainKey_MissingKey_Fails()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var ex = Assert.Throws<AssertionFailedException>(() => dict.Should().ContainKey("c"));
        Assert.Contains("\"c\"", ex.Expected);
        Assert.Contains("\"a\"", ex.Actual);
        Assert.Contains("\"b\"", ex.Actual);
    }

    [Fact]
    public void ContainKey_ReadOnlyDictionary_Passes()
    {
        IReadOnlyDictionary<int, string> dict = new Dictionary<int, string> { [1] = "one", [2] = "two" };
        dict.Should().ContainKey(1);
    }

    [Fact]
    public void ContainKey_ReadOnlyDictionary_MissingKey_Fails()
    {
        IReadOnlyDictionary<int, string> dict = new Dictionary<int, string> { [1] = "one" };
        Assert.Throws<AssertionFailedException>(() => dict.Should().ContainKey(99));
    }

    [Fact]
    public void ContainKey_FailureMessage_ShowsAvailableKeys()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        var ex = Assert.Throws<AssertionFailedException>(() => dict.Should().ContainKey("z"));
        Assert.Contains("\"x\"", ex.Actual);
        Assert.Contains("\"y\"", ex.Actual);
    }

    [Fact]
    public void ContainKey_Which_ExposesValue()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["key"] = 42 };
        var result = dict.Should().ContainKey("key");
        Assert.Equal(42, result.Which);
    }

    [Fact]
    public void ContainKey_NullSubject_Fails()
    {
        IEnumerable<KeyValuePair<string, int>>? dict = null;
        Assert.Throws<AssertionFailedException>(() => dict.Should().ContainKey("any"));
    }

    [Fact]
    public void ContainKey_Because_IncludesReason()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            dict.Should().ContainKey("missing", because: "it was expected"));
        Assert.Contains("it was expected", ex.Message);
    }

    // T003 (SC-002)
    [Fact]
    public void NotBeNull_NonNullDictionary_Passes()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        dict.Should().NotBeNull();
    }

    // T004 (SC-003)
    [Fact]
    public void NotBeNull_NullDictionary_Fails()
    {
        IReadOnlyDictionary<string, int>? dict = null;
        var ex = Assert.Throws<AssertionFailedException>(() => dict.Should().NotBeNull());
        Assert.Equal("not <null>", ex.Expected);
        Assert.Equal("<null>", ex.Actual);
    }

    // T005 (SC-004)
    [Fact]
    public void NotBeNull_Chaining_ContainKey()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int> { ["key"] = 99 };
        dict.Should().NotBeNull().And.ContainKey("key");
    }

    // T006 (SC-005)
    [Fact]
    public void BeNull_NullDictionary_Passes()
    {
        IDictionary<string, int>? dict = null;
        dict.Should().BeNull();
    }

    // T007 (SC-006)
    [Fact]
    public void BeNull_NonNullDictionary_Fails()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var ex = Assert.Throws<AssertionFailedException>(() => dict.Should().BeNull());
        Assert.Equal("<null>", ex.Expected);
    }

    // T008
    [Fact]
    public void BeNull_NullDictionary_Chaining()
    {
        IDictionary<string, int>? dict = null;
        AndConstraint<GenericDictionaryAssertions<string, int>> result = dict.Should().BeNull();
        _ = result.And;
    }

    // T009 (US3 scenario 1)
    [Fact]
    public void NotBeNull_NullDictionary_WithBecause_IncludesReasonInMessage()
    {
        IReadOnlyDictionary<string, int>? dict = null;
        var ex = Assert.Throws<AssertionFailedException>(() =>
            dict.Should().NotBeNull("the config must be loaded"));
        Assert.Contains("the config must be loaded", ex.Message);
    }

    // T022 (US3 scenario 3)
    [Fact]
    public void BeNull_NonNullDictionary_WithBecause_IncludesReasonInMessage()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            dict.Should().BeNull("the config must be initialised"));
        Assert.Contains("the config must be initialised", ex.Message);
    }

    // T024 (FR-008 concrete Dictionary<K,V>)
    [Fact]
    public void NotBeNull_NonNullConcreteDictionary_Passes()
    {
        var dict = new Dictionary<string, int> { ["x"] = 1 };
        dict.Should().NotBeNull();
    }

    // T025 (FR-008 IEnumerable<KeyValuePair<K,V>> overload)
    [Fact]
    public void NotBeNull_IEnumerableKeyValuePair_Passes()
    {
        IEnumerable<KeyValuePair<string, int>> dict =
            new List<KeyValuePair<string, int>> { new("a", 1) };
        dict.Should().NotBeNull();
    }
}
