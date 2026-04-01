using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo.Exceptions;

/// <summary>
/// Assertions for inspecting a caught exception.
/// </summary>
/// <typeparam name="TException">The type of exception captured.</typeparam>
public readonly struct ExceptionAssertions<TException> where TException : Exception
{
    internal ExceptionAssertions(TException exception, string? expression)
    {
        Which = exception;
        Expression = expression;
    }

    /// <summary>Gets the captured exception for further inspection.</summary>
    public TException Which { get; }

    /// <summary>Gets this instance for fluent chaining.</summary>
    public ExceptionAssertions<TException> And => this;

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the exception message contains the specified substring (ordinal, case-sensitive).
    /// </summary>
    /// <param name="expectedSubstring">The substring expected in the exception message.</param>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndConstraint{T}"/> for further chaining.</returns>
    [StackTraceHidden]
    public AndConstraint<ExceptionAssertions<TException>> WithMessage(string expectedSubstring, string because = "", params object[] becauseArgs)
    {
        if (!Which.Message.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            MessageFormatter.Fail(
                $"exception message to contain \"{expectedSubstring}\"",
                $"\"{Which.Message}\"",
                Expression,
                because,
                becauseArgs);
        }

        return new AndConstraint<ExceptionAssertions<TException>>(this);
    }
}
