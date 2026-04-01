using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo.Collections;

/// <summary>
/// Assertions for generic collections implementing <see cref="IEnumerable{T}"/>.
/// </summary>
/// <typeparam name="T">The element type of the collection.</typeparam>
public readonly struct GenericCollectionAssertions<T>
{
    internal GenericCollectionAssertions(IEnumerable<T>? subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public IEnumerable<T>? Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the collection has exactly <paramref name="expected"/> elements.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> HaveCount(int expected, string because = "", params object[] becauseArgs)
    {
        GuardNull();
        int actual = Subject!.Count();
        if (actual != expected)
        {
            MessageFormatter.Fail(
                $"{expected} item(s)",
                $"{actual} item(s)",
                Expression, because, becauseArgs);
        }
        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the collection contains exactly one element.
    /// </summary>
    [StackTraceHidden]
    public AndWhichConstraint<GenericCollectionAssertions<T>, T> ContainSingle(string because = "", params object[] becauseArgs)
    {
        GuardNull();
        var list = Subject!.ToList();
        if (list.Count != 1)
        {
            MessageFormatter.Fail(
                "exactly 1 element",
                $"{list.Count} element(s)",
                Expression, because, becauseArgs);
        }
        return new AndWhichConstraint<GenericCollectionAssertions<T>, T>(this, list[0]);
    }

    /// <summary>
    /// Asserts that exactly one element matches the <paramref name="predicate"/>.
    /// </summary>
    [StackTraceHidden]
    public AndWhichConstraint<GenericCollectionAssertions<T>, T> ContainSingle(Func<T, bool> predicate, string because = "", params object[] becauseArgs)
    {
        GuardNull();
        var matches = Subject!.Where(predicate).ToList();
        if (matches.Count != 1)
        {
            MessageFormatter.Fail(
                "exactly 1 matching element",
                $"{matches.Count} matching element(s)",
                Expression, because, becauseArgs);
        }
        return new AndWhichConstraint<GenericCollectionAssertions<T>, T>(this, matches.Count == 1 ? matches[0] : default!);
    }

    /// <summary>
    /// Asserts that the collection contains the <paramref name="expected"/> element.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> Contain(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        GuardNull();
        comparer ??= EqualityComparer<T>.Default;
        if (!Subject!.Contains(expected, comparer))
        {
            MessageFormatter.Fail(
                $"a collection containing {MessageFormatter.FormatValue(expected)}",
                "the collection did not contain it",
                Expression, because, becauseArgs);
        }
        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the collection is empty.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> BeEmpty(string because = "", params object[] becauseArgs)
    {
        GuardNull();
        if (Subject!.Any())
        {
            MessageFormatter.Fail(
                "an empty collection",
                $"a collection with {Subject!.Count()} item(s)",
                Expression, because, becauseArgs);
        }
        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the collection is equivalent to <paramref name="expected"/> (order-independent, frequency-aware).
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> BeEquivalentTo(IEnumerable<T> expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        GuardNull();
        comparer ??= EqualityComparer<T>.Default;
        if (!AreEquivalent(Subject!, expected, comparer))
        {
            MessageFormatter.Fail(
                $"a collection equivalent to {FormatCollection(expected)}",
                FormatCollection(Subject!),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the collection is equivalent to <paramref name="expected"/> array (order-independent, frequency-aware).
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> BeEquivalentTo(T[] expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        return BeEquivalentTo((IEnumerable<T>)expected, comparer, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that all elements in the collection satisfy the <paramref name="inspector"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> AllSatisfy(Action<T> inspector, string because = "", params object[] becauseArgs)
    {
        GuardNull();
        var failures = new List<(int Index, Exception Error)>();
        int index = 0;
        foreach (var item in Subject!)
        {
            try
            {
                inspector(item);
            }
            catch (Exception ex)
            {
                failures.Add((index, ex));
            }
            index++;
        }

        if (failures.Count > 0)
        {
            var failureMessages = string.Join("; ", failures.Select(f => $"[{f.Index}]: {f.Error.Message}"));
            MessageFormatter.Fail(
                "all elements to satisfy the inspector",
                $"{failures.Count} element(s) failed: {failureMessages}",
                Expression, because, becauseArgs);
        }
        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }

    [StackTraceHidden]
    private void GuardNull()
    {
        if (Subject is null)
        {
            MessageFormatter.Fail(
                "a collection",
                "<null>",
                Expression, "", []);
        }
    }

    private static bool AreEquivalent(IEnumerable<T> actual, IEnumerable<T> expected, IEqualityComparer<T> comparer)
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();

        if (expectedList.Count != actualList.Count)
            return false;

        var remaining = new List<T>(expectedList);
        foreach (var item in actualList)
        {
            int idx = -1;
            for (int i = 0; i < remaining.Count; i++)
            {
                if (comparer.Equals(item, remaining[i]))
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0) return false;
            remaining.RemoveAt(idx);
        }
        return remaining.Count == 0;
    }

    private static string FormatCollection(IEnumerable<T> collection)
    {
        var items = collection.Select(i => MessageFormatter.FormatValue(i));
        return $"[{string.Join(", ", items)}]";
    }
}
