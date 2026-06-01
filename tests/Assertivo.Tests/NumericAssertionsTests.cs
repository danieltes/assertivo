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

    [Fact]
    public void NotBe_WithDifferentInt_Passes()
    {
        42.Should().NotBe(99);
    }

    [Fact]
    public void NotBe_WithEqualInt_FailsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => 42.Should().NotBe(42));
        Assert.Equal("not 42", ex.Expected);
        Assert.Equal("42", ex.Actual);
    }

    [Fact]
    public void NotBe_WithDifferentLong_Passes()
    {
        100L.Should().NotBe(200L);
    }

    [Fact]
    public void NotBe_WithEqualLong_FailsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => 100L.Should().NotBe(100L));
        Assert.Equal("not 100", ex.Expected);
        Assert.Equal("100", ex.Actual);
    }

    [Fact]
    public void NotBe_WithBecauseReason_IncludesReasonInMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            42.Should().NotBe(42, because: "values must differ"));
        Assert.Contains("values must differ", ex.Message);
    }

    [Fact]
    public void NotBe_ReturnsAndConstraint_AllowingChaining()
    {
        42.Should().NotBe(99).And.Be(42);
    }

    // T009 — Custom comparer tests
    [Fact]
    public void NotBe_WithCustomComparerReportingEqual_Fails()
    {
        var alwaysEqual = EqualityComparer<int>.Create((a, b) => true);
        var ex = Assert.Throws<AssertionFailedException>(() =>
            42.Should().NotBe(99, comparer: alwaysEqual));
        Assert.Equal("not 99", ex.Expected);
        Assert.Equal("42", ex.Actual);
    }

    [Fact]
    public void NotBe_WithCustomComparerReportingUnequal_Passes()
    {
        var neverEqual = EqualityComparer<int>.Create((a, b) => false);
        42.Should().NotBe(42, comparer: neverEqual);
    }

    [Fact]
    public void NotBe_WithNullComparer_FallsBackToDefault()
    {
        42.Should().NotBe(99, comparer: null);
    }
}
