using Assertivo;

namespace Assertivo.Tests;

public class MessageFormatterTests
{
    [Fact]
    public void BecauseWithArgs_FormatsCorrectly()
    {
        string? result = MessageFormatter.FormatReason("expected {0} items", [3]);
        Assert.Equal("expected 3 items", result);
    }

    [Fact]
    public void BecauseWithoutArgs_ReturnsAsIs()
    {
        string? result = MessageFormatter.FormatReason("simple reason", []);
        Assert.Equal("simple reason", result);
    }

    [Fact]
    public void EmptyBecause_ReturnsNull()
    {
        string? result = MessageFormatter.FormatReason("", []);
        Assert.Null(result);
    }

    [Fact]
    public void FormatValue_Null_ReturnsNullTag()
    {
        Assert.Equal("<null>", MessageFormatter.FormatValue(null));
    }

    [Fact]
    public void FormatValue_String_WrapsInQuotes()
    {
        Assert.Equal("\"hello\"", MessageFormatter.FormatValue("hello"));
    }

    [Fact]
    public void FormatValue_Int_ReturnsToString()
    {
        Assert.Equal("42", MessageFormatter.FormatValue(42));
    }

    [Fact]
    public void BuildMessage_WithAllFields_ContainsAllParts()
    {
        string message = MessageFormatter.BuildMessage("42", "99", "myVar", "it should match");
        Assert.Contains("Expected 42 but found 99", message);
        Assert.Contains("Expression: myVar", message);
        Assert.Contains("Because: it should match", message);
    }

    [Fact]
    public void BuildMessage_WithNullExpression_OmitsExpression()
    {
        string message = MessageFormatter.BuildMessage("42", "99", null, null);
        Assert.Contains("Expected 42 but found 99", message);
        Assert.DoesNotContain("Expression:", message);
    }

    [Fact]
    public void AssertionFailure_StoresAllProperties()
    {
        var failure = new AssertionFailure("expected", "actual", "expr", "reason", "full message");
        Assert.Equal("expected", failure.Expected);
        Assert.Equal("actual", failure.Actual);
        Assert.Equal("expr", failure.Expression);
        Assert.Equal("reason", failure.Reason);
        Assert.Equal("full message", failure.Message);
    }

    [Fact]
    public void AssertionConfiguration_DefaultBehavior_ThrowsAssertionFailedException()
    {
        var failure = new AssertionFailure("exp", "act", "expr", "reason", "msg");
        var ex = Assert.Throws<AssertionFailedException>(() =>
            AssertionConfiguration.ReportFailure(failure));
        Assert.Equal("msg", ex.Message);
        Assert.Equal("exp", ex.Expected);
        Assert.Equal("act", ex.Actual);
    }

    [Fact]
    public void AssertionConfiguration_NullDelegate_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AssertionConfiguration.ReportFailure = null!);
    }

    [Fact]
    public void CallerArgumentExpression_CapturesSubjectExpression()
    {
        int myVariable = 42;
        var ex = Assert.Throws<AssertionFailedException>(() => myVariable.Should().Be(99));
        Assert.NotNull(ex.Expression);
        Assert.Contains("myVariable", ex.Expression);
    }
}
