# Research: NotBeEmpty Assertion

**Feature**: `00030-not-be-empty`  
**Phase**: 0 - Pre-design research  
**Date**: 2026-06-07

## Decision 1: Null subject behavior for string `NotBeEmpty`

**Decision**: A null string subject **passes** `NotBeEmpty`.

**Rationale**: `BeEmpty` uses `string.Equals(Subject, string.Empty, StringComparison.Ordinal)`, which returns `false` for `null`. `NotBeEmpty` is the strict logical complement — it fails only when the subject is `""`, so `null` falls outside the failure condition and passes. This also matches the established pattern for `NotContain`, which passes when the subject is `null` (because a null string cannot contain anything).

**Alternatives considered**:
- Fail on null (treating null as indistinguishable from empty, like `NotBeNullOrEmpty`). Rejected because it breaks the strict `BeEmpty` ↔ `NotBeEmpty` complement symmetry and would surprise callers who use null subjects intentionally.
- Fail on null via `GuardNull()`. Rejected — `GuardNull` is a collection-only helper. Applying it to strings would require a new helper and diverge from how string assertions handle null across the board.

## Decision 2: Null subject behavior for collection `NotBeEmpty`

**Decision**: A null collection subject **fails** `NotBeEmpty`, via the existing `GuardNull()` call.

**Rationale**: `BeEmpty` already calls `GuardNull()` before checking element count, so `NotBeEmpty` must do the same for behavioral consistency. A null reference is not a valid collection to assert non-emptiness on; it is a programming error distinct from an empty collection.

**Alternatives considered**:
- Pass on null (symmetric with string behavior). Rejected because the collection assertion model treats null as invalid at the `GuardNull()` boundary; bypassing it would create an inconsistency with all other collection assertions.

## Decision 3: Failure message format for string `NotBeEmpty`

**Decision**: When the subject is `""`, fail with expected = `"a non-empty string"` and actual = `"\"\""` (i.e., `MessageFormatter.FormatValue("")`, which wraps the empty string in double quotes).

**Rationale**: `MessageFormatter.FormatValue` is the canonical way to represent string values in failure messages throughout the library. Using it for the actual value gives a consistent, recognizable output format.

**Alternatives considered**:
- Actual = `"an empty string"`. Rejected because it diverges from the pattern of showing the literal value; other string assertions show the actual subject value via `FormatValue`.

## Decision 4: Failure message format for collection `NotBeEmpty`

**Decision**: When the collection has zero elements, fail with expected = `"a non-empty collection"` and actual = `"an empty collection"`.

**Rationale**: This mirrors the inverse of `BeEmpty`'s message (which uses expected = `"an empty collection"` and actual = a count summary). The new messages are explicit, self-descriptive, and symmetric.

**Alternatives considered**:
- Actual = `"a collection with 0 item(s)"`. Rejected as slightly less readable and less symmetric than using the plain phrase.

## Decision 5: File placement for collection `NotBeEmpty`

**Decision**: `GenericCollectionAssertions<T>.NotBeEmpty` goes into a new partial file `GenericCollectionAssertions.NotBeEmpty.cs`.

**Rationale**: `GenericCollectionAssertions.cs` is already at 307 lines, exceeding the 300-line constitution limit. Adding a new method inline would increase the violation. The project already uses partial files for this type (`GenericCollectionAssertions.Equal.cs`), so extracting to a new partial file is idiomatic and keeps each file focused.

**Alternatives considered**:
- Add inline to `GenericCollectionAssertions.cs`. Rejected because it worsens an existing constitution violation.
- Create a dedicated `NotBeEmptyAssertions.cs` in a subfolder. Rejected as overengineering for a single method.

## Decision 6: Implementation strategy — enumeration for collection

**Decision**: Use `!Subject!.Any()` as the emptiness check.

**Rationale**: `Any()` is a single-pass, early-exit operation that returns on the first element found. For the failing case (zero elements), it traverses the full (empty) sequence at cost O(0). For the passing case (at least one element), it exits on the first element. This matches the constitution's requirement for zero unnecessary allocations and no second-pass enumeration.

**Alternatives considered**:
- `Subject!.Count() == 0`. Rejected because `Count()` forces full enumeration, which is unnecessary when only non-emptiness is needed.

## Resolved Clarifications

All design decisions are resolved:
- Null string → passes (strict complement of `BeEmpty`).
- Null collection → fails (canonical `GuardNull()` behavior).
- String failure message → expected `"a non-empty string"`, actual `"\"\""`.
- Collection failure message → expected `"a non-empty collection"`, actual `"an empty collection"`.
- File placement → new partial file for collection; inline for string.
- Enumeration → single-pass `Any()`.
