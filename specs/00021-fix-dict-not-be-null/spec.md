# Feature Specification: Collection and Dictionary Null-Guard Assertions (BeNull / NotBeNull)

**Feature Branch**: `00021-fix-dict-not-be-null`  
**Created**: 2026-04-26  
**Status**: Draft  
**Input**: User description: "Calling `.Should().NotBeNull()` on an `IReadOnlyDictionary<K, V>` (or any dictionary type) fails to compile because `GenericDictionaryAssertions<TKey, TValue>` does not expose a `NotBeNull()` method."

## Clarifications

### Session 2026-04-26

- Q: Should `BeNull()` be included alongside `NotBeNull()` in this fix? → A: Yes — add both for full symmetry with `ObjectAssertions<T>` (Option A)
- Q: Should the fix extend to `GenericCollectionAssertions<T>` as well, which has the identical gap? → A: Yes — patch both assertion types together in the same branch (Option A)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Guard against null before asserting dictionary contents (Priority: P1)

A developer chains a null-guard assertion with a dictionary-specific assertion in one fluent expression. Without this, they must split null-checking and content-checking into separate statements or cast away the specific assertion type.

**Why this priority**: This is the blocking bug. Users cannot write the natural assertion `dict.Should().NotBeNull().And.ContainKey(...)` today because the compiler rejects it. Fixing it unlocks the entire chaining pattern for dictionaries.

**Independent Test**: Can be fully tested by calling `.Should().NotBeNull()` on a non-null `IReadOnlyDictionary<string, int>` and observing that it compiles, passes at runtime, and returns a chainable constraint.

**Acceptance Scenarios**:

1. **Given** a non-null `IReadOnlyDictionary<string, int>`, **When** `.Should().NotBeNull()` is called, **Then** the assertion passes and returns an `AndConstraint<GenericDictionaryAssertions<string, int>>`
2. **Given** a `null` `IReadOnlyDictionary<string, int>`, **When** `.Should().NotBeNull()` is called, **Then** the assertion fails with an error message that follows the library's standard format
3. **Given** a non-null dictionary, **When** `.Should().NotBeNull().And.ContainKey("key")` is called, **Then** both assertions pass and the expression compiles without error
4. **Given** a `null` `IReadOnlyDictionary<string, int>`, **When** `.Should().BeNull()` is called, **Then** the assertion passes
5. **Given** a non-null `IReadOnlyDictionary<string, int>`, **When** `.Should().BeNull()` is called, **Then** the assertion fails with an error message that follows the library's standard format

---

### User Story 2 - Guard against null before asserting collection contents (Priority: P1)

A developer chains a null-guard assertion with a collection-specific assertion in one fluent expression. Without this, they face the identical compiler error as with dictionaries.

**Why this priority**: Same root cause as the dictionary bug, same user impact, same fix effort. Resolving both together avoids an immediate follow-up report.

**Independent Test**: Can be fully tested by calling `.Should().NotBeNull()` on a non-null `IEnumerable<int>` and observing that it compiles, passes, and returns a chainable constraint.

**Acceptance Scenarios**:

1. **Given** a non-null `IEnumerable<int>`, **When** `.Should().NotBeNull()` is called, **Then** the assertion passes and returns an `AndConstraint<GenericCollectionAssertions<int>>`
2. **Given** a `null` `IEnumerable<int>`, **When** `.Should().NotBeNull()` is called, **Then** the assertion fails with an error message that follows the library's standard format
3. **Given** a `null` `IEnumerable<int>`, **When** `.Should().BeNull()` is called, **Then** the assertion passes
4. **Given** a non-null `IEnumerable<int>`, **When** `.Should().BeNull()` is called, **Then** the assertion fails with an error message that follows the library's standard format

---

### User Story 3 - Custom failure message for null collection or dictionary (Priority: P2)

A developer provides a custom `because` message to give more context when the null-guard fails in a test output.

**Why this priority**: Consistent with how other assertion methods in the library accept a `because` parameter; needed for diagnostic completeness but not a blocker.

**Independent Test**: Can be fully tested by calling `.Should().NotBeNull("because the config must be initialised")` on a null dictionary or collection and inspecting the failure message.

**Acceptance Scenarios**:

1. **Given** a `null` dictionary and a `because` message `"the config must be initialised"`, **When** `.Should().NotBeNull("the config must be initialised")` is called, **Then** the failure message contains the `because` text
2. **Given** a `null` collection and a `because` message `"the list must be populated"`, **When** `.Should().NotBeNull("the list must be populated")` is called, **Then** the failure message contains the `because` text
3. **Given** a non-null dictionary and a `because` message `"the config must be initialised"`, **When** `.Should().BeNull("the config must be initialised")` is called, **Then** the failure message contains the `because` text
4. **Given** a non-null collection and a `because` message `"the list must be populated"`, **When** `.Should().BeNull("the list must be populated")` is called, **Then** the failure message contains the `because` text

