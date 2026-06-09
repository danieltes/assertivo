# Public API Contract: NotBeSameAs Assertion

**Feature**: `00031-not-be-same-as`
**Phase**: 1 — Design
**Date**: 2026-06-08

---

## `ObjectAssertions<T>.NotBeSameAs`

**Namespace**: `Assertivo`
**File**: `src/Assertivo/ObjectAssertions.cs`

```csharp
/// <summary>
/// Asserts that the subject is not the same reference as <paramref name="unexpected"/>.
/// </summary>
/// <param name="unexpected">The reference the subject must not be identical to.</param>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
/// <exception cref="InvalidOperationException">
/// Thrown when <typeparamref name="T"/> is a value type, because reference comparison
/// is not meaningful for value types.
/// </exception>
[StackTraceHidden]
public AndConstraint<ObjectAssertions<T>> NotBeSameAs(
    object? unexpected,
    string because = "",
    params object[] becauseArgs);
```

---

## Behavior Contract

| Scenario | Outcome |
|----------|---------|
| Subject and `unexpected` are distinct object instances | Passes; returns `AndConstraint<ObjectAssertions<T>>` |
| Subject and `unexpected` are the same object instance | Fails with `AssertionFailedException` |
| Subject is `null`, `unexpected` is `null` | Fails — `ReferenceEquals(null, null)` is `true` |
| Subject is `null`, `unexpected` is a non-null object | Passes — different references |
| Subject is a non-null object, `unexpected` is `null` | Passes — different references |
| `T` is a value type (e.g., `int`, `bool`, `struct`) | Throws `InvalidOperationException` before comparison |
| `because` is supplied and assertion fails | Formatted reason is included in the failure message |

---

## Value-Type Guard Contract

When `typeof(T).IsValueType` is `true`, the method throws before performing any comparison:

```
System.InvalidOperationException:
  NotBeSameAs is not meaningful for value type '{typeof(T).Name}'. Use Be() for value equality.
```

This guard runs **before** `ReferenceEquals` — no comparison is performed for value-type subjects.

---

## Failure Message Contract

When `ReferenceEquals(Subject, unexpected)` is `true` (assertion fails), the failure is emitted via `MessageFormatter.Fail` with:

- **Expected**: `not the same reference`
- **Actual**: `same reference`
- **Expression**: caller expression string when available
- **Because**: formatted reason appended when non-empty

Representative output:
```
Expected not the same reference but found same reference.
Expression: result
Because: the factory must return a new instance each time
```

### Machine-Checkable Diagnostic Requirements

For same-reference failures, tests MUST assert all of the following:

1. An `AssertionFailedException` is thrown.
2. `Expected` equals `not the same reference`.
3. `Actual` equals `same reference`.
4. When `because` is non-empty, the `Reason` property equals the formatted reason.
5. When `because` is non-empty, the full `Message` contains the reason text.

---

## Because Formatting Contract

| Input form | Expected behavior |
|------------|-------------------|
| `because = ""` (default) | No `Because:` line is emitted |
| `because = "my reason"` | `Because: my reason` line is emitted |
| `because = "expected {0}", becauseArgs = ["X"]` | `Because: expected X` line is emitted |

---

## Chaining Contract

Returns `AndConstraint<ObjectAssertions<T>>`, enabling fluent continuation:

```csharp
var a = factory.Create();
var b = factory.Create();
a.Should().NotBeSameAs(b).And.NotBeNull();
```

---

## Compatibility

- Additive API change only — no existing methods are modified.
- `BeSameAs` remains unchanged.
- Adding `NotBeSameAs` completes the `BeSameAs` / `NotBeSameAs` negation pair per constitution §VII.2.

---

## Traceability Matrix

| Requirement | Coverage |
|-------------|---------|
| FR-001 | Method signature above |
| FR-002 | Behavior Contract — distinct instances row |
| FR-003 | Behavior Contract — same instance row + Failure Message Contract |
| FR-004 | Value-Type Guard Contract |
| FR-005 | Because Formatting Contract + Machine-Checkable Diagnostic Requirement #4–5 |
| FR-006 | Behavior Contract — null/null row |
| FR-007 | Behavior Contract — null/non-null row |

---

## Documentation Update Scope

Completion requires all of the following to be updated consistently:

1. `specs/00031-not-be-same-as/contracts/public-api.md` (this contract).
2. `specs/00001-assertion-library-core/contracts/public-api.md` — add `NotBeSameAs` to `ObjectAssertions<T>` section.
3. `specs/00031-not-be-same-as/quickstart.md` (usage and failure examples).
4. XML documentation comments on `ObjectAssertions<T>.NotBeSameAs` in `src/Assertivo/ObjectAssertions.cs`.
