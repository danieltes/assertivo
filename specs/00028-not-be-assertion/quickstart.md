# Quickstart: NotBe Inequality Assertion

**Feature**: 00028-not-be-assertion  
**Date**: 2026-05-31

`NotBe` is the symmetric counterpart of `Be`. Use it to assert that a subject does **not** equal a specific value.

---

## Basic usage

### Integers and other numeric types

```csharp
int previousCount = 5;
int newCount = collection.Count;

// Assert the count changed
newCount.Should().NotBe(previousCount);

// Assert a result is not a sentinel value
int userId = CreateUser();
userId.Should().NotBe(0);
```

### Strings

```csharp
string name = profile.DisplayName;

// Assert the name was updated away from a default
name.Should().NotBe("default");

// Assert two names are different
string original = "Alice";
string renamed = "Bob";
renamed.Should().NotBe(original);
```

### Any object type

```csharp
var order = new Order { Status = OrderStatus.Shipped };

// Assert using the generic Should<T>() overload
order.Status.Should<OrderStatus>().NotBe(OrderStatus.Cancelled);
```

---

## Custom equality comparer (`ObjectAssertions<T>` and `NumericAssertions<T>`)

```csharp
// Case-insensitive string identity via ObjectAssertions<string>
string tag = "Hello";
tag.Should<string>().NotBe("HELLO", StringComparer.OrdinalIgnoreCase);
// ↑ fails — "Hello" and "HELLO" are equal under OrdinalIgnoreCase

tag.Should<string>().NotBe("world", StringComparer.OrdinalIgnoreCase);
// ↑ passes — "Hello" and "world" are not equal
```

---

## Adding a reason (`because`)

When an assertion fails, pass a `because` string to get a more informative message:

```csharp
int result = Compute();
result.Should().NotBe(0, "the computation should always produce a non-zero result");
```

Failure output:
```
Expected not 0 but found 0.
Expression: result
Because: the computation should always produce a non-zero result
```

---

## Chaining

`NotBe` returns an `AndConstraint` that exposes `.And` for continued assertions:

```csharp
int score = GetScore();
score.Should()
    .NotBe(0, "score must not be zero")
    .And.BeGreaterThanOrEqualTo(1);
```

---

## Failure output

When `NotBe` fails (subject **equals** the unexpected value):

```
Expected not "hello" but found "hello".
Expression: name
```

When called with a `because`:

```
Expected not 42 but found 42.
Expression: userId
Because: the ID must differ from the default sentinel
```

---

## Null handling (`StringAssertions`)

```csharp
string? result = TryGetValue();

// Pass: null ≠ "something"
result.Should().NotBe("something");

// Fail: null == null
string? nothing = null;
nothing.Should().NotBe(null);
// ↑ throws AssertionFailedException: "Expected not <null> but found <null>."
```