---

### Edge Cases

- What happens when the dictionary or collection reference is `null` and no `because` is supplied? The failure message should still be clear and follow the library format.
- What happens when `NotBeNull()` is called on a concrete `Dictionary<K,V>` rather than an interface? It should behave identically.
- What happens when `NotBeNull()` is called on a concrete `List<T>` rather than `IEnumerable<T>`? It should behave identically.

## Requirements *(mandatory)*

### Functional Requirements

> **Mirroring note**: FR-001–FR-008 and FR-009–FR-015 are intentional parallel mirrors for the dictionary and collection types. When modifying one block, always update the other to maintain symmetry.

**Dictionary assertions (`GenericDictionaryAssertions<TKey, TValue>`):**

- **FR-001**: MUST expose a `NotBeNull()` method that accepts an optional `because` string parameter
- **FR-002**: MUST expose a `BeNull()` method that accepts an optional `because` string parameter, maintaining full symmetry with `ObjectAssertions<T>`
- **FR-003**: When the subject is `null`, `NotBeNull()` MUST throw an assertion failure with a message following the existing library format
- **FR-004**: When the subject is not `null`, `BeNull()` MUST throw an assertion failure with a message following the existing library format
- **FR-005**: When the subject is not `null`, `NotBeNull()` MUST return an `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>` so further chaining is possible (e.g. `.And.ContainKey(...)`)
- **FR-006**: When the subject is `null`, `BeNull()` MUST return an `AndConstraint<GenericDictionaryAssertions<TKey, TValue>>`
- **FR-007**: Both methods MUST incorporate the optional `because` text in their failure messages when provided
- **FR-008**: Both methods MUST be reachable via `.Should()` on all dictionary types handled by the library's type dispatch (including `IDictionary<K,V>` and `IReadOnlyDictionary<K,V>`)

**Collection assertions (`GenericCollectionAssertions<T>`):**

- **FR-009**: MUST expose a `NotBeNull()` method that accepts an optional `because` string parameter
- **FR-010**: MUST expose a `BeNull()` method that accepts an optional `because` string parameter, maintaining full symmetry with `ObjectAssertions<T>`
- **FR-011**: When the subject is `null`, `NotBeNull()` MUST throw an assertion failure with a message following the existing library format
- **FR-012**: When the subject is not `null`, `BeNull()` MUST throw an assertion failure with a message following the existing library format
- **FR-013**: When the subject is not `null`, `NotBeNull()` MUST return an `AndConstraint<GenericCollectionAssertions<T>>` so further chaining is possible (e.g. `.And.HaveCount(...)`)
- **FR-014**: When the subject is `null`, `BeNull()` MUST return an `AndConstraint<GenericCollectionAssertions<T>>`
- **FR-015**: Both methods MUST incorporate the optional `because` text in their failure messages when provided

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing dictionary and collection assertion tests continue to pass without modification
- **SC-002**: A new test calling `.Should().NotBeNull()` on a non-null dictionary passes
- **SC-003**: A new test calling `.Should().NotBeNull()` on a `null` dictionary fails with a well-formed error message matching the library's format
- **SC-004**: A new test confirming `.Should().NotBeNull().And.ContainKey(...)` passes without exception at runtime, exercising the full fluent chain
- **SC-005**: A new test calling `.Should().BeNull()` on a `null` dictionary passes
- **SC-006**: A new test calling `.Should().BeNull()` on a non-null dictionary fails with a well-formed error message matching the library's format
- **SC-007**: A new test calling `.Should().NotBeNull()` on a non-null collection passes
- **SC-008**: A new test calling `.Should().NotBeNull()` on a `null` collection fails with a well-formed error message matching the library's format
- **SC-009**: A new test calling `.Should().BeNull()` on a `null` collection passes
- **SC-010**: A new test calling `.Should().BeNull()` on a non-null collection fails with a well-formed error message matching the library's format
- **SC-011**: Zero new compiler warnings or errors introduced in the library project

## Assumptions

- Both `BeNull()` and `NotBeNull()` will be added directly to `GenericDictionaryAssertions<TKey, TValue>` and `GenericCollectionAssertions<T>` (not by restructuring the inheritance hierarchy), consistent with how existing methods are provided in each type
- `because` formatting follows the same helper already used elsewhere in the library (`MessageFormatter`)
- No public API surface changes are needed in `Should.cs` — type dispatch already routes correctly to each assertions type; only the missing methods need to be added
- The `AndConstraint<T>` return type pattern is already established in the codebase and will be reused without modification
