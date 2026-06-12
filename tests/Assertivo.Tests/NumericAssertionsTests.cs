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

    // ── T005: float happy-path range ─────────────────────────────────────────

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenFloatMeetsThreshold_Passes()
    {
        (1.5f).Should().BeGreaterThanOrEqualTo(1.0f);
    }

    [Fact]
    public void BeLessThan_WhenFloatBelowThreshold_Passes()
    {
        (0.5f).Should().BeLessThan(1.0f);
    }

    // ── T006: double happy-path range ────────────────────────────────────────

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenDoubleMeetsThreshold_Passes()
    {
        (3.14).Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void BeLessThan_WhenDoubleBelowThreshold_Passes()
    {
        (0.5).Should().BeLessThan(1.0);
    }

    // ── T007: decimal happy-path range ───────────────────────────────────────

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenDecimalMeetsThreshold_Passes()
    {
        (9.99m).Should().BeGreaterThanOrEqualTo(0.00m);
    }

    [Fact]
    public void BeLessThan_WhenDecimalBelowThreshold_Passes()
    {
        (9.99m).Should().BeLessThan(10.00m);
    }

    // ── T008: float failure-path range ───────────────────────────────────────

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenFloatBelowThreshold_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (0.5f).Should().BeGreaterThanOrEqualTo(1.0f));
        Assert.Contains("0.5", ex.Message);
        Assert.Contains("1", ex.Message);
    }

    [Fact]
    public void BeLessThan_WhenFloatAtOrAboveThreshold_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (5.0f).Should().BeLessThan(1.0f));
        Assert.Contains("5", ex.Message);
        Assert.Contains("1", ex.Message);
    }

    // ── T009: double failure-path range ──────────────────────────────────────

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenDoubleBelowThreshold_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (0.5).Should().BeGreaterThanOrEqualTo(1.0));
        Assert.Contains("0.5", ex.Message);
        Assert.Contains("1", ex.Message);
    }

    [Fact]
    public void BeLessThan_WhenDoubleAtOrAboveThreshold_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (5.0).Should().BeLessThan(1.0));
        Assert.Contains("5", ex.Message);
        Assert.Contains("1", ex.Message);
    }

    // ── T010: decimal failure-path range ─────────────────────────────────────

    [Fact]
    public void BeGreaterThanOrEqualTo_WhenDecimalBelowThreshold_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (0.5m).Should().BeGreaterThanOrEqualTo(1.0m));
        Assert.Contains("0.5", ex.Message);
        Assert.Contains("1", ex.Message);
    }

    [Fact]
    public void BeLessThan_WhenDecimalAtOrAboveThreshold_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (5.0m).Should().BeLessThan(1.0m));
        Assert.Contains("5", ex.Message);
        Assert.Contains("1", ex.Message);
    }

    // ── T011: caller expression capture ──────────────────────────────────────

    [Fact]
    public void BeLessThan_FailureMessage_ContainsCallerExpression_ForDouble()
    {
        double result = 5.0;
        var ex = Assert.Throws<AssertionFailedException>(() => result.Should().BeLessThan(1.0));
        Assert.Contains("result", ex.Message);
    }

    [Fact]
    public void BeLessThan_FailureMessage_ContainsCallerExpression_ForFloat()
    {
        float result = 5.0f;
        var ex = Assert.Throws<AssertionFailedException>(() => result.Should().BeLessThan(1.0f));
        Assert.Contains("result", ex.Message);
    }

    [Fact]
    public void BeLessThan_FailureMessage_ContainsCallerExpression_ForDecimal()
    {
        decimal result = 5.0m;
        var ex = Assert.Throws<AssertionFailedException>(() => result.Should().BeLessThan(1.0m));
        Assert.Contains("result", ex.Message);
    }

    // ── T012: float Be/NotBe happy-path ──────────────────────────────────────

    [Fact]
    public void Be_WhenFloatValuesAreEqual_Passes()
    {
        (1.5f).Should().Be(1.5f);
    }

    [Fact]
    public void NotBe_WhenFloatValuesDiffer_Passes()
    {
        (1.0f).Should().NotBe(2.0f);
    }

    // ── T013: double Be/NotBe happy-path ─────────────────────────────────────

    [Fact]
    public void Be_WhenDoubleValuesAreEqual_Passes()
    {
        (3.14).Should().Be(3.14);
    }

    [Fact]
    public void NotBe_WhenDoubleValuesDiffer_Passes()
    {
        (1.0).Should().NotBe(2.0);
    }

    // ── T014: decimal Be/NotBe happy-path ────────────────────────────────────

    [Fact]
    public void Be_WhenDecimalValuesAreEqual_Passes()
    {
        (100.00m).Should().Be(100.00m);
    }

    [Fact]
    public void NotBe_WhenDecimalValuesDiffer_Passes()
    {
        (9.99m).Should().NotBe(10.00m);
    }

    // ── T015: Be failure paths, all 3 types ──────────────────────────────────

    [Fact]
    public void Be_WhenFloatValuesDiffer_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (1.5f).Should().Be(2.5f));
        Assert.Contains("2.5", ex.Expected);
        Assert.Contains("1.5", ex.Actual);
    }

    [Fact]
    public void Be_WhenDoubleValuesDiffer_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (1.5).Should().Be(2.5));
        Assert.Contains("2.5", ex.Expected);
        Assert.Contains("1.5", ex.Actual);
    }

    [Fact]
    public void Be_WhenDecimalValuesDiffer_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (1.5m).Should().Be(2.5m));
        Assert.Contains("2.5", ex.Expected);
        Assert.Contains("1.5", ex.Actual);
    }

    // ── T021: NotBe failure paths, all 3 types ───────────────────────────────

    [Fact]
    public void NotBe_WhenFloatValuesAreEqual_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (1.5f).Should().NotBe(1.5f));
        Assert.Contains("1.5", ex.Actual);
    }

    [Fact]
    public void NotBe_WhenDoubleValuesAreEqual_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (1.5).Should().NotBe(1.5));
        Assert.Contains("1.5", ex.Actual);
    }

    [Fact]
    public void NotBe_WhenDecimalValuesAreEqual_ThrowsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => (1.5m).Should().NotBe(1.5m));
        Assert.Contains("1.5", ex.Actual);
    }

    // ── T017: And-chaining, all 3 types ──────────────────────────────────────

    [Fact]
    public void Should_OnDouble_SupportsAndChaining()
    {
        (3.14).Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(4.0);
    }

    [Fact]
    public void Should_OnFloat_SupportsAndChaining()
    {
        (1.5f).Should().BeGreaterThanOrEqualTo(1.0f).And.BeLessThan(2.0f);
    }

    [Fact]
    public void Should_OnDecimal_SupportsAndChaining()
    {
        (9.99m).Should().NotBe(0.00m).And.BeLessThan(10.00m);
    }
}
