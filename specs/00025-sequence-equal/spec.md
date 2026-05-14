# Feature Specification: Ordered Sequence Equality Assertion

**Feature Branch**: `00025-sequence-equal`  
**Created**: 2026-05-13  
**Status**: Draft  
**Input**: User description: "Ordered sequence equality assertion (Equal) for IEnumerable collections"

## Clarifications

### Session 2026-05-13

- Q: What should `Equal` do when the `expected` argument is null? → A: Throw `ArgumentNullException` immediately — null expected is a caller error, not an assertion condition.
- Q: How should null element values be rendered in failure messages? → A: Render as `<null>`, consistent with the library's canonical null representation used in other assertions.
- Q: Should the `params T[]` overload resolution ambiguity (e.g., `Equal(someArray)` resolving to params rather than `IEnumerable<T>`) be called out? → A: Document as a known tradeoff in Assumptions — no behaviour change, transparency only.
- Q: Do FR-006 and FR-007 message templates include a comma before "but"? → A: No. Spec text updated to remove the comma and match `MessageFormatter.BuildMessage` output exactly, consistent with the contract file. (Resolved CHK012, CHK016, CHK031)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert Two Ordered Sequences Are Equal (Priority: P1)

A developer writing tests for order-sensitive operations (sorted results, pagination output, processing pipelines, event log entries) wants to assert that two sequences contain the same elements in the same order, where any deviation — including a reordering — is a failure.

**Why this priority**: This is the core value of the feature. Without an ordered equality assertion, developers either use the order-independent `BeEquivalentTo` (which silently ignores order and can hide bugs) or write manual index-by-index loops (which produce poor diagnostics and break fluent chaining).

**Independent Test**: Can be fully tested by asserting a known ordered sequence against an expected sequence, with separate cases for pass (matching order) and fail (differing order), without requiring any other feature.

**Acceptance Scenarios**:

1. **Given** two sequences with the same elements in the same order, **When** `Equal` is called, **Then** the assertion passes.
2. **Given** two sequences with the same elements but different order, **When** `Equal` is called, **Then** the assertion fails.
3. **Given** two empty sequences, **When** `Equal` is called, **Then** the assertion passes.
4. **Given** a null subject, **When** `Equal` is called, **Then** the assertion fails with a null-guard message.

---

### User Story 2 - Diagnose Ordered Equality Failures Precisely (Priority: P1)

A developer receiving a failed ordered equality assertion needs to know immediately whether the sequences have different lengths or differ at a specific index, so they can fix the source of the mismatch without manually inspecting both sequences.

**Why this priority**: Diagnostics are required for the assertion to be useful. A failure without actionable output forces manual investigation and negates the purpose of a fluent assertion library.

**Independent Test**: Can be fully tested by triggering count-mismatch and element-mismatch failures and verifying the exact shape of the failure message in each case, independent of any other feature.

**Acceptance Scenarios**:

1. **Given** two sequences with different counts, **When** `Equal` is called, **Then** the assertion fails and the message states the expected count and the actual count.
2. **Given** two sequences of equal length where elements differ at a specific index, **When** `Equal` is called, **Then** the failure message identifies that index and shows the expected and actual values at that position.
3. **Given** a failing assertion with a `because` reason, **When** the exception is thrown, **Then** the failure message includes the reason.
4. **Given** two sequences where multiple indices differ, **When** `Equal` is called, **Then** only the first differing index is reported.

---

### User Story 3 - Assert Against Inline Expected Values (Priority: P2)

A developer wants to assert a sequence against a known small set of values without creating a separate collection variable, keeping the test concise and readable.

**Why this priority**: This is a convenience over Story 1. The core behavior is identical; the params overload improves ergonomics for common cases.

**Independent Test**: Can be fully tested by using the inline-value form of the assertion with a fixed small sequence.

**Acceptance Scenarios**:

1. **Given** a subject sequence and an inline expected value list, **When** `Equal` is called with individual values, **Then** the assertion behaves identically to calling `Equal` with an equivalent enumerable — including pass/fail outcome and failure message content when the assertion fails.

---

### User Story 4 - Compare Elements with a Custom Equality Rule (Priority: P2)

A developer needs to compare sequence elements using a custom equality rule (for example, case-insensitive string comparison or value-object equality that differs from default), so the assertion can be adapted to domain-specific equality semantics.

**Why this priority**: Without custom comparer support, some equality rules cannot be expressed at all at the sequence level, forcing developers to pre-transform data before asserting.

**Independent Test**: Can be fully tested by providing a custom comparer and confirming that element comparison uses the comparer's logic rather than default equality.

**Acceptance Scenarios**:

1. **Given** two sequences that differ only in ways the custom comparer considers equal, **When** `Equal` is called with that comparer, **Then** the assertion passes.
2. **Given** two sequences where the custom comparer treats elements as unequal, **When** `Equal` is called with that comparer, **Then** the assertion fails.

