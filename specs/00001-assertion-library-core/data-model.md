# Data Model: Assertion Library Core

**Feature**: 00001-assertion-library-core
**Date**: 2026-03-31

## Entities

### 1. AssertionFailedException

The library's failure exception type. Thrown when an assertion fails.

**Fields**:
- `Message` (string): Structured failure message containing expected, actual, expression, and reason
- `Expected` (string): String representation of the expected value/condition
- `Actual` (string): String representation of the actual value/condition
- `Expression` (string?): The subject expression captured via [CallerArgumentExpression] at the `.Should()` call site (may be null)
- `Reason` (string?): User-supplied `because` reason (formatted if `becauseArgs` provided)

**Relationships**: Inherits from `System.Exception`. The `AssertionConfiguration.ReportFailure` delegate is the sole creator of this exception by default.

**Validation**: `Message` is always non-null and non-empty. `Expected` and `Actual` are always non-null.

### 2. AssertionConfiguration (static)

Static configuration class holding the pluggable failure reporting delegate.

**Fields**:
- `ReportFailure` (static Action\<AssertionFailure\>): Delegate invoked on assertion failure. Defaults to throwing `AssertionFailedException`. Adapters replace this at `[ModuleInitializer]` time.

**Relationships**: Referenced by all assertion classes via static call. Framework adapters set the delegate.

**Validation**: `ReportFailure` must never be null. Thread-safe via `volatile` field.

### 3. AssertionFailure (readonly struct)

Value type encapsulating structured failure data passed to the `ReportFailure` delegate.

**Fields**:
- `Expected` (string): Expected value/condition
- `Actual` (string): Actual value/condition
- `Expression` (string?): Captured caller argument expression
- `Reason` (string?): Formatted user-supplied reason
- `Message` (string): Pre-formatted human-readable failure message

**Relationships**: Created by assertion methods on failure. Consumed by `AssertionConfiguration.ReportFailure`.

### 4. AndConstraint\<TAssertions\> (readonly struct)

Continuation type returned by assertion methods that do not extract a sub-subject.

**Fields**:
- `And` (TAssertions): The parent assertion object, exposed as a readonly property for chaining

**Relationships**: Returned by most assertion methods (`Be`, `BeTrue`, `Contain`, `HaveCount`, etc.). `TAssertions` is one of the concrete assertion types.

### 5. AndWhichConstraint\<TAssertions, TSubject\> (readonly struct)

Continuation type returned by assertion methods that extract a sub-subject for drill-down.

**Fields**:
- `And` (TAssertions): The parent assertion object
- `Which` (TSubject): The extracted sub-subject for further `.Should()` chains

**Relationships**: Returned by `ContainSingle()`, `BeOfType<T>()`, `ContainKey()`, `Throw<T>()`. Independent from `AndConstraint` (not related by inheritance).

### 6. ExceptionAssertions\<TException\> (readonly struct)

Result type from `Throw<T>()` and `ThrowAsync<T>()`.

**Fields**:
- `Which` (TException): The caught exception instance
- `And` (ExceptionAssertions\<TException\>): Self-reference for further chaining

**Methods**:
- `WithMessage(expectedSubstring, because?, becauseArgs?)` → `AndConstraint<ExceptionAssertions<TException>>`: Asserts `Exception.Message` contains `expectedSubstring` (ordinal, case-sensitive)

**Relationships**: Returned by `ActionAssertions.Throw<T>()`. `AsyncFunctionAssertions.ThrowAsync<T>()` returns `Task<ExceptionAssertions<T>>`. `TException` is constrained to `Exception`.

> **Note on `.And` semantics**: `ExceptionAssertions<T>.And` returns *self* (the same `ExceptionAssertions<T>` instance) for continued exception-scoped assertions (e.g., `.WithMessage().And.WithMessage()`). This differs from `AndConstraint<T>.And` which returns the *parent assertion object* for continued subject-scoped assertions (e.g., `.Be(42).And.NotBeNull()`). The two patterns serve different chaining roles and are not interchangeable.

### 7. Assertion Objects (readonly structs)

Each assertion type holds the subject and the captured expression string.

| Type | Subject Type | Fields |
|------|-------------|--------|
| `ObjectAssertions<T>` | `T` (reference) | `Subject: T`, `Expression: string?` |
| `BooleanAssertions` | `bool` | `Subject: bool`, `Expression: string?` |
| `NumericAssertions<T>` where `T : struct, IComparable<T>, IEquatable<T>` | `T` | `Subject: T`, `Expression: string?` |
| `StringAssertions` | `string?` | `Subject: string?`, `Expression: string?` |
| `GenericCollectionAssertions<T>` | `IEnumerable<T>?` | `Subject: IEnumerable<T>?`, `Expression: string?` |
| `GenericDictionaryAssertions<TKey, TValue>` | `IEnumerable<KeyValuePair<TKey, TValue>>?` | `Subject`, `Expression: string?` |
| `ActionAssertions` | `Action` | `Subject: Action`, `Expression: string?` |
| `AsyncFunctionAssertions` | `Func<Task>` | `Subject: Func<Task>`, `Expression: string?` |

**State transitions**: None — all assertion objects are immutable value types. Each assertion method call creates a new constraint struct; the original assertion struct is never mutated.

**Validation rules**:
- Subject can be null for nullable types (`string?`, `IEnumerable<T>?`). Null subjects are valid — assertion methods check for null and produce descriptive failures.
- `Expression` (the subject expression) is always populated by `[CallerArgumentExpression]` at the `.Should()` call site; null only when called via reflection or indirect invocation.

## Type Resolution (Should() dispatch)

| Static Subject Type | Extension Method | Returns |
|---|---|---|
| `bool` | `Should(this bool)` | `BooleanAssertions` |
| `int` | `Should(this int)` | `NumericAssertions<int>` |
| `long` | `Should(this long)` | `NumericAssertions<long>` |
| `string` / `string?` | `Should(this string?)` | `StringAssertions` |
| `IEnumerable<T>` | `Should<T>(this IEnumerable<T>?)` | `GenericCollectionAssertions<T>` |
| `IDictionary<K,V>` / `IReadOnlyDictionary<K,V>` | `Should<K,V>(this IEnumerable<KeyValuePair<K,V>>?)` | `GenericDictionaryAssertions<K,V>` |
| `Action` | `Should(this Action)` | `ActionAssertions` |
| `Func<Task>` | `Should(this Func<Task>)` | `AsyncFunctionAssertions` |
| `T` (fallback) | `Should<T>(this T)` | `ObjectAssertions<T>` |

**Resolution order**: The C# compiler selects the most specific overload. Specialized overloads (`bool`, `int`, `long`, `string`, `Action`, `Func<Task>`) are preferred over the generic `T` fallback. `IEnumerable<T>` overload matches `List<T>`, arrays, etc.
