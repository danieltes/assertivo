using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo;

/// <summary>
/// Assertions for boolean values.
/// </summary>
public readonly struct BooleanAssertions
{
    internal BooleanAssertions(bool subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public bool Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the subject is <see langword="true"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<BooleanAssertions> BeTrue(string because = "", params object[] becauseArgs)
    {
        if (!Subject)
        {
            MessageFormatter.Fail("True", "False", Expression, because, becauseArgs);
        }
        return new AndConstraint<BooleanAssertions>(this);
    }

    /// <summary>
    /// Asserts that the subject is <see langword="false"/>.
    /// </summary>
    [StackTraceHidden]
    public AndConstraint<BooleanAssertions> BeFalse(string because = "", params object[] becauseArgs)
    {
        if (Subject)
        {
            MessageFormatter.Fail("False", "True", Expression, because, becauseArgs);
        }
        return new AndConstraint<BooleanAssertions>(this);
    }
}
