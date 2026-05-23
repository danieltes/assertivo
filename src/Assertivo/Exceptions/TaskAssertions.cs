using System.Diagnostics;

namespace Assertivo.Exceptions;

/// <summary>
/// Assertions for a <see cref="Task"/> subject that is already started.
/// Obtain an instance via <c>task.Should()</c>.
/// </summary>
public readonly struct TaskAssertions
{
    internal TaskAssertions(Task? subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the task under test.</summary>
    public Task? Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the task faults with an exception of type
    /// <typeparamref name="TException"/> or a subtype when awaited.
    /// Unwraps <see cref="AggregateException"/> with a single inner exception.
    /// </summary>
    /// <typeparam name="TException">The expected exception type (exact or base).</typeparam>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>
    /// A task resolving to <see cref="ExceptionAssertions{TException}"/> for
    /// inspecting the caught exception via <c>.Which</c>.
    /// </returns>
    [StackTraceHidden]
    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(
        string because = "", params object[] becauseArgs)
        where TException : Exception
    {
        if (Subject is null)
            MessageFormatter.Fail("task to be non-null", "task was null", Expression, because, becauseArgs);

        try
        {
            await Subject!.ConfigureAwait(false);
        }
        catch (Exception target)
        {
            if (target is AggregateException { InnerExceptions.Count: 1 } ae)
                target = ae.InnerExceptions[0];

            if (target is TException typed)
                return new ExceptionAssertions<TException>(typed, Expression);

            MessageFormatter.Fail(
                typeof(TException).Name,
                $"{target.GetType().Name}: {target.Message}",
                Expression,
                because,
                becauseArgs);
            return default!;
        }

        MessageFormatter.Fail(
            $"task to throw {typeof(TException).Name}",
            "no exception was thrown",
            Expression,
            because,
            becauseArgs);
        return default!;
    }
}
