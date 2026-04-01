using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo;

/// <summary>
/// Assertions for general object values.
/// </summary>
/// <typeparam name="T">The type of the subject.</typeparam>
public readonly struct ObjectAssertions<T>
{
    internal ObjectAssertions(T subject, string? expression)
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
    public AndConstraint<ObjectAssertions<T>> Be(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)
    {
        comparer ??= EqualityComparer<T>.Default;
        if (!comparer.Equals(Subject, expected))
        {
            MessageFormatter.Fail(
                MessageFormatter.FormatValue(expected),
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<ObjectAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the subject is the same reference as <paramref name="expected"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<ObjectAssertions<T>> BeSameAs(object? expected, string because = "", params object[] becauseArgs)
    {
        if (typeof(T).IsValueType)
        {
            throw new InvalidOperationException(
                $"BeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality.");
        }

        if (!ReferenceEquals(Subject, expected))
        {
            MessageFormatter.Fail(
                "same reference",
                "different reference",
                Expression, because, becauseArgs);
        }
        return new AndConstraint<ObjectAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the subject is <see langword="null"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<ObjectAssertions<T>> BeNull(string because = "", params object[] becauseArgs)
    {
        if (Subject is not null)
        {
            MessageFormatter.Fail(
                "<null>",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<ObjectAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the subject is not <see langword="null"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<ObjectAssertions<T>> NotBeNull(string because = "", params object[] becauseArgs)
    {
        if (Subject is null)
        {
            MessageFormatter.Fail(
                "not <null>",
                "<null>",
                Expression, because, becauseArgs);
        }
        return new AndConstraint<ObjectAssertions<T>>(this);
    }

    /// <summary>
    /// Asserts that the subject is exactly of type <typeparamref name="TTarget"/> (not a subclass).
    /// </summary>
    /// <typeparam name="TTarget">The exact expected type.</typeparam>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndWhichConstraint{TAssertions, TSubject}"/> exposing the typed subject via <c>.Which</c>.</returns>
    [StackTraceHidden]
    public AndWhichConstraint<ObjectAssertions<T>, TTarget> BeOfType<TTarget>(string because = "", params object[] becauseArgs)
    {
        if (Subject is null)
        {
            MessageFormatter.Fail(
                $"<{typeof(TTarget).FullName}>",
                "<null>",
                Expression, because, becauseArgs);
        }

        if (Subject!.GetType() != typeof(TTarget))
        {
            MessageFormatter.Fail(
                $"<{typeof(TTarget).FullName}>",
                $"<{Subject.GetType().FullName}>",
                Expression, because, becauseArgs);
        }

        return new AndWhichConstraint<ObjectAssertions<T>, TTarget>(this, (TTarget)(object)Subject!);
    }
}
