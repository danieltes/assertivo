using Assertivo;

namespace Assertivo.Tests;

public class ChainingTests
{
    [Fact]
    public void BooleanAssertions_And_ChainsCorrectly()
    {
        true.Should().BeTrue().And.BeTrue();
    }

    [Fact]
    public void StringAssertions_And_ChainsCorrectly()
    {
        "hello world".Should().Contain("hello").And.Contain("world");
    }

    [Fact]
    public void StringAssertions_MultipleAnd_ChainsCorrectly()
    {
        "hello world".Should()
            .Contain("hello").And
            .Contain("world").And
            .NotContain("mars");
    }

    [Fact]
    public void NumericAssertions_And_ChainsCorrectly()
    {
        42.Should().Be(42).And.Be(42);
    }

    [Fact]
    public void ObjectAssertions_And_ChainsCorrectly()
    {
        string? value = "hello";
        value.Should<string?>().NotBeNull().And.Be("hello");
    }

    [Fact]
    public void And_EachLinkExecutesAssertion()
    {
        // Verify that the second assertion in the chain actually runs
        var ex = Assert.Throws<AssertionFailedException>(() =>
            "hello".Should().Contain("hello").And.Contain("missing"));
        Assert.Contains("\"missing\"", ex.Message);
    }

    // --- US8 full acceptance scenarios (Phase 12) ---

    [Fact]
    public void ContainSingle_Which_ChainsToPropertyShould()
    {
        IEnumerable<string> list = new List<string> { "Alice" };
        list.Should().ContainSingle().Which.Should().Contain("Ali");
    }

    [Fact]
    public void Throw_And_ChainsMultipleWithMessage()
    {
        Action act = () => throw new InvalidOperationException("detailed error message");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("detailed").And
            .WithMessage("error").And
            .WithMessage("message");
    }

    [Fact]
    public void BeOfType_Which_ChainsToShould()
    {
        object value = "hello world";
        value.Should().BeOfType<string>().Which.Should().Contain("world");
    }

    [Fact]
    public void ContainKey_Which_ChainsToShouldBe()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["age"] = 30 };
        dict.Should().ContainKey("age").Which.Should().Be(30);
    }

    [Fact]
    public async Task ThrowAsync_Which_ChainsToMessageShould()
    {
        Func<Task> act = () => throw new InvalidOperationException("async failure");
        var result = await act.Should().ThrowAsync<InvalidOperationException>();
        result.Which.Message.Should().Contain("async");
    }

    [Fact]
    public void ContainSingle_And_ChainsToHaveCount()
    {
        IEnumerable<int> list = new List<int> { 42 };
        list.Should().ContainSingle().And.HaveCount(1);
    }

    [Fact]
    public void BeOfType_And_ChainsToNotBeNull()
    {
        object value = "hello";
        value.Should().BeOfType<string>().And.NotBeNull();
    }

    [Fact]
    public void ExceptionAssertions_And_Property_ReturnsSelf()
    {
        Action act = () => throw new InvalidOperationException("test");
        var assertions = act.Should().Throw<InvalidOperationException>();
        Assert.Equal(assertions.Which, assertions.And.Which);
    }
}
