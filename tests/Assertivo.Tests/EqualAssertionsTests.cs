using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assertivo.Collections;

namespace Assertivo.Tests;

public class EqualAssertionsTests
{
    // ── US1: Assert Two Ordered Sequences Are Equal ──────────────────────────

    [Fact]
    public void Equal_SameElementsSameOrder_Passes()
    {
        IEnumerable<int> subject = new[] { 1, 2, 3 };
        subject.Should().Equal(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Equal_SameElementsDifferentOrder_Fails()
    {
        IEnumerable<int> subject = new[] { 3, 1, 2 };
        Assert.Throws<AssertionFailedException>(() => subject.Should().Equal(new[] { 1, 2, 3 }));
    }

    [Fact]
    public void Equal_BothEmpty_Passes()
    {
        IEnumerable<int> subject = new int[0];
        subject.Should().Equal(new int[0]);
    }

    [Fact]
    public void Equal_NullSubject_Fails()
    {
        IEnumerable<int>? subject = null;
        Assert.Throws<AssertionFailedException>(() => subject.Should().Equal(new[] { 1 }));
    }

    [Fact]
    public void Equal_ReturnsAndConstraint_ForChaining()
    {
        IEnumerable<int> subject = new[] { 1, 2 };
        subject.Should().Equal(new[] { 1, 2 }).And.HaveCount(2);
    }

    // Dispatch tests (FR-012)

    [Fact]
    public void Equal_IEnumerableSubject_Passes()
    {
        IEnumerable<int> subject = new[] { 10, 20, 30 };
        subject.Should().Equal(new[] { 10, 20, 30 });
    }

    [Fact]
    public void Equal_IReadOnlyListSubject_Passes()
    {
        IReadOnlyList<int> subject = new List<int> { 10, 20, 30 };
        subject.Should().Equal(new[] { 10, 20, 30 });
    }

    [Fact]
    public void Equal_IReadOnlyCollectionSubject_Passes()
    {
        IReadOnlyCollection<int> subject = new ReadOnlyCollection<int>(new List<int> { 10, 20, 30 });
        subject.Should().Equal(new[] { 10, 20, 30 });
    }

    [Fact]
    public void Equal_ListSubject_Passes()
    {
        List<int> subject = new List<int> { 10, 20, 30 };
        subject.Should().Equal(new[] { 10, 20, 30 });
    }

    [Fact]
    public void Equal_ArraySubject_Passes()
    {
        int[] subject = new[] { 10, 20, 30 };
        subject.Should().Equal(new[] { 10, 20, 30 });
    }

    // ── US2: Diagnostic messages ─────────────────────────────────────────────

    [Fact]
    public void Equal_DifferentCounts_MessageStatesExpectedAndActualCounts()
    {
        IEnumerable<int> subject = new[] { 1, 2 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 2, 3 }));
        Assert.Contains("3 element(s)", ex.Expected);
        Assert.Contains("2 element(s)", ex.Actual);
    }

