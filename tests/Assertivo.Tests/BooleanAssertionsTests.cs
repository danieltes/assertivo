using Assertivo;

namespace Assertivo.Tests;

public class BooleanAssertionsTests
{
    [Fact]
    public void BeTrue_WithTrue_Passes()
    {
        true.Should().BeTrue();
    }

    [Fact]
    public void BeTrue_WithFalse_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => false.Should().BeTrue());
        Assert.Contains("True", ex.Expected);
        Assert.Contains("False", ex.Actual);
    }

    [Fact]
    public void BeFalse_WithFalse_Passes()
    {
        false.Should().BeFalse();
    }

    [Fact]
    public void BeFalse_WithTrue_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => true.Should().BeFalse());
        Assert.Contains("False", ex.Expected);
        Assert.Contains("True", ex.Actual);
    }

    [Fact]
    public void BeTrue_WithBecause_IncludesReasonInMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            false.Should().BeTrue(because: "the flag should be set"));
        Assert.Contains("the flag should be set", ex.Message);
    }

    [Fact]
    public void BeFalse_WithBecause_IncludesReasonInMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            true.Should().BeFalse(because: "the flag should be cleared"));
        Assert.Contains("the flag should be cleared", ex.Message);
    }
}
