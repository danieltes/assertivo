using System.Diagnostics;

namespace Assertivo.Exceptions;

/// <summary>
/// Assertions for asynchronous functions that are expected to throw.
/// </summary>
public readonly struct AsyncFunctionAssertions
{
    internal AsyncFunctionAssertions(Func<Task> subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public Func<Task> Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the async function throws an exception of type <typeparamref name="TException"/> or a subtype when awaited.
    /// Unwraps <see cref="AggregateException"/> inner exceptions (FR-020).
    /// </summary>
    /// <typeparam name="TException">The expected exception type (exact or base).</typeparam>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>A task resolving to <see cref="ExceptionAssertions{TException}"/> for inspecting the caught exception.</returns>
    [StackTraceHidden]
    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs)
        where TException : Exception
    {
        Exception? caught = null;
        try
        {
            await Subject().ConfigureAwait(false);
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

        // Unwrap AggregateException if the inner exception matches
        var target = caught;
        if (target is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
        {
            target = aggregate.InnerExceptions[0];
        }

        if (target is TException typed)
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
