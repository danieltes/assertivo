# Data Model: NotBe Inequality Assertion

**Feature**: 00028-not-be-assertion  
**Phase**: 1 — Design  
**Date**: 2026-05-31

This feature introduces no new types or entities. It is a purely additive extension of three existing assertion structs. The data model documents each affected type and the new method signature it gains.

---

## Affected Types

### `ObjectAssertions<T>` — `src/Assertivo/ObjectAssertions.cs`

**Existing shape** (relevant excerpt):

| Member | Kind | Description |
|--------|------|-------------|
| `Subject` | `T` | The value under test |
| `Expression` | `string?` | Caller expression via `[CallerArgumentExpression]` |
| `Be(T, IEqualityComparer<T>?, string, object[])` | Method | Asserts subject equals expected |
| `BeSameAs(object?, string, object[])` | Method | Asserts same reference |
| `BeNull(string, object[])` | Method | Asserts subject is null |
| `NotBeNull(string, object[])` | Method | Asserts subject is not null |

**New method added by this feature**:

| Member | Return type | Description |
|--------|-------------|-------------|
| `NotBe(T unexpected, IEqualityComparer<T>? comparer, string because, object[] becauseArgs)` | `AndConstraint<ObjectAssertions<T>>` | Asserts subject does NOT equal `unexpected` |

**Behaviour**:
- `comparer ?? EqualityComparer<T>.Default` is used to test equality.
- If `comparer.Equals(Subject, unexpected)` is `true` → fail via `MessageFormatter.Fail` with expected = `$"not {FormatValue(unexpected)}"`, actual = `FormatValue(Subject)`.
- If `comparer.Equals(Subject, unexpected)` is `false` → return `new AndConstraint<ObjectAssertions<T>>(this)`.

**Null handling** (for reference-type `T`, e.g. `string?`): `EqualityComparer<T>.Default.Equals(null, null)` returns `true` — assertion fails. `EqualityComparer<T>.Default.Equals(null, non-null)` and `Equals(non-null, null)` both return `false` — assertion passes. Behaviour is consistent with `StringAssertions.NotBe` null semantics.

---

### `NumericAssertions<T>` — `src/Assertivo/Numeric/NumericAssertions.cs`

**Constraint**: `where T : struct, IComparable<T>, IEquatable<T>`

**Existing shape** (relevant excerpt):

| Member | Kind | Description |
|--------|------|-------------|
| `Subject` | `T` | The numeric value under test |
| `Expression` | `string?` | Caller expression |
| `Be(T, IEqualityComparer<T>?, string, object[])` | Method | Asserts subject equals expected |
| `BeGreaterThanOrEqualTo(T, IComparer<T>?, string, object[])` | Method | Numeric comparison |
| `BeLessThan(T, IComparer<T>?, string, object[])` | Method | Numeric comparison |

**New method added by this feature**:

| Member | Return type | Description |
|--------|-------------|-------------|
| `NotBe(T unexpected, IEqualityComparer<T>? comparer, string because, object[] becauseArgs)` | `AndConstraint<NumericAssertions<T>>` | Asserts subject does NOT equal `unexpected` |

**Behaviour**: Identical logic to `ObjectAssertions<T>.NotBe`, using `EqualityComparer<T>.Default` fallback. Since `T` is constrained to `struct`, `Subject` is never null; `unexpected` is also non-nullable (`T`, not `T?`).

---

### `StringAssertions` — `src/Assertivo/StringAssertions.cs`

**Existing shape** (relevant excerpt):

| Member | Kind | Description |
|--------|------|-------------|
| `Subject` | `string?` | The string value under test (may be null) |
| `Expression` | `string?` | Caller expression |
| `Be(string?, string, object[])` | Method | Ordinal equality |
| `Contain(string, string, object[])` | Method | Substring check |
| `NotContain(string, string, object[])` | Method | Substring absence check |
| `NotBeNullOrEmpty(string, object[])` | Method | Null/empty guard |

**New method added by this feature**:

| Member | Return type | Description |
|--------|-------------|-------------|
| `NotBe(string? unexpected, string because, object[] becauseArgs)` | `AndConstraint<StringAssertions>` | Asserts subject does NOT equal `unexpected` (ordinal, case-sensitive) |

**Behaviour**:
- Uses `string.Equals(Subject, unexpected, StringComparison.Ordinal)` — same as `Be`.
- If equal → fail with expected = `$"not {FormatValue(unexpected)}"`, actual = `FormatValue(Subject)`.
- If not equal → return `new AndConstraint<StringAssertions>(this)`.
- No `IEqualityComparer<string>` parameter — ordinal-only, consistent with `Be`. Case-insensitive variants deferred.

**Null handling** (all covered by `string.Equals(..., Ordinal)`):

| Subject | Unexpected | `string.Equals` result | `NotBe` outcome |
|---------|------------|----------------------|-----------------|
| `null` | `null` | `true` | ❌ Fails |
| `null` | `"x"` | `false` | ✅ Passes |
| `"x"` | `null` | `false` | ✅ Passes |
| `"x"` | `"x"` | `true` | ❌ Fails |
| `"x"` | `"y"` | `false` | ✅ Passes |

> **Note**: `"x"` in the table represents any non-null string, including the empty string `""`. `NotBe("")` on a null subject passes (null ≠ `""`), and `NotBe(null)` on an `""` subject passes (`""` ≠ null) — both fall under the `null × non-null` and `non-null × null` rows respectively.

---

## Invariants

- All three methods are `[StackTraceHidden]` — library frames excluded from failure stack traces.
- All three methods are `readonly` members of `readonly struct` types — no mutation, thread-safe.
- All three methods return a `readonly struct` (`AndConstraint<T>`) — zero heap allocation on the passing path.
- Failure is always reported via `MessageFormatter.Fail` → `AssertionConfiguration.ReportFailure` — no direct `throw` in assertion code.
- No global state. Comparer is always provided at the call site or defaulted locally.
