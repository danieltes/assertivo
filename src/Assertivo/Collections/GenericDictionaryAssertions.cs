using System.Diagnostics;
using Assertivo.Primitives;

namespace Assertivo.Collections;

/// <summary>
/// Assertions for dictionary types implementing <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public readonly struct GenericDictionaryAssertions<TKey, TValue> where TKey : notnull
{
    internal GenericDictionaryAssertions(IEnumerable<KeyValuePair<TKey, TValue>>? subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the subject under test.</summary>
    public IEnumerable<KeyValuePair<TKey, TValue>>? Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the dictionary contains the specified key.
    /// </summary>
    /// <param name="expected">The key expected to exist.</param>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndWhichConstraint{TAssertions, TSubject}"/> exposing the value via <c>.Which</c>.</returns>
    [StackTraceHidden]
    public AndWhichConstraint<GenericDictionaryAssertions<TKey, TValue>, TValue> ContainKey(TKey expected, string because = "", params object[] becauseArgs)
    {
        GuardNull(because, becauseArgs);

        foreach (var kvp in Subject!)
        {
            if (EqualityComparer<TKey>.Default.Equals(kvp.Key, expected))
            {
                return new AndWhichConstraint<GenericDictionaryAssertions<TKey, TValue>, TValue>(this, kvp.Value);
            }
        }

        var keys = string.Join(", ", Subject!.Select(kvp => MessageFormatter.FormatValue(kvp.Key)));

        MessageFormatter.Fail(
            $"dictionary to contain key {MessageFormatter.FormatValue(expected)}",
            $"{{  {keys}  }}",
            Expression,
            because,
            becauseArgs);
        return default!;
    }

    /// <summary>
    /// Asserts that the subject is not <see langword="null"/>.
    /// </summary>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
    [StackTraceHidden]
    public AndConstraint<GenericDictionaryAssertions<TKey, TValue>> NotBeNull(string because = "", params object[] becauseArgs)
    {
        if (Subject is null)
        {
            MessageFormatter.Fail(
                "not <null>",
                "<null>",
                Expression,
                because,
                becauseArgs);
        }
        return new AndConstraint<GenericDictionaryAssertions<TKey, TValue>>(this);
    }

    /// <summary>
    /// Asserts that the subject is <see langword="null"/>.
    /// </summary>
    /// <param name="because">An optional reason for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
    [StackTraceHidden]
    public AndConstraint<GenericDictionaryAssertions<TKey, TValue>> BeNull(string because = "", params object[] becauseArgs)
    {
        if (Subject is not null)
        {
            MessageFormatter.Fail(
                "<null>",
                MessageFormatter.FormatValue(Subject),
                Expression,
                because,
                becauseArgs);
        }
        return new AndConstraint<GenericDictionaryAssertions<TKey, TValue>>(this);
    }

    [StackTraceHidden]
    private void GuardNull(string because, object[] becauseArgs)
    {
        if (Subject is null)
        {
            MessageFormatter.Fail(
                "a non-null dictionary",
                "<null>",
                Expression,
                because,
                becauseArgs);
        }
    }
}
