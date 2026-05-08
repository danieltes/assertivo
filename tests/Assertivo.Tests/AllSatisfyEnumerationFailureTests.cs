using Assertivo;
using Assertivo.Tests.TestUtilities;

namespace Assertivo.Tests;

public class AllSatisfyEnumerationFailureTests
{
    [Fact]
    public void AllSatisfy_WhenSourceEnumerationThrows_ShouldRethrowOriginalException()
    {
        var sourceException = new InvalidOperationException("source enumeration failed");
        IEnumerable<int> subject = new ThrowingEnumerable<int>([1, 2, 3], throwAfterMoveNextCount: 2, sourceException);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            subject.Should().AllSatisfy(value => value.Should().Be(0)));

        Assert.Same(sourceException, exception);
    }

    [Fact]
    public void AllSatisfy_WithNonAssertionInspectorException_ShouldIncludeExceptionTypeAndMessage()
    {
        IEnumerable<int> subject = [1, 2, 3];

        var exception = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy(value =>
            {
                if (value == 2)
                {
                    throw new InvalidOperationException("boom");
                }

                if (value == 3)
                {
                    value.Should().Be(0);
                }
            }));

        Assert.Contains("InvalidOperationException: boom", exception.Message);
        Assert.Contains("[1]", exception.Message);
        Assert.Contains("[2]", exception.Message);
    }
}
