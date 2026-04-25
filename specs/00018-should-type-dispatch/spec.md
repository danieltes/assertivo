# Feature Specification: Should() Type-Aware Dispatch

**Feature Branch**: `00018-should-type-dispatch`  
**Created**: 2026-04-25  
**Status**: Draft  
**Input**: User description: "Add type-aware dispatch to the .Should() extension so that IReadOnlyList<T>, IReadOnlyCollection<T>, Action, Func<T>, Func<Task>, and dictionary types resolve to their appropriate specialized assertion classes rather than falling back to ObjectAssertions<T>."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assert on Read-Only Collection Interfaces (Priority: P1)

A developer writing tests against code that returns `IReadOnlyList<T>` or `IReadOnlyCollection<T>` (common return types from repositories, services, and domain aggregates) wants to use Assertivo's full collection assertion API without casting or wrapping the subject.

**Why this priority**: Read-only collection interfaces are the most common contract return types in production code. Developers encounter this gap immediately when testing any service layer that exposes results as `IReadOnlyList<T>` or `IReadOnlyCollection<T>`, making it the highest-impact gap.

**Independent Test**: Can be fully validated by calling `.Should()` on a variable declared as `IReadOnlyList<string>` or `IReadOnlyCollection<int>` and successfully invoking `.HaveCount()`, `.BeEmpty()`, `.Contain()`, `.ContainSingle()`, `.AllSatisfy()`, and `.BeEquivalentTo()` without any cast or additional wrapping.

**Acceptance Scenarios**:

1. **Given** a variable declared as `IReadOnlyList<T>`, **When** `.Should()` is called on it, **Then** the returned assertion object exposes all collection assertion methods.
2. **Given** a variable declared as `IReadOnlyCollection<T>`, **When** `.Should()` is called on it, **Then** the returned assertion object exposes all collection assertion methods.
3. **Given** an empty `IReadOnlyList<T>`, **When** `.Should().BeEmpty()` is called, **Then** the assertion passes without error.
4. **Given** a single-element `IReadOnlyList<T>`, **When** `.Should().ContainSingle()` is called, **Then** the assertion passes and `.Which` provides access to the single element for further chaining.
5. **Given** an `IReadOnlyCollection<T>` with known contents, **When** `.Should().BeEquivalentTo(expected)` is called, **Then** the assertion verifies element equality correctly.

---

### User Story 2 - Assert on Non-Async Delegate (Func<T>) (Priority: P2)

A developer who captures a non-async delegate as `Func<T>` and wants to assert that invoking it throws a specific exception type can use `.Should().Throw<TException>()` without needing to wrap it in an `Action` lambda.

**Why this priority**: Exception-throwing behavior is tested frequently. `Func<T>` is often the natural type when the code under test returns a value but can also throw. Without this dispatch, developers must write a manual `Action` wrapper, which obscures test intent.

**Independent Test**: Can be fully validated by calling `.Should()` on a `Func<string>` subject and successfully invoking `.Throw<InvalidOperationException>()` with `.Which` chaining for further assertion on the exception.

**Acceptance Scenarios**:

1. **Given** a variable declared as `Func<T>` that throws when invoked, **When** `.Should().Throw<TException>()` is called, **Then** the assertion passes and `.Which` provides the caught exception for chaining.
2. **Given** a variable declared as `Func<T>` that does not throw, **When** `.Should().Throw<TException>()` is called, **Then** the assertion fails with a meaningful error message.
3. **Given** a variable declared as `Func<T>`, **When** `.Should()` is called, **Then** the returned assertion object is the same type as what `Action.Should()` already returns, ensuring API consistency.

---

### User Story 3 - Assert on Read-Only Dictionary (Priority: P3)

A developer asserting on a variable declared as `IReadOnlyDictionary<TKey, TValue>` can access dictionary-specific assertions (key presence, value checks, count) directly via `.Should()` without receiving a generic object assertion that offers only null/equality checks.

**Why this priority**: Lower priority than collection dispatch because `IDictionary<K,V>` and `IReadOnlyDictionary<K,V>` are less universally used as return-type contracts than `IReadOnlyList<T>`, and workarounds (casting to `IDictionary`) are feasible but inelegant.

**Independent Test**: Can be fully validated by calling `.Should()` on a variable declared as `IReadOnlyDictionary<string, int>` and successfully invoking `.ContainKey()`, `.HaveCount()`, and `.NotBeNull()` without any cast.

**Acceptance Scenarios**:

1. **Given** a variable declared as `IReadOnlyDictionary<TKey, TValue>`, **When** `.Should()` is called, **Then** the returned assertion object exposes dictionary assertion methods.
2. **Given** a non-null `IReadOnlyDictionary<TKey, TValue>`, **When** `.Should().NotBeNull()` is called, **Then** the assertion passes without error.
3. **Given** an `IReadOnlyDictionary<TKey, TValue>` with a known key, **When** `.Should().ContainKey(key)` is called, **Then** the assertion passes.

---

### Edge Cases

