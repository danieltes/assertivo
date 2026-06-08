# Public API Contract: NotBeEmpty Assertion

**Feature**: `00030-not-be-empty`  
**Phase**: 1 - Design  
**Date**: 2026-06-07

---

## 1. `StringAssertions.NotBeEmpty`

**Namespace**: `Assertivo`  
**File**: `src/Assertivo/StringAssertions.cs`

```csharp
/// <summary>
/// Asserts that the subject is not an empty string.
/// </summary>
/// <remarks>
/// Passes for any value other than <see cref="string.Empty"/>, including <see langword="null"/>.
/// Use <see cref="NotBeNullOrEmpty"/> to fail on both null and empty.
/// </remarks>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<StringAssertions> NotBeEmpty(
    string because = "",
    params object[] becauseArgs);
```

### Behavior Contract

| Scenario | Outcome |
|---|---|
| Subject is a non-empty string (e.g., `"hello"`) | Passes and returns `AndConstraint<StringAssertions>` |
| Subject is `""` (empty string) | Fails with expected `"a non-empty string"`, actual `"\"\""` |
| Subject is `null` | Passes (null is not the empty string) |
| Subject is whitespace-only (e.g., `"   "`) | Passes (whitespace-only strings are not `""`) |
| `because` supplied on failure | Formatted reason is included in the failure message |

### Failure Message Contract

When the subject is `""`, the failure is emitted via `MessageFormatter.Fail` with:

- **Expected**: `a non-empty string`
- **Actual**: `""` (via `MessageFormatter.FormatValue(Subject)`)
- **Expression**: caller expression when available
- **Because**: appended when non-empty

Representative output:
```
Expected a non-empty string but found "".
Expression: result
Because: response body must not be blank
```

### Machine-Checkable Diagnostic Requirements

For empty-string failures, tests MUST assert all of the following:

1. `Expected` equals `a non-empty string`.
2. `Actual` equals `""`.
3. `Expression:` line is present when caller expression metadata is available.
4. `Because:` line is present only when a non-empty/non-whitespace reason is supplied.

### Chaining Contract

Returns `AndConstraint<StringAssertions>`, enabling fluent continuation:

```csharp
apiResponse.Should().NotBeEmpty().And.Contain("token");
```

---

## 2. `GenericCollectionAssertions<T>.NotBeEmpty`

**Namespace**: `Assertivo.Collections`  
**File**: `src/Assertivo/Collections/GenericCollectionAssertions.NotBeEmpty.cs` (new partial file)

```csharp
/// <summary>
/// Asserts that the collection is not empty (contains at least one element).
/// </summary>
/// <param name="because">An optional reason for the assertion.</param>
/// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
/// <returns>An <see cref="AndConstraint{TAssertions}"/> for continued chaining.</returns>
[StackTraceHidden]
public AndConstraint<GenericCollectionAssertions<T>> NotBeEmpty(
    string because = "",
    params object[] becauseArgs);
```

### Behavior Contract

| Scenario | Outcome |
|---|---|
| Subject has one or more elements | Passes and returns `AndConstraint<GenericCollectionAssertions<T>>` |
| Subject has zero elements | Fails with expected `"a non-empty collection"`, actual `"an empty collection"` |
| Subject is `null` | Fails via canonical null-guard contract: expected `"a collection"`, actual `"<null>"` |
| `because` supplied on failure | Formatted reason is included in the failure message |

### Null-Guard Contract

A null subject is caught by `GuardNull()` before any element evaluation, producing:

- **Expected**: `a collection`
- **Actual**: `<null>`

This is the same null-guard behavior used by all other collection assertions that call `GuardNull()`.

### Failure Message Contract (Empty Collection)

When the subject has zero elements, the failure is emitted via `MessageFormatter.Fail` with:

- **Expected**: `a non-empty collection`
- **Actual**: `an empty collection`
- **Expression**: caller expression when available
- **Because**: appended when non-empty

Representative output:
```
Expected a non-empty collection but found an empty collection.
Expression: results
Because: pipeline must produce results
```

### Machine-Checkable Diagnostic Requirements

For empty-collection failures, tests MUST assert all of the following:

1. `Expected` equals `a non-empty collection`.
2. `Actual` equals `an empty collection`.
3. `Expression:` line is present when caller expression metadata is available.
4. `Because:` line is present only when a non-empty/non-whitespace reason is supplied.

### Enumeration Contract

- Evaluation uses `Any()` — single-pass, early-exit.
- No materialization is required for pass/fail determination.
- The sequence is evaluated only as far as needed to find the first element.

### Chaining Contract

Returns `AndConstraint<GenericCollectionAssertions<T>>`, enabling fluent continuation:

```csharp
results.Should().NotBeEmpty().And.HaveCount(3);
```

---

## Because Formatting Contract (Both Methods)

Applies identically to both `StringAssertions.NotBeEmpty` and `GenericCollectionAssertions<T>.NotBeEmpty`:

| Input form | Expected behavior |
|---|---|
| `because = ""` | No `Because:` line is emitted |
| `because = "   "` | `Because:    ` line IS emitted — whitespace-only reasons are not suppressed (`MessageFormatter` uses `string.IsNullOrEmpty`, not `IsNullOrWhiteSpace`) |
| `because = "because {0}", becauseArgs = ["X"]` | `Because: because X` line is emitted |

---

## Compatibility

- Additive API change only.
- No existing methods are modified.
- `BeEmpty` on both types remains unchanged.
- `NotBeNullOrEmpty` on `StringAssertions` remains unchanged and is a distinct assertion (fails on both null and empty).

---

## Traceability Matrix

| Requirement | Coverage |
|---|---|
| FR-001 | `StringAssertions.NotBeEmpty` method signature |
| FR-002 | String Behavior Contract (non-`""` and null pass) |
| FR-003 | String Failure Message Contract and Machine-Checkable Diagnostic Requirements |
| FR-004 | `GenericCollectionAssertions<T>.NotBeEmpty` method signature |
| FR-005 | Collection Null-Guard Contract |
| FR-006 | Collection Behavior Contract (one-or-more elements passes) |
| FR-007 | Collection Failure Message Contract and Machine-Checkable Diagnostic Requirements |
| FR-008 | Because Formatting Contract |
| FR-009 | `[StackTraceHidden]` on both method signatures |
| FR-010 | Documentation Update Scope below |

---

## Documentation Update Scope

Completion requires all of the following to be updated consistently:

1. `specs/00030-not-be-empty/contracts/public-api.md` (this contract).
2. `specs/00030-not-be-empty/quickstart.md` (usage and failure examples).
3. XML documentation comments on `StringAssertions.NotBeEmpty` in `src/Assertivo/StringAssertions.cs`.
4. XML documentation comments on `GenericCollectionAssertions<T>.NotBeEmpty` in `src/Assertivo/Collections/GenericCollectionAssertions.NotBeEmpty.cs`.
