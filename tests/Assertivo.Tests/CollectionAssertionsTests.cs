using Assertivo;
using Assertivo.Collections;

namespace Assertivo.Tests;

public class CollectionAssertionsTests
{
    [Fact]
    public void HaveCount_WithCorrectCount_Passes()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        list.Should().HaveCount(3);
    }

    [Fact]
    public void HaveCount_WithWrongCount_Fails()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        var ex = Assert.Throws<AssertionFailedException>(() => list.Should().HaveCount(5));
        Assert.Contains("5", ex.Expected);
        Assert.Contains("3", ex.Actual);
    }

    [Fact]
    public void Contain_WithExistingElement_Passes()
    {
        IEnumerable<string> list = new List<string> { "apple", "banana" };
        list.Should().Contain("apple");
    }

    [Fact]
    public void Contain_WithMissingElement_Fails()
    {
        IEnumerable<string> list = new List<string> { "apple", "banana" };
        Assert.Throws<AssertionFailedException>(() => list.Should().Contain("cherry"));
    }

    [Fact]
    public void Contain_WithCustomComparer_UsesComparer()
    {
        IEnumerable<string> list = new List<string> { "APPLE" };
        list.Should().Contain("apple", comparer: StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void BeEmpty_WithEmptyCollection_Passes()
    {
        IEnumerable<int> list = new List<int>();
        list.Should().BeEmpty();
    }

    [Fact]
    public void BeEmpty_WithNonEmptyCollection_Fails()
    {
        IEnumerable<int> list = new List<int> { 1 };
        Assert.Throws<AssertionFailedException>(() => list.Should().BeEmpty());
    }

    [Fact]
    public void ContainSingle_WithOneElement_Passes()
    {
        IEnumerable<int> list = new List<int> { 42 };
        list.Should().ContainSingle();
    }

    [Fact]
    public void ContainSingle_WithZeroElements_Fails()
    {
        IEnumerable<int> list = new List<int>();
        var ex = Assert.Throws<AssertionFailedException>(() => list.Should().ContainSingle());
        Assert.Contains("0", ex.Actual);
    }

    [Fact]
    public void ContainSingle_WithManyElements_Fails()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        var ex = Assert.Throws<AssertionFailedException>(() => list.Should().ContainSingle());
        Assert.Contains("3", ex.Actual);
    }

    [Fact]
    public void ContainSingle_Which_ExposesTypedElement()
    {
        IEnumerable<string> list = new List<string> { "Alice" };
        var result = list.Should().ContainSingle();
        Assert.Equal("Alice", result.Which);
    }

    [Fact]
    public void ContainSingle_Predicate_OneMatch_Passes()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        list.Should().ContainSingle(x => x > 2);
    }

    [Fact]
    public void ContainSingle_Predicate_ZeroMatches_Fails()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        Assert.Throws<AssertionFailedException>(() => list.Should().ContainSingle(x => x > 10));
    }

    [Fact]
    public void ContainSingle_Predicate_MultipleMatches_Fails()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        Assert.Throws<AssertionFailedException>(() => list.Should().ContainSingle(x => x > 1));
    }

    [Fact]
    public void BeEquivalentTo_Matching_Passes()
    {
        IEnumerable<string> list = new List<string> { "b", "a", "c" };
        list.Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [Fact]
    public void BeEquivalentTo_Mismatched_Fails()
    {
        IEnumerable<int> list = new List<int> { 1, 2 };
        Assert.Throws<AssertionFailedException>(() => list.Should().BeEquivalentTo(new[] { 1, 3 }));
    }

    [Fact]
    public void BeEquivalentTo_DuplicatesMatter()
    {
        IEnumerable<int> list = new List<int> { 1, 1, 2 };
        Assert.Throws<AssertionFailedException>(() =>
            list.Should().BeEquivalentTo(new[] { 1, 2, 2 }));
    }

    [Fact]
    public void BeEquivalentTo_OrderIndependent_Passes()
    {
        IEnumerable<int> list = new List<int> { 3, 1, 2 };
        list.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void AllSatisfy_AllPass_Passes()
    {
        IEnumerable<int> list = new List<int> { 2, 4, 6 };
        list.Should().AllSatisfy(x => (x % 2).Should().Be(0));
    }

    [Fact]
    public void AllSatisfy_SomeFail_FailsWithElementIndices()
    {
        IEnumerable<int> list = new List<int> { 2, 3, 4 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            list.Should().AllSatisfy(x => (x % 2).Should().Be(0)));
        Assert.Contains("[1]", ex.Message);
    }

    [Fact]
    public void AllSatisfy_EmptyCollection_PassesVacuously()
    {
        IEnumerable<int> list = new List<int>();
        list.Should().AllSatisfy(x => (x % 2).Should().Be(0));
    }

    [Fact]
    public void NullSubject_Fails()
    {
        IEnumerable<int>? list = null;
        Assert.Throws<AssertionFailedException>(() => list.Should().HaveCount(0));
    }

    [Fact]
    public void ContainSingle_Which_ChainsToShould()
    {
        IEnumerable<string> list = new List<string> { "Alice" };
        list.Should().ContainSingle().Which.Should().Contain("Ali");
    }

    [Fact]
    public void HaveCount_WithBecause_IncludesReason()
    {
        IEnumerable<int> list = new List<int> { 1 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            list.Should().HaveCount(3, because: "we need three items"));
        Assert.Contains("we need three items", ex.Message);
    }
}
