using System.Collections.Generic;
using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo.Collections;

public readonly partial struct GenericCollectionAssertions<T>
{
    /// <summary>
    /// Asserts that the collection contains the same elements in the same order as
    /// <paramref name="expected"/>.
    /// </summary>
    /// <param name="expected">The expected sequence. Must not be <see langword="null"/>.</param>
    /// <param name="comparer">
    /// An optional equality comparer for elements. When <see langword="null"/>,
    /// <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="expected"/> is <see langword="null"/>.
    /// </exception>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> Equal(
        IEnumerable<T> expected,
        IEqualityComparer<T>? comparer = null,
        string because = "",
        params object[] becauseArgs)
    {
        ArgumentNullException.ThrowIfNull(expected);
        GuardNull();

        comparer ??= EqualityComparer<T>.Default;
        var actualList = Subject!.ToList();
        var expectedList = expected.ToList();

        if (actualList.Count != expectedList.Count)
        {
            MessageFormatter.Fail(
                $"collection with {expectedList.Count} element(s)",
                $"{actualList.Count} element(s)",
                Expression,
                because,
                becauseArgs);
        }

        for (int i = 0; i < expectedList.Count; i++)
        {
            if (!comparer.Equals(actualList[i], expectedList[i]))
            {
                MessageFormatter.Fail(
                    $"{MessageFormatter.FormatValue(expectedList[i])} at index {i}",
                    $"{MessageFormatter.FormatValue(actualList[i])}",
                    Expression,
                    because,
                    becauseArgs);
            }
        }

        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the collection contains exactly the provided elements in the given order.
    /// </summary>
    /// <param name="expected">The expected elements.</param>
    /// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> Equal(params T[] expected)
    {
        return Equal((IEnumerable<T>)expected);
    }
}
