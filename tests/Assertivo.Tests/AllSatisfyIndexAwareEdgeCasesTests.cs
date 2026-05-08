using Assertivo;
using Assertivo.Primitives;

namespace Assertivo.Tests;

public class AllSatisfyIndexAwareEdgeCasesTests
{
    [Fact]
    public void AllSatisfy_IndexAwareNullSubject_ShouldFailThroughAssertionPipeline()
    {
        IEnumerable<int>? subject = null;

        Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy((value, index) => value.Should().Be(index)));
    }

    [Fact]
    public void AllSatisfy_IndexAwareNullInspector_ShouldThrowArgumentNullException()
    {
        IEnumerable<int> subject = [1, 2, 3];

        var exception = Assert.Throws<ArgumentNullException>(() =>
            subject.Should().AllSatisfy((Action<int, int>)null!));

        Assert.Equal("inspector", exception.ParamName);
    }

    [Fact]
    public void AllSatisfy_IndexAwareEmptyCollection_ShouldPass()
    {
        IEnumerable<int> subject = [];

        subject.Should().AllSatisfy((_, _) => { });
    }

    [Fact]
    public void AllSatisfy_IndexAwareFailure_WithBecause_ShouldIncludeReason()
    {
        IEnumerable<int> subject = [1, 2, 3];

        var exception = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy((value, index) => value.Should().Be(index), "index and value must match"));

        Assert.Contains("Because: index and value must match", exception.Message);
    }

    [Fact]
    public void AllSatisfy_IndexAwarePassingInspector_ShouldAllowChaining()
    {
        IEnumerable<int> subject = [5, 6, 7];

        AndConstraint<Collections.GenericCollectionAssertions<int>> result =
            subject.Should().AllSatisfy((value, _) => value.Should().BeGreaterThanOrEqualTo(5));

        result.And.HaveCount(3);
    }
}
