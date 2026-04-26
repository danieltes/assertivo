using System.Runtime.CompilerServices;
using Assertivo.Collections;
using Assertivo.Exceptions;
using Assertivo.Numeric;

namespace Assertivo;

/// <summary>
/// Extension methods providing the <c>.Should()</c> entry point for all assertion types.
/// </summary>
public static class ShouldExtensions
{
    /// <summary>Returns a <see cref="BooleanAssertions"/> for the specified boolean subject.</summary>
    public static BooleanAssertions Should(this bool subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="NumericAssertions{T}"/> for the specified int subject.</summary>
    public static NumericAssertions<int> Should(this int subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="NumericAssertions{T}"/> for the specified long subject.</summary>
    public static NumericAssertions<long> Should(this long subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="StringAssertions"/> for the specified string subject.</summary>
    public static StringAssertions Should(this string? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns an <see cref="ActionAssertions"/> for the specified action subject.</summary>
    public static ActionAssertions Should(this Action subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns an <see cref="AsyncFunctionAssertions"/> for the specified async function subject.</summary>
    public static AsyncFunctionAssertions Should(this Func<Task> subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>
    /// Returns an <see cref="ActionAssertions"/> for the specified function subject,
    /// adapting it to an <see cref="Action"/> by discarding the return value.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown immediately if <paramref name="subject"/> is <see langword="null"/>.
    /// </exception>
    public static ActionAssertions Should<T>(this Func<T> subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return new ActionAssertions(() => subject(), caller);
    }

    /// <summary>
    /// Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified
    /// read-only list subject.
    /// </summary>
    public static GenericCollectionAssertions<T> Should<T>(
        this IReadOnlyList<T>? subject,
        [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>
    /// Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified
    /// read-only collection subject.
    /// </summary>
    public static GenericCollectionAssertions<T> Should<T>(
        this IReadOnlyCollection<T>? subject,
        [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified enumerable subject.</summary>
    public static GenericCollectionAssertions<T> Should<T>(this IEnumerable<T>? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified array subject.</summary>
    public static GenericCollectionAssertions<T> Should<T>(this T[]? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="GenericCollectionAssertions{T}"/> for the specified list subject.</summary>
    public static GenericCollectionAssertions<T> Should<T>(
        this List<T>? subject,
        [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);

    /// <summary>Returns a <see cref="GenericDictionaryAssertions{TKey, TValue}"/> for the specified dictionary subject.</summary>
    public static GenericDictionaryAssertions<TKey, TValue> Should<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>>? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        where TKey : notnull
        => new(subject, caller);

    /// <summary>Returns a <see cref="GenericDictionaryAssertions{TKey, TValue}"/> for the specified <see cref="IDictionary{TKey, TValue}"/> subject.</summary>
    public static GenericDictionaryAssertions<TKey, TValue> Should<TKey, TValue>(this IDictionary<TKey, TValue>? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        where TKey : notnull
        => new(subject, caller);

    /// <summary>Returns a <see cref="GenericDictionaryAssertions{TKey, TValue}"/> for the specified <see cref="IReadOnlyDictionary{TKey, TValue}"/> subject.</summary>
    public static GenericDictionaryAssertions<TKey, TValue> Should<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        where TKey : notnull
        => new(subject, caller);

    /// <summary>Returns an <see cref="ObjectAssertions{T}"/> for the specified subject (generic fallback).</summary>
    public static ObjectAssertions<T> Should<T>(this T subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)
        => new(subject, caller);
}
