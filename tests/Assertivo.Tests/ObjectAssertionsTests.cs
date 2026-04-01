using Assertivo;

namespace Assertivo.Tests;

public class ObjectAssertionsTests
{
    [Fact]
    public void Be_WithEqualValues_Passes()
    {
        int value = 42;
        value.Should().Be(42);
    }

    [Fact]
    public void Be_WithDifferentValues_FailsWithMessage()
    {
        int value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() => value.Should().Be(99));
        Assert.Contains("99", ex.Message);
        Assert.Contains("42", ex.Message);
        Assert.Equal("99", ex.Expected);
        Assert.Equal("42", ex.Actual);
    }

    [Fact]
    public void Be_WithCustomComparer_UsesComparer()
    {
        string value = "HELLO";
        value.Should<string>().Be("hello", StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Be_WithNull_BehavesLikeBeNull()
    {
        string? value = null;
        value.Should<string?>().Be(null);
    }

    [Fact]
    public void Be_WithNullExpected_OnNonNullSubject_Fails()
    {
        string? value = "hello";
        Assert.Throws<AssertionFailedException>(() => value.Should<string?>().Be(null));
    }

    [Fact]
    public void BeSameAs_WithSameReference_Passes()
    {
        var obj = new object();
        obj.Should().BeSameAs(obj);
    }

    [Fact]
    public void BeSameAs_WithDifferentReference_Fails()
    {
        var obj1 = new object();
        var obj2 = new object();
        Assert.Throws<AssertionFailedException>(() => obj1.Should().BeSameAs(obj2));
    }

    [Fact]
    public void BeSameAs_WithValueType_ThrowsInvalidOperationException()
    {
        int value = 42;
        Assert.Throws<InvalidOperationException>(() => value.Should<int>().BeSameAs(42));
    }

    [Fact]
    public void BeNull_WithNull_Passes()
    {
        string? value = null;
        value.Should<string?>().BeNull();
    }

    [Fact]
    public void BeNull_WithNonNull_Fails()
    {
        string? value = "hello";
        Assert.Throws<AssertionFailedException>(() => value.Should<string?>().BeNull());
    }

    [Fact]
    public void NotBeNull_WithNonNull_Passes()
    {
        string? value = "hello";
        value.Should<string?>().NotBeNull();
    }

    [Fact]
    public void NotBeNull_WithNull_Fails()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() => value.Should<string?>().NotBeNull());
    }

    [Fact]
    public void Be_WithBecauseReason_IncludesReasonInMessage()
    {
        int value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should().Be(99, because: "the answer should be correct"));
        Assert.Contains("the answer should be correct", ex.Message);
        Assert.Equal("the answer should be correct", ex.Reason);
    }

    [Fact]
    public void Be_WithBecauseArgsFormatting_FormatsReasonCorrectly()
    {
        int value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should().Be(99, because: "expected {0} to be {1}", becauseArgs: [42, 99]));
        Assert.Contains("expected 42 to be 99", ex.Message);
    }

    [Fact]
    public void Be_CapturesExpression()
    {
        int value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() => value.Should().Be(99));
        Assert.NotNull(ex.Expression);
        Assert.Contains("value", ex.Expression);
    }

    [Fact]
    public void BeOfType_ExactType_Passes()
    {
        object value = "hello";
        value.Should().BeOfType<string>();
    }

    [Fact]
    public void BeOfType_InheritanceMismatch_Fails()
    {
        object value = new ArgumentNullException();
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should().BeOfType<ArgumentException>());
        Assert.Contains("ArgumentException", ex.Expected);
        Assert.Contains("ArgumentNullException", ex.Actual);
    }

    [Fact]
    public void BeOfType_Which_ExposesTypedValue()
    {
        object value = "hello world";
        var result = value.Should().BeOfType<string>();
        result.Which.Should().Contain("world");
    }

    [Fact]
    public void BeOfType_Because_IncludesReason()
    {
        object value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should().BeOfType<string>(because: "it was supposed to be a string"));
        Assert.Contains("it was supposed to be a string", ex.Message);
    }

    [Fact]
    public void BeOfType_NullSubject_Fails()
    {
        object? value = null;
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should<object?>().BeOfType<string>());
        Assert.Contains("<null>", ex.Actual);
        Assert.Contains("String", ex.Expected);
    }
}
