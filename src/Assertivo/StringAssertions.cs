using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo;

/// <summary>
/// Assertions for string values.
/// </summary>
public readonly struct StringAssertions
{
    internal StringAssertions(string? subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public string? Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the subject equals the <paramref name="expected"/> string.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<StringAssertions> Be(string? expected, string because = "", params object[] becauseArgs)
    {
        if (!string.Equals(Subject, expected, StringComparison.Ordinal))
        {
            MessageFormatter.Fail(
                MessageFormatter.FormatValue(expected),
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<StringAssertions>(this);
    }

    /// <summary>
    /// Asserts that the subject contains the <paramref name="expected"/> substring.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<StringAssertions> Contain(string expected, string because = "", params object[] becauseArgs)
    {
        if (Subject is null || !Subject.Contains(expected, StringComparison.Ordinal))
        {
            MessageFormatter.Fail(
                $"a string containing {MessageFormatter.FormatValue(expected)}",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<StringAssertions>(this);
    }

    /// <summary>
    /// Asserts that the subject does not contain the <paramref name="expected"/> substring.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<StringAssertions> NotContain(string expected, string because = "", params object[] becauseArgs)
    {
        if (Subject is not null && Subject.Contains(expected, StringComparison.Ordinal))
        {
            MessageFormatter.Fail(
                $"a string not containing {MessageFormatter.FormatValue(expected)}",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<StringAssertions>(this);
    }

    /// <summary>
    /// Asserts that the subject is not null or empty.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<StringAssertions> NotBeNullOrEmpty(string because = "", params object[] becauseArgs)
    {
        if (string.IsNullOrEmpty(Subject))
        {
            MessageFormatter.Fail(
                "a non-null and non-empty string",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<StringAssertions>(this);
    }

    /// <summary>
    /// Asserts that the subject is an empty string.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<StringAssertions> BeEmpty(string because = "", params object[] becauseArgs)
    {
        if (!string.Equals(Subject, string.Empty, StringComparison.Ordinal))
        {
            MessageFormatter.Fail(
                "\"\"",
                MessageFormatter.FormatValue(Subject),
                Expression, because, becauseArgs);
        }
        return new AndConstraint<StringAssertions>(this);
    }
}
