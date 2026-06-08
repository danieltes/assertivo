using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo.Collections;

public readonly partial struct GenericCollectionAssertions<T>
{
    /// <summary>
    /// Asserts that the collection is not empty (contains at least one element).
    /// </summary>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
    [StackTraceHidden]
    public AndConstraint<GenericCollectionAssertions<T>> NotBeEmpty(string because = "", params object[] becauseArgs)
    {
        GuardNull();
        if (!Subject!.Any())
        {
            MessageFormatter.Fail("a non-empty collection", "an empty collection", Expression, because, becauseArgs);
        }
        return new AndConstraint<GenericCollectionAssertions<T>>(this);
    }
}
