using Assertivo;
using Assertivo.Collections;

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
}
