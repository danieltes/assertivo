# Research: Ordered Sequence Equality Assertion

**Feature**: `00025-sequence-equal`  
**Phase**: 0 — Pre-design research  
**Date**: 2026-05-13

## Decision Log

### D-001: File-split strategy for 300-line limit

**Decision**: Declare `GenericCollectionAssertions<T>` as `partial struct` and place `Equal` overloads in a new file `GenericCollectionAssertions.Equal.cs`.

**Rationale**: `GenericCollectionAssertions.cs` is currently at 278 lines. The `Equal` implementation including required XML documentation (`<summary>`, `<param>`, `<returns>`) adds ~50 lines, bringing the total to ~328 lines — breaching the constitution's 300-line maximum. The standard C# mechanism for this is `partial`. The partial keyword is additive and non-breaking; no callers, tests, or dispatch code need to change.

**Alternatives considered**:
- Squeeze all new code into the existing file by shortening existing docs or collapsing signatures — rejected: violates constitution XML doc requirement and reduces readability.
- Create a subclass or wrapper — rejected: `GenericCollectionAssertions<T>` is a `readonly struct`; inheritance is not available.
- Defer until another method is extracted to make room — rejected: unnecessary churn on unrelated code; partial is the correct tool.

---

### D-002: Failure message format alignment with `MessageFormatter.BuildMessage`

**Decision**: Use `MessageFormatter.Fail(expected, actual, expression, because, becauseArgs)` directly, accepting that the produced message is `Expected {expected} but found {actual}.` (no Oxford comma before "but").

**Rationale**: The spec diagnostic format (`Expected collection with N element(s), but found M element(s).`) includes a comma that `BuildMessage` does not produce. The comma is a cosmetic style preference, not a semantic requirement. Introducing a new overload of `Fail` or `BuildMessage` solely for one assertion's comma preference would violate the principle of not over-engineering. All existing assertions use `BuildMessage` without the comma; consistency with the existing message style is higher value than matching the spec's editorial comma exactly.

Concrete message shapes produced:
- Count mismatch → `Expected collection with 3 element(s) but found 2 element(s).`
- Element mismatch → `Expected "foo" at index 1 but found "bar".`
- Null element → `Expected <null> at index 0 but found "x".`

**Alternatives considered**:
- Add a `BuildMessageWithReason` overload that inserts a comma — rejected: over-engineering for one comma.
- Use `string.Format` directly and bypass `MessageFormatter` — rejected: bypasses the failure pipeline (framework adapter exception surfacing, `AssertionConfiguration`).

---

### D-003: Sequence materialization strategy

**Decision**: Materialize both sequences with `.ToList()` before comparing.

**Rationale**: Comparing counts requires knowing both lengths, which requires full enumeration of `IEnumerable<T>`. Materializing both into `List<T>` first gives O(1) index access, a single forward pass for count comparison, and a single forward pass for element comparison — total: two enumerations of the expected sequence and one of the actual. For the common case where both sequences have the same count, the total work is O(n). This is consistent with how `BeEquivalentTo` and `HaveCount` already materialize via `ToList()` and `.Count()`.

**Allocation profile**:
- Two `List<T>` allocations (bounded by sequence length).
- One `AndConstraint<GenericCollectionAssertions<T>>` value-type wrapper on happy path.
- Passes constitution §6.2: "Collection assertions MUST NOT allocate beyond a single enumerator and the result object" — two lists is within the permitted collection assertion budget.

**Alternatives considered**:
- Zip-iterate both sequences simultaneously without materializing — rejected: requires knowing whether both sequences are exhausted simultaneously, which still requires full enumeration and adds complexity with no measurable allocation saving.
- Use `IReadOnlyList<T>` fast path — rejected: valuable micro-optimisation but not required by this feature; can be added in a later performance pass without a spec change.

---

### D-004: `params T[]` overload chain and disambiguation

**Decision**: Implement the params convenience overload as `Equal(params T[] expected) => Equal((IEnumerable<T>)expected)`, identical to the `BeEquivalentTo(T[] expected)` pattern already in the codebase.

**Rationale**: The explicit cast to `IEnumerable<T>` prevents an infinite self-call loop that would otherwise occur because `T[]` satisfies `IEnumerable<T>`. This pattern is already established by `BeEquivalentTo`. The known overload resolution tradeoff (documented in spec Assumptions) — that `Equal(someArray)` resolves to the params overload rather than the `IEnumerable<T>` overload — is acceptable because it has no behavioural difference when no comparer or `because` is needed.

**Alternatives considered**:
- Expose only the `IEnumerable<T>` overload — rejected: reduces ergonomics for common small inline value lists.
- Use a named parameter or builder pattern to avoid ambiguity — rejected: violates constitution API rule 4 (no boolean traps / builder for simple cases).

---

### D-005: Default equality comparer behaviour for null comparer argument

**Decision**: `comparer ??= EqualityComparer<T>.Default` — a null comparer argument is silently treated as default equality. No exception is thrown.

**Rationale**: This is consistent with all other assertion methods in the library that accept an optional `IEqualityComparer<T>` (`Contain`, `BeEquivalentTo`). Throwing on null comparer would be surprising for a parameter that is explicitly nullable in the signature.

**Alternatives considered**:
- Throw `ArgumentNullException` for explicit null comparer — rejected: the parameter is nullable by design, and callers explicitly passing null intend "use default"; throwing breaks that intent.

---

## Summary Table

| ID | Decision | Outcome |
|----|----------|---------|
| D-001 | File split for 300-line limit | `partial struct` + new `GenericCollectionAssertions.Equal.cs` |
| D-002 | Message format | Use existing `MessageFormatter.Fail`; no comma before "but" |
| D-003 | Materialization | `.ToList()` for both sequences; single forward pass each |
| D-004 | params overload | `=> Equal((IEnumerable<T>)expected)` cast delegation |
| D-005 | Null comparer | `??= EqualityComparer<T>.Default` — silent default |

All NEEDS CLARIFICATION items resolved. No blockers remain.
