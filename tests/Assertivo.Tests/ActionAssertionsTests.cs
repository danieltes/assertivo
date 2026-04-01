using Assertivo;
using Assertivo.Exceptions;

namespace Assertivo.Tests;

public class ActionAssertionsTests
{
    [Fact]
    public void Throw_ExactType_Passes()
    {
        Action act = () => throw new InvalidOperationException("test");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Throw_Subclass_Passes()
    {
        Action act = () => throw new ArgumentNullException("param");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throw_WrongType_Fails()
    {
        Action act = () => throw new InvalidOperationException("test");
        var ex = Assert.Throws<AssertionFailedException>(() =>
            act.Should().Throw<ArgumentException>());
        Assert.Contains("ArgumentException", ex.Expected);
        Assert.Contains("InvalidOperationException", ex.Actual);
    }

    [Fact]
    public void Throw_NoThrow_Fails()
    {
        Action act = () => { };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            act.Should().Throw<InvalidOperationException>());
        Assert.Contains("InvalidOperationException", ex.Expected);
        Assert.Contains("no exception was thrown", ex.Actual);
    }

    [Fact]
    public void Throw_Which_Message_DrillDown()
    {
        Action act = () => throw new InvalidOperationException("specific error");
        var result = act.Should().Throw<InvalidOperationException>();
        Assert.Equal("specific error", result.Which.Message);
    }

    [Fact]
    public void Throw_Which_PropertyName_DrillDown()
    {
        Action act = () => throw new ArgumentNullException("myParam");
        var result = act.Should().Throw<ArgumentNullException>();
        Assert.Equal("myParam", result.Which.ParamName);
    }

    [Fact]
    public void Throw_Because_IncludesReason()
    {
        Action act = () => { };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            act.Should().Throw<InvalidOperationException>(because: "it should throw"));
        Assert.Contains("it should throw", ex.Message);
    }
}