    [Fact]
    public void Equal_ElementMismatch_MessageStatesIndexAndBothValues()
    {
        IEnumerable<int> subject = new[] { 1, 99, 3 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 2, 3 }));
        Assert.Contains("index 1", ex.Expected);
        Assert.Contains("2", ex.Expected);
        Assert.Contains("99", ex.Actual);
    }

    [Fact]
    public void Equal_ElementMismatch_ReportsOnlyFirstDifferingIndex()
    {
        IEnumerable<int> subject = new[] { 10, 20, 30 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 2, 3 }));
        Assert.Contains("index 0", ex.Expected);
        Assert.DoesNotContain("index 1", ex.Expected);
        Assert.DoesNotContain("index 2", ex.Expected);
    }

    [Fact]
    public void Equal_ElementMismatch_AtLastIndex_ReportsLastIndex()
    {
        IEnumerable<int> subject = new[] { 1, 2, 99 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 2, 3 }));
        Assert.Contains("index 2", ex.Expected);
        Assert.Contains("3", ex.Expected);
        Assert.Contains("99", ex.Actual);
    }

    [Fact]
    public void Equal_NullElementInExpected_RenderedAsNullLiteral()
    {
        IEnumerable<string?> subject = new string?[] { "a", "b" };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new string?[] { "a", null }));
        Assert.Contains("<null>", ex.Expected);
    }

    [Fact]
    public void Equal_NullElementInActual_RenderedAsNullLiteral()
    {
        IEnumerable<string?> subject = new string?[] { "a", null };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new string?[] { "a", "b" }));
        Assert.Contains("<null>", ex.Actual);
    }

    [Fact]
    public void Equal_WithBecause_IncludesBecauseInMessage()
    {
        IEnumerable<int> subject = new[] { 1, 2 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 3 }, because: "order matters"));
        Assert.Contains("order matters", ex.Message);
    }

    // ── US3: Params overload ─────────────────────────────────────────────────

    [Fact]
    public void Equal_ParamsOverload_BehavesIdenticallyToEnumerableOverload()
    {
        IEnumerable<int> subject = new[] { 1, 2, 3 };

        // Pass case: both forms should pass
        subject.Should().Equal(1, 2, 3);

        // Fail case: both forms should produce same exception shape
        var exParams = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(1, 9, 3));
        var exEnum = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal((IEnumerable<int>)new[] { 1, 9, 3 }));

        Assert.Equal(exParams.Expected, exEnum.Expected);
        Assert.Equal(exParams.Actual, exEnum.Actual);
    }

    // ── US4: Custom comparer ─────────────────────────────────────────────────

    [Fact]
    public void Equal_CustomComparer_TreatsElementsAsEqualWhenComparerSaysSo_Passes()
    {
        IEnumerable<string> subject = new[] { "Foo", "BAR" };
        subject.Should().Equal(
            new[] { "foo", "bar" },
            comparer: StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equal_CustomComparer_TreatsElementsAsUnequalWhenComparerSaysSo_Fails()
    {
        IEnumerable<string> subject = new[] { "foo", "bar" };
        Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(
                new[] { "foo", "bar" },
                comparer: new NeverEqualComparer<string>()));
    }

    // ── Edge cases ───────────────────────────────────────────────────────────

    [Fact]
    public void Equal_NullExpectedArgument_ThrowsArgumentNullException()
    {
        IEnumerable<int> subject = new[] { 1 };
        Assert.Throws<ArgumentNullException>(() =>
            subject.Should().Equal(expected: null!));
    }

    [Fact]
    public void Equal_NullComparerTreatedAsDefault_DoesNotThrow()
    {
        IEnumerable<int> subject = new[] { 1, 2 };
        subject.Should().Equal(new[] { 1, 2 }, comparer: null);
    }

    [Fact]
    public void Equal_BecauseWithFormatArgs_SubstitutesCorrectly()
    {
        IEnumerable<int> subject = new[] { 1, 2 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 3 }, because: "step {0} failed", becauseArgs: new object[] { 5 }));
        Assert.Contains("step 5 failed", ex.Message);
    }

    [Fact]
    public void Equal_NonEmptySubjectVsEmptyExpected_CountMismatchFail()
    {
        IEnumerable<int> subject = new[] { 1, 2 };
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new int[0]));
        Assert.Contains("0 element(s)", ex.Expected);
        Assert.Contains("2 element(s)", ex.Actual);
    }

    [Fact]
    public void Equal_EmptySubjectVsNonEmptyExpected_CountMismatchFail()
    {
        IEnumerable<int> subject = new int[0];
        var ex = Assert.Throws<AssertionFailedException>(() =>
            subject.Should().Equal(new[] { 1, 2 }));
        Assert.Contains("2 element(s)", ex.Expected);
        Assert.Contains("0 element(s)", ex.Actual);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private sealed class NeverEqualComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y) => false;
        public int GetHashCode(T obj) => 0;
    }
}
