# Quickstart Validation Guide: NotBeSameAs

**Feature**: `00031-not-be-same-as`
**Date**: 2026-06-08

This guide shows how to validate that the `NotBeSameAs` assertion works correctly end-to-end. Run through each scenario after implementation to confirm the feature is complete.

---

## Prerequisites

- .NET 10 SDK installed
- Repository cloned and dependencies restored

---

## Run the Tests

```
dotnet test tests/Assertivo.Tests/Assertivo.Tests.csproj
```

All pre-existing tests must remain green. The 6 new tests for `NotBeSameAs` must also pass.

---

## Scenario 1 — Distinct Object References (must pass silently)

```csharp
var a = new object();
var b = new object();
a.Should().NotBeSameAs(b);   // passes — distinct heap objects
```

**Expected outcome**: No exception thrown. The return value is an `AndConstraint<ObjectAssertions<object>>`.

---

## Scenario 2 — Same Reference (must throw)

```csharp
var obj = new object();
Assert.Throws<AssertionFailedException>(() => obj.Should().NotBeSameAs(obj));
```

**Expected outcome**: `AssertionFailedException` is thrown with:
- `Expected` = `not the same reference`
- `Actual` = `same reference`

---

## Scenario 3 — Value-Type Subject (must throw guard exception)

```csharp
int value = 42;
Assert.Throws<InvalidOperationException>(() => value.Should<int>().NotBeSameAs(42));
```

**Expected outcome**: `InvalidOperationException` with message containing:
```
NotBeSameAs is not meaningful for value type 'Int32'. Use Be() for value equality.
```

---

## Scenario 4 — Null Subject, Null Unexpected (must throw)

```csharp
object? subject = null;
Assert.Throws<AssertionFailedException>(() => subject.Should<object?>().NotBeSameAs(null));
```

**Expected outcome**: `AssertionFailedException` thrown — `null` and `null` are the same reference.

---

## Scenario 5 — Null Subject, Non-Null Unexpected (must pass silently)

```csharp
object? subject = null;
subject.Should<object?>().NotBeSameAs(new object());   // passes
```

**Expected outcome**: No exception thrown.

---

## Scenario 6 — Because Reason in Failure Message

```csharp
var obj = new object();
var ex = Assert.Throws<AssertionFailedException>(() =>
    obj.Should().NotBeSameAs(obj, because: "the factory must return a new instance"));

Assert.Contains("the factory must return a new instance", ex.Message);
Assert.Equal("the factory must return a new instance", ex.Reason);
```

**Expected outcome**: Exception message contains the supplied reason.

---

## Fluent Chaining

```csharp
var a = new object();
var b = new object();
a.Should().NotBeSameAs(b).And.NotBeNull();   // both assertions in one chain
```

**Expected outcome**: No exception thrown; chain completes successfully.

---

## Verify Test Count

After adding the 6 new tests, running:

```
dotnet test tests/Assertivo.Tests --list-tests | grep NotBeSameAs
```

Should produce exactly 6 lines, one per test method listed in the spec acceptance criteria.

---

## References

- Contract: [contracts/public-api.md](contracts/public-api.md)
- Data model: [data-model.md](data-model.md)
- Spec: [spec.md](spec.md)
