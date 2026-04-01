# Public API Contract: Assertivo

**Feature**: 00001-assertion-library-core
**Date**: 2026-03-31
**Format**: C# public API surface (pseudo-signatures)

This document defines the public API contract that consumers of the Assertivo NuGet package can depend on. All types listed here are public. Internal types are not listed.

---

## Namespace: Assertivo

### Static Class: Should (extension methods)

```csharp
public static class ShouldExtensions
{
    // Specialized overloads (selected by compiler over generic fallback)
    public static BooleanAssertions Should(this bool subject, [CallerArgumentExpression("subject")] string? caller = null);
    public static NumericAssertions<int> Should(this int subject, [CallerArgumentExpression("subject")] string? caller = null);
    public static NumericAssertions<long> Should(this long subject, [CallerArgumentExpression("subject")] string? caller = null);
    public static StringAssertions Should(this string? subject, [CallerArgumentExpression("subject")] string? caller = null);
    public static ActionAssertions Should(this Action subject, [CallerArgumentExpression("subject")] string? caller = null);
    public static AsyncFunctionAssertions Should(this Func<Task> subject, [CallerArgumentExpression("subject")] string? caller = null);

    // Generic overloads
    public static GenericCollectionAssertions<T> Should<T>(this IEnumerable<T>? subject, [CallerArgumentExpression("subject")] string? caller = null);
    public static GenericDictionaryAssertions<TKey, TValue> Should<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>>? subject, [CallerArgumentExpression("subject")] string? caller = null)
        where TKey : notnull;
    public static ObjectAssertions<T> Should<T>(this T subject, [CallerArgumentExpression("subject")] string? caller = null);
}
```

### Struct: ObjectAssertions\<T\>

```csharp
public readonly struct ObjectAssertions<T>
{
    public T Subject { get; }
    public AndConstraint<ObjectAssertions<T>> Be(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
    public AndConstraint<ObjectAssertions<T>> BeSameAs(object? expected, string because = "", params object[] becauseArgs);
    public AndConstraint<ObjectAssertions<T>> BeNull(string because = "", params object[] becauseArgs);
    public AndConstraint<ObjectAssertions<T>> NotBeNull(string because = "", params object[] becauseArgs);
    public AndWhichConstraint<ObjectAssertions<T>, TTarget> BeOfType<TTarget>(string because = "", params object[] becauseArgs);
}
```

### Struct: BooleanAssertions

```csharp
public readonly struct BooleanAssertions
{
    public bool Subject { get; }
    public AndConstraint<BooleanAssertions> BeTrue(string because = "", params object[] becauseArgs);
    public AndConstraint<BooleanAssertions> BeFalse(string because = "", params object[] becauseArgs);
}
```

### Struct: NumericAssertions\<T\>

```csharp
public readonly struct NumericAssertions<T> where T : struct, IComparable<T>, IEquatable<T>
{
    public T Subject { get; }
    public AndConstraint<NumericAssertions<T>> Be(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
    public AndConstraint<NumericAssertions<T>> BeGreaterThanOrEqualTo(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
    public AndConstraint<NumericAssertions<T>> BeLessThan(T value, IComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
}
```

### Struct: StringAssertions

```csharp
public readonly struct StringAssertions
{
    public string? Subject { get; }
    public AndConstraint<StringAssertions> Be(string? expected, string because = "", params object[] becauseArgs);
    public AndConstraint<StringAssertions> Contain(string expected, string because = "", params object[] becauseArgs);
    public AndConstraint<StringAssertions> NotContain(string expected, string because = "", params object[] becauseArgs);
    public AndConstraint<StringAssertions> NotBeNullOrEmpty(string because = "", params object[] becauseArgs);
    public AndConstraint<StringAssertions> BeEmpty(string because = "", params object[] becauseArgs);
}
```

---

## Namespace: Assertivo.Collections

### Struct: GenericCollectionAssertions\<T\>

```csharp
public readonly struct GenericCollectionAssertions<T>
{
    public IEnumerable<T>? Subject { get; }
    public AndConstraint<GenericCollectionAssertions<T>> HaveCount(int expected, string because = "", params object[] becauseArgs);
    public AndWhichConstraint<GenericCollectionAssertions<T>, T> ContainSingle(string because = "", params object[] becauseArgs);
    public AndWhichConstraint<GenericCollectionAssertions<T>, T> ContainSingle(Func<T, bool> predicate, string because = "", params object[] becauseArgs);
    public AndConstraint<GenericCollectionAssertions<T>> Contain(T expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
    public AndConstraint<GenericCollectionAssertions<T>> BeEmpty(string because = "", params object[] becauseArgs);
    public AndConstraint<GenericCollectionAssertions<T>> BeEquivalentTo(IEnumerable<T> expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
    public AndConstraint<GenericCollectionAssertions<T>> BeEquivalentTo(T[] expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs);
    public AndConstraint<GenericCollectionAssertions<T>> AllSatisfy(Action<T> inspector, string because = "", params object[] becauseArgs);
}
```

### Struct: GenericDictionaryAssertions\<TKey, TValue\>

```csharp
public readonly struct GenericDictionaryAssertions<TKey, TValue> where TKey : notnull
{
    public IEnumerable<KeyValuePair<TKey, TValue>>? Subject { get; }
    public AndWhichConstraint<GenericDictionaryAssertions<TKey, TValue>, TValue> ContainKey(TKey expected, string because = "", params object[] becauseArgs);
}
```

---

## Namespace: Assertivo.Exceptions

### Struct: ActionAssertions

```csharp
public readonly struct ActionAssertions
{
    public Action Subject { get; }
    public ExceptionAssertions<TException> Throw<TException>(string because = "", params object[] becauseArgs) where TException : Exception;
}
```

### Struct: AsyncFunctionAssertions

```csharp
public readonly struct AsyncFunctionAssertions
{
    public Func<Task> Subject { get; }
    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs) where TException : Exception;
}
```

### Struct: ExceptionAssertions\<TException\>

```csharp
public readonly struct ExceptionAssertions<TException> where TException : Exception
{
    public TException Which { get; }
    public ExceptionAssertions<TException> And { get; }
    public AndConstraint<ExceptionAssertions<TException>> WithMessage(string expectedSubstring, string because = "", params object[] becauseArgs);
}
```

---

## Namespace: Assertivo.Primitives

### Struct: AndConstraint\<TAssertions\>

```csharp
public readonly struct AndConstraint<TAssertions>
{
    public TAssertions And { get; }
}
```

### Struct: AndWhichConstraint\<TAssertions, TSubject\>

```csharp
public readonly struct AndWhichConstraint<TAssertions, TSubject>
{
    public TAssertions And { get; }
    public TSubject Which { get; }
}
```

---

## Namespace: Assertivo (Configuration & Exceptions)

### Class: AssertionFailedException

```csharp
public class AssertionFailedException : Exception
{
    public string Expected { get; }
    public string Actual { get; }
    public string? Expression { get; }
    public string? Reason { get; }
}
```

### Readonly Struct: AssertionFailure

```csharp
public readonly struct AssertionFailure
{
    public string Expected { get; }
    public string Actual { get; }
    public string? Expression { get; }
    public string? Reason { get; }
    public string Message { get; }
}
```

### Static Class: AssertionConfiguration

```csharp
public static class AssertionConfiguration
{
    public static Action<AssertionFailure> ReportFailure { get; set; }
    // Default: throws AssertionFailedException
}
```
