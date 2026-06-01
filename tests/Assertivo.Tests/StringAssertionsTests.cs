using Assertivo;

namespace Assertivo.Tests;

public class StringAssertionsTests
{
    [Fact]
    public void Be_WithEqualStrings_Passes()
    {
        "hello".Should().Be("hello");
    }

    [Fact]
    public void Be_WithDifferentStrings_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => "hello".Should().Be("world"));
        Assert.Contains("\"world\"", ex.Expected);
        Assert.Contains("\"hello\"", ex.Actual);
    }

    [Fact]
    public void Be_WithNullSubjectAndNullExpected_Passes()
    {
        string? value = null;
        value.Should().Be(null);
    }

    [Fact]
    public void Contain_WithMatchingSubstring_Passes()
    {
        "hello world".Should().Contain("world");
    }

    [Fact]
    public void Contain_WithMissingSubstring_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => "hello world".Should().Contain("mars"));
        Assert.Contains("\"mars\"", ex.Message);
        Assert.Contains("\"hello world\"", ex.Message);
    }

    [Fact]
    public void Contain_WithNullSubject_Fails()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() => value.Should().Contain("anything"));
    }

    [Fact]
    public void NotContain_WithAbsentSubstring_Passes()
    {
        "safe content".Should().NotContain("secret");
    }

    [Fact]
    public void NotContain_WithPresentSubstring_Fails()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            "has secret inside".Should().NotContain("secret", because: "credentials must not be logged"));
        Assert.Contains("credentials must not be logged", ex.Message);
    }

    [Fact]
    public void NotContain_WithNullSubject_Passes()
    {
        string? value = null;
        value.Should().NotContain("anything");
    }

    [Fact]
    public void NotBeNullOrEmpty_WithNonEmptyString_Passes()
    {
        "hello".Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NotBeNullOrEmpty_WithNull_Fails()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() => value.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void NotBeNullOrEmpty_WithEmptyString_Fails()
    {
        Assert.Throws<AssertionFailedException>(() => "".Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void BeEmpty_WithEmptyString_Passes()
    {
        "".Should().BeEmpty();
    }

    [Fact]
    public void BeEmpty_WithNonEmptyString_Fails()
    {
        Assert.Throws<AssertionFailedException>(() => "hello".Should().BeEmpty());
    }

    [Fact]
    public void BeEmpty_WithNull_Fails()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() => value.Should().BeEmpty());
    }

    [Fact]
    public void Be_CaseSensitive_OrdinalDefault()
    {
        Assert.Throws<AssertionFailedException>(() => "Hello".Should().Be("hello"));
    }

    [Fact]
    public void Contain_CaseSensitive_OrdinalDefault()
    {
        Assert.Throws<AssertionFailedException>(() => "Hello World".Should().Contain("hello"));
    }

    [Fact]
    public void NotBe_WithDifferentStrings_Passes()
    {
        "hello".Should().NotBe("world");
    }

    [Fact]
    public void NotBe_WithEqualStrings_FailsWithMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => "hello".Should().NotBe("hello"));
        Assert.Equal("not \"hello\"", ex.Expected);
        Assert.Equal("\"hello\"", ex.Actual);
    }

    [Fact]
    public void NotBe_WithBecauseReason_IncludesReasonInMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            "hello".Should().NotBe("hello", because: "strings must differ"));
        Assert.Contains("strings must differ", ex.Message);
    }

    [Fact]
    public void NotBe_ReturnsAndConstraint_AllowingChaining()
    {
        "hello".Should().NotBe("world").And.Be("hello");
    }

    // T010 — Null handling tests
    [Fact]
    public void NotBe_WithNullSubjectAndNonNullUnexpected_Passes()
    {
        string? value = null;
        value.Should().NotBe("anything");
    }

    [Fact]
    public void NotBe_WithNullSubjectAndNullUnexpected_Fails()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() => value.Should().NotBe(null));
    }

    [Fact]
    public void NotBe_WithNonNullSubjectAndNullUnexpected_Passes()
    {
        "hello".Should().NotBe(null);
    }
}
