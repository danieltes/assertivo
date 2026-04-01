using Assertivo;
using Assertivo.Exceptions;

namespace Assertivo.Tests;

public class ExceptionAssertionsTests
{
    [Fact]
    public void WithMessage_ContainingSubstring_Passes()
    {
        Action act = () => throw new InvalidOperationException("something went wrong");
        act.Should().Throw<InvalidOperationException>().WithMessage("went wrong");
    }

    [Fact]
    public void WithMessage_MissingSubstring_Fails()
    {
        Action act = () => throw new InvalidOperationException("something went wrong");
        var ex = Assert.Throws<AssertionFailedException>(() =>
            act.Should().Throw<InvalidOperationException>().WithMessage("not found"));
        Assert.Contains("not found", ex.Expected);
        Assert.Contains("something went wrong", ex.Actual);
    }

    [Fact]
    public void WithMessage_EmptyString_Passes()
    {
        Action act = () => throw new InvalidOperationException("any message");
        act.Should().Throw<InvalidOperationException>().WithMessage("");
    }

    [Fact]
    public void Which_ExposesException()
    {
        Action act = () => throw new InvalidOperationException("test");
        var result = act.Should().Throw<InvalidOperationException>();
        Assert.Equal("test", result.Which.Message);
    }

    [Fact]
    public void And_ChainsBackToSelf()
    {
        Action act = () => throw new InvalidOperationException("test message");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("test").And
            .WithMessage("message");
    }

    [Fact]
    public void WithMessage_Because_IncludesReason()
    {
        Action act = () => throw new InvalidOperationException("actual");
        var ex = Assert.Throws<AssertionFailedException>(() =>
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("expected", because: "the message should match"));
        Assert.Contains("the message should match", ex.Message);
    }
}