### Edge Cases

- A null subject fails as an assertion failure, not a null-reference runtime exception.
- A null `expected` argument throws `ArgumentNullException` immediately before any comparison.
- An empty subject against a non-empty expected sequence triggers a count-mismatch failure.
- A non-empty subject against an empty expected sequence triggers a count-mismatch failure.
- When counts differ, no element-level comparison is attempted and no index is reported.
- Only the first differing index is reported; subsequent differences are not surfaced.
- A null element value (in either sequence) is rendered as `<null>` in failure messages.
- A null comparer argument is treated as equivalent to omitting the comparer (default equality is used).
- Calling `Equal()` with zero arguments (empty `params` array) is equivalent to calling `Equal(Array.Empty<T>())`; the assertion passes when the subject is also empty and fails with a count-mismatch message when the subject is non-empty.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `GenericCollectionAssertions<T>` MUST expose `Equal(IEnumerable<T> expected, IEqualityComparer<T>? comparer = null, string because = "", params object[] becauseArgs)` as the primary ordered-equality assertion.
- **FR-002**: `GenericCollectionAssertions<T>` MUST expose a params convenience overload `Equal(params T[] expected)` that delegates to FR-001 with default comparer and no reason.
- **FR-003**: Both `Equal` overloads MUST return `AndConstraint<GenericCollectionAssertions<T>>` to support fluent chaining.
- **FR-004**: `Equal` MUST fail through the library's standard assertion failure pipeline when the subject is null (via `GuardNull()` → `MessageFormatter.Fail` → `AssertionConfiguration.ReportFailure`, consistent with all other assertion methods in `GenericCollectionAssertions<T>`).
- **FR-005**: `Equal` MUST pass when both subject and expected sequences are empty.
- **FR-006**: When counts differ, `Equal` MUST produce a failure message of the form: `Expected collection with N element(s) but found M element(s).` (no comma before "but", consistent with `MessageFormatter.BuildMessage` output)
- **FR-007**: When counts match but elements differ, `Equal` MUST produce a failure message of the form: `Expected {FormatValue(expected[i])} at index {i} but found {FormatValue(actual[i])}.` where `i` is the first differing zero-based index, `FormatValue` follows the library's standard value-rendering rules (null → `<null>`, string → double-quoted, other → `ToString()`), and there is no comma before "but", consistent with `MessageFormatter.BuildMessage` output.
- **FR-008**: `Equal` MUST compare elements using the provided `IEqualityComparer<T>` when supplied, and default equality when no comparer is given.
- **FR-009**: `because` and `becauseArgs` MUST be incorporated into the failure message.
- **FR-010**: When counts differ, `Equal` MUST NOT perform element-level comparisons.
- **FR-011**: `Equal` MUST report only the first differing index; multi-index aggregation is out of scope. The element-comparison loop terminates at the first mismatch because `MessageFormatter.Fail` does not return (it always throws via the failure pipeline).
- **FR-012**: Existing `Should()` dispatch paths that resolve to `GenericCollectionAssertions<T>` MUST support `Equal` across `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `List<T>`, and arrays.
- **FR-013**: Deep structural equality of complex objects is out of scope for this feature.
- **FR-014**: Multi-index failure aggregation is out of scope for this feature; that concern belongs to `AllSatisfy`.
- **FR-015**: `Equal` MUST throw `ArgumentNullException` immediately when the `expected` argument is null, before any subject or element comparison is performed.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can assert ordered sequence equality in a single fluent statement without writing manual loops or helper methods.
- **SC-002**: A count-mismatch failure message identifies both the expected and actual element counts, enabling the developer to understand the discrepancy without inspecting both sequences manually.
- **SC-003**: An element-mismatch failure message identifies the first differing index and both values at that index, enabling the developer to locate the error immediately.
- **SC-004**: All acceptance scenarios across all four user stories pass as automated tests with zero false positives or false negatives.
- **SC-005**: The `Equal` assertion integrates seamlessly into chained assertions without breaking the fluent chain for passing cases.

## Assumptions

- The assertion materializes both sequences into index-accessible form for comparison; deferred enumeration side-effects are the caller's responsibility.
- The params convenience overload does not need a `because` parameter; callers requiring a reason string use the `IEnumerable<T>` overload directly.
- The failure message format for count mismatch and element mismatch uses singular/plural-aware `element(s)` phrasing consistent with existing library message patterns.
- `BeEquivalentTo` remains unchanged and continues to serve order-independent use cases; `Equal` is strictly additive.
- The feature targets the same collection types already supported by `GenericCollectionAssertions<T>`.
- The `params T[] expected` overload is subject to standard C# overload resolution: passing a `T[]` directly resolves to the params overload rather than the `IEnumerable<T>` overload. Callers who need to pass an array and also supply a comparer or `because` must use the `IEnumerable<T>` overload explicitly. This is a known tradeoff of the convenience overload and requires no behavioural change.
