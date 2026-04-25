using Assertivo;
using Assertivo.Collections;
using Assertivo.Exceptions;

namespace Assertivo.Tests;

public class ShouldDispatchTests
{
    // ── User Story 1: IReadOnlyList<T> and IReadOnlyCollection<T> ────────────

    [Fact]
    public void Should_IReadOnlyListSubject_ReturnsGenericCollectionAssertions()
    {
        IReadOnlyList<string> subject = new List<string> { "a", "b" };
        Assert.IsType<GenericCollectionAssertions<string>>(subject.Should());
    }

    [Fact]
    public void Should_IReadOnlyCollectionSubject_ReturnsGenericCollectionAssertions()
    {
        IReadOnlyCollection<string> subject = new List<string> { "a", "b" };
        Assert.IsType<GenericCollectionAssertions<string>>(subject.Should());
    }

    [Fact]
    public void Should_IReadOnlyListSubject_HaveCount_Passes()
    {
        IReadOnlyList<string> subject = new List<string> { "a", "b", "c" };
        subject.Should().HaveCount(3);
    }

    [Fact]
    public void Should_IReadOnlyListSubject_Contain_Passes()
    {
        IReadOnlyList<string> subject = new List<string> { "apple", "banana" };
        subject.Should().Contain("apple");
    }

    [Fact]
    public void Should_IReadOnlyListSubject_BeEmpty_Passes()
    {
        IReadOnlyList<string> subject = new List<string>();
        subject.Should().BeEmpty();
    }

    [Fact]
    public void Should_IReadOnlyListSubject_ContainSingle_ProvidesWhichForChaining()
    {
        IReadOnlyList<string> subject = new List<string> { "only" };
        var which = subject.Should().ContainSingle().Which;
        Assert.Equal("only", which);
    }

    [Fact]
    public void Should_IReadOnlyListSubject_AllSatisfy_Passes()
    {
        IReadOnlyList<int> subject = new List<int> { 2, 4, 6 };
        subject.Should().AllSatisfy(x => (x % 2).Should().Be(0));
    }

    [Fact]
    public void Should_IReadOnlyListSubject_BeEquivalentTo_Passes()
    {
        IReadOnlyList<string> subject = new List<string> { "a", "b" };
        subject.Should().BeEquivalentTo(new List<string> { "b", "a" });
    }

    [Fact]
    public void Should_IReadOnlyCollectionSubject_HaveCount_Passes()
    {
        IReadOnlyCollection<string> subject = new List<string> { "x", "y" };
        subject.Should().HaveCount(2);
    }

    [Fact]
    public void Should_IReadOnlyCollectionSubject_ContainSingle_ProvidesWhichForChaining()
    {
        IReadOnlyCollection<string> subject = new List<string> { "sole" };
        var which = subject.Should().ContainSingle().Which;
        Assert.Equal("sole", which);
    }

    [Fact]
    public void Should_IReadOnlyListSubject_HaveCount_WithWrongCount_Fails()
    {
        IReadOnlyList<string> subject = new List<string> { "a", "b" };
        var ex = Assert.Throws<AssertionFailedException>(() => subject.Should().HaveCount(5));
        Assert.True(ex.Expected.Contains("5") || ex.Actual.Contains("2"));
    }

    [Fact]
    public void Should_IReadOnlyCollectionSubject_BeEmpty_WhenNotEmpty_Fails()
    {
        IReadOnlyCollection<string> subject = new List<string> { "item" };
        Assert.Throws<AssertionFailedException>(() => subject.Should().BeEmpty());
    }

    [Fact]
    public void Should_NullIReadOnlyListSubject_WrapsNullSafely()
    {
        IReadOnlyList<string>? subject = null;
        // null is accepted at call site — Should() must not throw
        var assertions = subject.Should();
        Assert.IsType<GenericCollectionAssertions<string>>(assertions);
        // Attempting a collection assertion on a null subject throws AssertionFailedException
        Assert.Throws<AssertionFailedException>(() => assertions.HaveCount(0));
    }

    [Fact]
    public void Should_NullIReadOnlyCollectionSubject_WrapsNullSafely()
    {
        IReadOnlyCollection<string>? subject = null;
        var assertions = subject.Should();
        Assert.IsType<GenericCollectionAssertions<string>>(assertions);
        Assert.Throws<AssertionFailedException>(() => assertions.HaveCount(0));
    }

    // ── User Story 2: Func<T> ────────────────────────────────────────────────

    [Fact]
    public void Should_FuncTSubject_ReturnsActionAssertions()
    {
        Func<string> subject = () => "hello";
        Assert.IsType<ActionAssertions>(subject.Should());
    }

    [Fact]
    public void Should_FuncTSubjectThatThrows_ThrowPassesAndWhichProvidesException()
    {
        Func<string> subject = () => throw new InvalidOperationException("oops");
        var result = subject.Should().Throw<InvalidOperationException>();
        Assert.Equal("oops", result.Which.Message);
    }

    [Fact]
    public void Should_FuncTSubjectThatDoesNotThrow_ThrowAssertionFails()
    {
        Func<int> subject = () => 42;
        var ex = Assert.Throws<AssertionFailedException>(
            () => subject.Should().Throw<InvalidOperationException>());
        Assert.Contains("InvalidOperationException", ex.Expected);
    }

    [Fact]
    public void Should_FuncTSubjectThatDoesNotThrow_NotThrowPasses()
    {
        Func<int> subject = () => 42;
        subject.Should().NotThrow();
    }

    [Fact]
    public void Should_NullFuncTSubject_ThrowsArgumentNullException()
    {
        Func<string>? subject = null;
        Assert.Throws<ArgumentNullException>(() => subject!.Should());
    }

    // ── User Story 3: IReadOnlyDictionary<K,V> (pre-satisfied) ──────────────

    [Fact]
    public void Should_IReadOnlyDictionarySubject_ReturnsGenericDictionaryAssertions()
    {
        IReadOnlyDictionary<string, int> subject = new Dictionary<string, int> { ["a"] = 1 };
        Assert.IsType<GenericDictionaryAssertions<string, int>>(subject.Should());
    }

    [Fact]
    public void Should_IReadOnlyDictionarySubject_ContainKey_Passes()
    {
        IReadOnlyDictionary<string, int> subject = new Dictionary<string, int> { ["key"] = 99 };
        subject.Should().ContainKey("key");
    }

    [Fact]
    public void Should_IReadOnlyDictionarySubject_ContainKey_Which_ExposesValue()
    {
        IReadOnlyDictionary<string, int> subject = new Dictionary<string, int> { ["score"] = 42 };
        var result = subject.Should().ContainKey("score");
        Assert.Equal(42, result.Which);
    }

    // ── Polish: IEnumerable<T> regression guard (FR-005) ────────────────────

    [Fact]
    public void Should_IEnumerableTSubject_StillReturnsGenericCollectionAssertions()
    {
        IEnumerable<string> subject = new List<string> { "x" };
        Assert.IsType<GenericCollectionAssertions<string>>(subject.Should());
    }

    [Fact]
    public void Should_ListSubject_ReturnsGenericCollectionAssertions()
    {
        List<string> subject = new List<string> { "a", "b" };
        Assert.IsType<GenericCollectionAssertions<string>>(subject.Should());
    }

    [Fact]
    public void Should_IEnumerableKVPSubject_ReturnsGenericDictionaryAssertions()
    {
        IEnumerable<KeyValuePair<string, int>> subject = new Dictionary<string, int> { ["x"] = 1 };
        Assert.IsType<GenericDictionaryAssertions<string, int>>(subject.Should());
    }
}