- What happens when a null `IReadOnlyList<T>` has `.Should()` called on it? The null reference must be safely wrapped (not throw a NullReferenceException before the assertion runs) so that `.Should().BeNull()` can pass normally.
- What happens when a null `Func<T>` has `.Should()` called on it? An `ArgumentNullException` is thrown immediately at the `.Should()` call site — a null delegate cannot be meaningfully invoked for exception-assertion purposes.
- What happens when `Func<T>` returns a value successfully (no throw)? `.Should().NotThrow()` must pass. The return value is not surfaced; `NotThrow()` is terminal with no `.Which` accessor.
- What happens when a type implements both `IReadOnlyList<T>` and `IReadOnlyDictionary<K,V>`? A compiler ambiguity error is raised — this is the intended and acceptable behavior. No tie-break rule is defined; such types are non-existent in practice and the ambiguity surfaces a design smell.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST resolve `.Should()` on a subject declared as `IReadOnlyList<T>` to a collection assertion type that exposes `ContainSingle()`, `HaveCount()`, `BeEmpty()`, `Contain()`, `AllSatisfy()`, and `BeEquivalentTo()`.
- **FR-002**: The library MUST resolve `.Should()` on a subject declared as `IReadOnlyCollection<T>` to the same collection assertion type described in FR-001.
- **FR-003**: The library MUST resolve `.Should()` on a subject declared as `Func<T>` (exactly one type parameter, non-Task) to `ActionAssertions`, achieved by adapting the delegate internally as `() => subject()` within the extension method overload. No new assertion class is introduced. The returned `ActionAssertions` exposes `Throw<TException>()` with `.Which` chaining and `NotThrow()`. `NotThrow()` is terminal — it does NOT expose the delegate return value via `.Which`. Multi-parameter `Func` variants (e.g., `Func<T1, T2, TResult>`) are out of scope and continue to fall back to `ObjectAssertions<T>`. A null `Func<T>` subject MUST cause `.Should()` to throw `ArgumentNullException` immediately.
- **FR-004**: The library MUST resolve `.Should()` on a subject declared as `IReadOnlyDictionary<TKey, TValue>` to a dictionary assertion type that exposes at minimum `ContainKey()`, `HaveCount()`, `BeEmpty()`, and `NotBeNull()`.
- **FR-005**: All existing `.Should()` dispatch for `Action`, `Func<Task>`, `IEnumerable<T>`, `List<T>`, `T[]`, `IDictionary<TKey, TValue>`, `bool`, `int`, `long`, `string`, and the generic object fallback MUST continue to resolve correctly without regression. A dedicated test MUST verify that a subject declared as `IEnumerable<T>` still resolves to `GenericCollectionAssertions<T>` (not `ObjectAssertions<T>`) after the new overloads are added.
- **FR-006**: A null subject of any of the newly dispatched types MUST NOT throw a `NullReferenceException` when `.Should()` is called; the null must be forwarded into the assertion object so that null-checking assertions (e.g., `BeNull()`) work normally.
- **FR-007**: The `ContainSingle()` assertion on collection subjects MUST return a chaining object that provides access to the matched element via `.Which` for further assertions.
- **FR-008**: The `Throw<TException>()` assertion on delegate subjects MUST return a chaining object that provides access to the caught exception via `.Which` for further assertions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of the acceptance scenarios defined in User Stories 1, 2, and 3 pass as automated tests without any subject cast or manual wrapping.
- **SC-002**: Zero regressions in existing test suite — all previously passing tests continue to pass after the change. An explicit regression test confirms that `IEnumerable<T>.Should()` continues to return `GenericCollectionAssertions<T>`.
- **SC-003**: A developer can write a complete assertion chain (e.g., `list.Should().ContainSingle().Which.Should().Be("value")`) on an `IReadOnlyList<string>` in a single expression with no intermediate variable type declarations.
- **SC-004**: The dispatch mechanism resolves at compile time (not at runtime via casting), so mismatched assertion method calls produce compiler errors rather than runtime failures.

## Clarifications

### Session 2026-04-25 (round 2)

- Q: Should `Func<T>` dispatch wrap the delegate internally as `() => subject()` and pass it to the existing `ActionAssertions`, or should a new `FuncAssertions<T>` class be created? → A: Wrap internally — adapt `Func<T>` to `Action` inside the overload; no new assertion class needed.

### Session 2026-04-25

- Q: Should delegate dispatch cover only `Func<T>` (single type parameter) or also multi-parameter variants like `Func<T1,T2,...,TResult>`? → A: `Func<T>` only (single type parameter).
- Q: When `Func<T>.Should().NotThrow()` succeeds, does it expose the return value via `.Which` for further chaining? → A: No — `NotThrow()` is terminal (no `.Which`), consistent with the existing `Action` behavior.
- Q: When `.Should()` is called on a null `Func<T>` subject, what should happen? → A: Throw `ArgumentNullException` immediately at the `.Should()` call site.
- Q: When a type implements both `IReadOnlyList<T>` and `IReadOnlyDictionary<K,V>`, should the library define a tie-break or let the compiler raise an ambiguity error? → A: Compiler ambiguity error is acceptable — no tie-break rule is defined.
- Q: Should `IEnumerable<T>` → `GenericCollectionAssertions<T>` dispatch be explicitly covered by a dedicated regression test? → A: Yes — add an explicit regression test.

## Assumptions

- Dispatched types (`IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `Func<T>`, `IReadOnlyDictionary<TKey, TValue>`) will be added as explicit extension method overloads, consistent with the pattern already established in `ShouldExtensions` for other types.
- `Func<T>` dispatch (single type parameter only; multi-parameter `Func` variants are explicitly out of scope) adapts the delegate as `() => subject()` internally within the `.Should()` extension method overload and passes the resulting `Action` to the existing `ActionAssertions` constructor. No new assertion class is created. The return value is discarded because `NotThrow()` is terminal.
- The `IReadOnlyList<T>` and `IReadOnlyCollection<T>` overloads will be more specific than the existing `IEnumerable<T>` overload in the method resolution order, eliminating the fallback to `ObjectAssertions<T>`.
- Existing collection and dictionary assertion classes (`GenericCollectionAssertions<T>`, `GenericDictionaryAssertions<TKey, TValue>`) are already complete and do not require new methods as part of this feature.
- `Func<T>` where `T` is `Task` continues to be handled by the existing `Func<Task>` overload (no change to async delegate dispatch).
