using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo.Numeric;

/// <summary>
/// Assertions for numeric values.
/// </summary>
/// <typeparam name="T">A numeric value type that supports comparison and equality.</typeparam>
public readonly struct NumericAssertions<T> where T : struct, IComparable<T>, IEquatable<T>
{
    internal NumericAssertions(T subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public T Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the subject equals the <paramref name="expected"/> value.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<NumericAssertions<T>> Be(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        comparer ??= EqualityComparer<T>.Default;
        if (!comparer.Equals(Subject, expected))
        {
            MessageFormatter.Fail(
                MessageFormatter.FormatValue(expected),
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<NumericAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the subject is greater than or equal to <paramref name="value"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<NumericAssertions<T>> BeGreaterThanOrEqualTo(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        comparer ??= Comparer<T>.Default;
        if (comparer.Compare(Subject, value) < 0)
        {
            MessageFormatter.Fail(
                $"a value greater than or equal to {MessageFormatter.FormatValue(value)}",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<NumericAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the subject is less than <paramref name="value"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<NumericAssertions<T>> BeLessThan(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        comparer ??= Comparer<T>.Default;
        if (comparer.Compare(Subject, value) >= 0)
        {
            MessageFormatter.Fail(
                $"a value less than {MessageFormatter.FormatValue(value)}",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<NumericAssertions<T>>(this);
    }
}
