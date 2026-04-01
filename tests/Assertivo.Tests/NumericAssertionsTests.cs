using Assertivo;

namespace Assertivo.Tests;

public class NumericAssertionsTests
{
    [Fact]
    public void Be_WithEqualInt_Passes()
    {
        42.Should().Be(42);
    }

    [Fact]
    public void Be_WithDifferentInt_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => 42.Should().Be(99));
        Assert.Contains("99", ex.Expected);
        Assert.Contains("42", ex.Actual);
    }

    [Fact]
    public void Be_WithEqualLong_Passes()
    {
        100L.Should().Be(100L);
    }

    [Fact]
    public void Be_WithDifferentLong_Fails()
    {
        Assert.Throws<AssertionFailedException>(() => 100L.Should().Be(200L));
    }

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenEqual_Passes()
    {
        10.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenGreater_Passes()
    {
        15.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenLess_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => 10.Should().BeGreaterThanOrEqualTo(11));
        Assert.Contains("11", ex.Message);
        Assert.Contains("10", ex.Message);
    }

    [Fact]
    public void BeLessThan_WhenLess_Passes()
    {
        5.Should().BeLessThan(10);
    }

    [Fact]
    public void BeLessThan_WhenEqual_Fails()
    {
        Assert.Throws<AssertionFailedException>(() => 10.Should().BeLessThan(10));
    }

    [Fact]
    public void BeLessThan_WhenGreater_Fails()
    {
        Assert.Throws<AssertionFailedException>(() => 15.Should().BeLessThan(10));
    }

    [Fact]
    public void BeGreaterThanOrEqualTo_WithCustomComparer_UsesComparer()
    {
        // Reverse comparer: makes 5 "greater than" 10 in reversed ordering
        var reverseComparer = Comparer<int>.Create((a, b) => b.CompareTo(a));
        5.Should().BeGreaterThanOrEqualTo(10, comparer: reverseComparer);
    }

    [Fact]
    public void Be_WithBecause_IncludesReasonInMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            42.Should().Be(99, because: "the calculation should be correct"));
        Assert.Contains("the calculation should be correct", ex.Message);
    }
}
