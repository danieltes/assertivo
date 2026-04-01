using System.Diagnostics;

namespace Assertivo.Exceptions;

/// <summary>
/// Assertions for synchronous actions that are expected to throw.
/// </summary>
public readonly struct ActionAssertions
{
    internal ActionAssertions(Action subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public Action Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the action throws an exception of type <typeparamref name="TException"/> or a subtype.
    /// </summary>
    /// <typeparam name="TException">The expected exception type (exact or base).</typeparam>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="ExceptionAssertions{TException}"/> for inspecting the caught exception.</returns>
    [StackTraceHidden]
    public ExceptionAssertions<TException> Throw<TException>(string because = "", params object[] becauseArgs)
        where TException : Exception
    {
        Exception? caught = null;
        try
        {
            Subject();
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        if (caught is null)
        {
            MessageFormatter.Fail(
                $"<{typeof(TException).FullName}> to be thrown",
                "no exception was thrown",
                Expression,
                because,
                becauseArgs);
        }

        if (caught is TException typed)
        {
            return new ExceptionAssertions<TException>(typed, Expression);
        }

        MessageFormatter.Fail(
            $"<{typeof(TException).FullName}>",
            $"<{caught!.GetType().FullName}>",
            Expression,
            because,
            becauseArgs);
        return default!;
    }
}
