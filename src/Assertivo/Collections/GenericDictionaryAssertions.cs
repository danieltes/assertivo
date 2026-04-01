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
