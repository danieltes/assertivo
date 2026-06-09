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

    [Fact]
    public void NotBe_WithDifferentValues_Passes()
    {
        int value = 42;
        value.Should().NotBe(99);
    }

    [Fact]
    public void NotBe_WithEqualValues_FailsWithMessage()
    {
        int value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() => value.Should().NotBe(42));
        Assert.Equal("not 42", ex.Expected);
        Assert.Equal("42", ex.Actual);
    }

    [Fact]
    public void NotBe_WithBecauseReason_IncludesReasonInMessage()
    {
        int value = 42;
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should().NotBe(42, because: "values must differ"));
        Assert.Contains("values must differ", ex.Message);
    }

    [Fact]
    public void NotBe_ReturnsAndConstraint_AllowingChaining()
    {
        42.Should().NotBe(99).And.Be(42);
    }

    // T008 — Custom comparer tests
    [Fact]
    public void NotBe_WithCustomComparerReportingEqual_Fails()
    {
        // OrdinalIgnoreCase treats "hello" and "HELLO" as equal
        string value = "hello";
        var ex = Assert.Throws<AssertionFailedException>(() =>
            value.Should<string>().NotBe("HELLO", StringComparer.OrdinalIgnoreCase));
        Assert.Contains("not", ex.Expected);
        Assert.Equal("\"hello\"", ex.Actual);
    }

    [Fact]
    public void NotBe_WithCustomComparerReportingUnequal_Passes()
    {
        // Comparer that always reports false — identical values still pass
        var neverEqual = EqualityComparer<string>.Create((a, b) => false);
        "hello".Should<string>().NotBe("hello", neverEqual);
    }

    [Fact]
    public void NotBe_WithNullComparer_FallsBackToDefault()
    {
        42.Should<int>().NotBe(99, comparer: null);
    }

    // T011 — Null reference subject tests
    [Fact]
    public void NotBe_WithNullReferenceSubjectAndNullUnexpected_Fails()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() =>
            value.Should<string?>().NotBe(null));
    }

    // NotBeSameAs tests (T003, T004, T006, T007, T008, T009)

    [Fact]
    public void NotBeSameAs_WithDifferentReferences_Passes()
    {
        var a = new object();
        var b = new object();
        a.Should().NotBeSameAs(b).And.NotBeNull();
    }

    [Fact]
    public void NotBeSameAs_WithSameReference_Fails()
    {
        var obj = new object();
        var ex = Assert.Throws<AssertionFailedException>(() => obj.Should().NotBeSameAs(obj));
        Assert.Equal("not the same reference", ex.Expected);
        Assert.Equal("same reference", ex.Actual);
    }

    [Fact]
    public void NotBeSameAs_WithNullSubjectAndNullUnexpected_Fails()
    {
        object? subject = null;
        Assert.Throws<AssertionFailedException>(() => subject.Should<object?>().NotBeSameAs(null));
    }

    [Fact]
    public void NotBeSameAs_WithNullSubjectAndNonNullUnexpected_Passes()
    {
        object? subject = null;
        subject.Should<object?>().NotBeSameAs(new object());
    }

    [Fact]
    public void NotBeSameAs_WithBecauseReason_IncludesReasonInMessage()
    {
        var obj = new object();
        var ex = Assert.Throws<AssertionFailedException>(() =>
            obj.Should().NotBeSameAs(obj, because: "the factory must return a new instance"));
        Assert.Equal("the factory must return a new instance", ex.Reason);
        Assert.Contains("the factory must return a new instance", ex.Message);
    }

    [Fact]
    public void NotBeSameAs_WithValueType_ThrowsInvalidOperationException()
    {
        int value = 42;
        var ex = Assert.Throws<InvalidOperationException>(() => value.Should<int>().NotBeSameAs(42));
        Assert.Equal("NotBeSameAs is not meaningful for value type 'Int32'. Use Be() for value equality.", ex.Message);
    }
}
