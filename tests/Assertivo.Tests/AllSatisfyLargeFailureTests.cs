using Assertivo;
using Assertivo.Tests.TestUtilities;

namespace Assertivo.Tests;

public class AllSatisfyLargeFailureTests
{
    [Fact]
    public void AllSatisfy_WithFiftyFailures_ShouldIncludeDetailsForAllFifty()
    {
        var subject = Enumerable.Range(0, 50).ToArray();

        var exception = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy(value => value.Should().Be(999)));

        Assert.Contains("50 element(s) failed", exception.Message);
        Assert.Contains("[49]:", exception.Message);
        Assert.DoesNotContain("showing first 50 failure(s)", exception.Message);
    }

    [Fact]
    public void AllSatisfy_WithFiftyOneFailures_ShouldTruncateDetailsAfterFifty()
    {
        var subject = Enumerable.Range(0, 51).ToArray();

        var exception = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy(value => value.Should().Be(999)));

        Assert.Contains("51 element(s) failed", exception.Message);
        Assert.Contains("[49]:", exception.Message);
        Assert.DoesNotContain("[50]:", exception.Message);
        Assert.Contains("showing first 50 failure(s)", exception.Message);
    }

    [Fact]
    public void AllSatisfy_WithHundredFailures_ShouldRenderExplicitFailingIndices()
    {
        var subject = Enumerable.Range(0, 100).ToArray();

        var exception = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy(value => value.Should().Be(999)));

        var parsed = FailingIndexParser.Parse(exception.Message);
        Assert.Equal(100, parsed.Count);
        Assert.Equal(0, parsed[0]);
        Assert.Equal(99, parsed[^1]);
        Assert.DoesNotContain("0-99", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AllSatisfy_WithHundredOneFailures_ShouldRenderCompressedFailingIndices()
    {
        var subject = Enumerable.Range(0, 101).ToArray();

        var exception = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy(value => value.Should().Be(999)));

        Assert.Contains("Failing indices: [0-100]", exception.Message);

        var parsed = FailingIndexParser.Parse(exception.Message);
        Assert.Equal(101, parsed.Count);
        Assert.Equal(0, parsed[0]);
        Assert.Equal(100, parsed[^1]);
    }
}
