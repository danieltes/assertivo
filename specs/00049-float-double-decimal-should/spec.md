# Feature Specification: Float/Double/Decimal Should Dispatch

**Feature Branch**: `00049-float-double-decimal-should`  
**Created**: 2026-06-10  
**Status**: Draft  
**Input**: User description: "Add explicit Should() overloads for float, double, and decimal that dispatch to NumericAssertions<T> instead of the generic ObjectAssertions<T> fallback."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Range-Assert on Floating-Point Values (Priority: P1)

A developer writing a unit test for a calculation that returns a `double` or `float` wants to assert the result falls within an expected range. They call `.Should().BeGreaterThanOrEqualTo(...)` or `.Should().BeLessThan(...)` on the value and expect it to compile and produce a clear failure message when the condition is not met.

**Why this priority**: Range comparisons on floating-point results are the most common assertion gap. Without this, the library silently falls back to `ObjectAssertions<T>`, which has no range methods, and the consumer gets a misleading compile error pointing at the wrong type.

**Independent Test**: Can be tested entirely by writing a passing assertion (`(3.14).Should().BeGreaterThanOrEqualTo(0.0)`) and a failing assertion (`(3.14).Should().BeLessThan(1.0)`) and verifying compilation succeeds and the failure message is correct.

**Acceptance Scenarios**:

1. **Given** a `double` value of `3.14`, **When** `.Should().BeGreaterThanOrEqualTo(0.0)` is called, **Then** the assertion passes without compile error or runtime exception.
2. **Given** a `double` value of `0.5`, **When** `.Should().BeLessThan(1.0)` is called, **Then** the assertion passes.
3. **Given** a `double` value of `5.0`, **When** `.Should().BeLessThan(1.0)` is called, **Then** the assertion fails with a message containing the actual value (`5.0`), the expected threshold (`1.0`), and the caller expression.
4. **Given** a `float` value of `1.5f`, **When** `.Should().BeGreaterThanOrEqualTo(1.0f)` is called, **Then** the assertion passes.
5. **Given** a `decimal` value of `9.99m`, **When** `.Should().BeLessThan(10.0m)` is called, **Then** the assertion passes.

---

### User Story 2 - Equality Assertions on Floating-Point Types (Priority: P2)

A developer wants to assert exact equality or inequality on a `float`, `double`, or `decimal` value using the same `.Should().Be(expected)` / `.Should().NotBe(expected)` surface already available for `int` and `long`.

**Why this priority**: Equality is the baseline assertion. Ensuring consistency across all numeric types prevents surprises when switching from integer subjects to floating-point subjects.

**Independent Test**: Can be tested by asserting `(3.14).Should().Be(3.14)` passes and `(3.14).Should().Be(2.0)` fails with a clear mismatch message.

**Acceptance Scenarios**:

1. **Given** a `decimal` value of `100.00m`, **When** `.Should().Be(100.00m)` is called, **Then** the assertion passes.
2. **Given** a `float` value of `1.0f`, **When** `.Should().NotBe(2.0f)` is called, **Then** the assertion passes.
3. **Given** a `double` value of `1.0`, **When** `.Should().Be(2.0)` is called, **Then** the assertion fails with a message showing actual `1.0` and expected `2.0`.

---

### User Story 3 - Consistent NumericAssertions Surface for All Numeric Types (Priority: P3)

A developer exploring the API via IntelliSense expects that any numeric type — including `float`, `double`, and `decimal` — surfaces the same assertion methods as `int` and `long`. There is no invisible "second class" numeric tier.

**Why this priority**: Discoverability is a first-class concern per the project constitution. A developer should not need to know which numeric types are "supported"; the API must present a uniform surface.

**Independent Test**: Can be verified by inspecting the return type of `.Should()` for `float`, `double`, and `decimal` — all must resolve to `NumericAssertions<T>`, not `ObjectAssertions<T>`.

**Acceptance Scenarios**:

1. **Given** a `float` subject, **When** `.Should()` is called, **Then** the return type is `NumericAssertions<float>`.
2. **Given** a `double` subject, **When** `.Should()` is called, **Then** the return type is `NumericAssertions<double>`.
3. **Given** a `decimal` subject, **When** `.Should()` is called, **Then** the return type is `NumericAssertions<decimal>`.
4. **Given** any primitive numeric type (`int`, `long`, `float`, `double`, `decimal`), **When** `.Should()` is called, **Then** the generic `ObjectAssertions<T>` fallback is never reached.

---

### Edge Cases

- `double.NaN`, `double.PositiveInfinity`, and `double.NegativeInfinity` behavior in range assertions is out of scope for this feature; no acceptance tests for these values are required here.
- `decimal.MaxValue` and `decimal.MinValue` boundary values follow standard comparison rules; no special handling required.
- `float?`, `double?`, and `decimal?` (nullable variants) are out of scope for this feature.

## Clarifications

### Session 2026-06-10

- Q: Should NaN and infinity edge case behavior be specified and tested in this feature? → A: Out of scope; no tests required for NaN or infinity behavior in this feature.
- Q: Should the fix extend to other missing primitive numeric types (short, ushort, byte, sbyte, uint, ulong)? → A: No — scope is limited to float, double, and decimal only; other types may be addressed in a follow-up feature.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST provide an explicit `Should()` overload for `float` subjects that returns `NumericAssertions<float>`.
- **FR-002**: The library MUST provide an explicit `Should()` overload for `double` subjects that returns `NumericAssertions<double>`.
- **FR-003**: The library MUST provide an explicit `Should()` overload for `decimal` subjects that returns `NumericAssertions<decimal>`.
- **FR-004**: `BeGreaterThanOrEqualTo` MUST compile and execute correctly when called on `float`, `double`, and `decimal` subjects. *(explicit example of FR-006)*
- **FR-005**: `BeLessThan` MUST compile and execute correctly when called on `float`, `double`, and `decimal` subjects. *(explicit example of FR-006)*
- **FR-006**: All numeric assertion methods currently available for `int` and `long` (including `Be`, `NotBe`, and any range methods) MUST be equally available for `float`, `double`, and `decimal`. *(the implementation constraint — no changes to `NumericAssertions<T>` — is stated as SC-005)*
- **FR-007**: The generic `ObjectAssertions<T>` fallback MUST NOT be reachable for any of the five primitive numeric types (`int`, `long`, `float`, `double`, `decimal`).
- **FR-008**: Each new overload MUST include the `[CallerArgumentExpression]` parameter so assertion failure messages contain the caller expression.
- **FR-009**: Assertion failure messages for failing range comparisons MUST include the actual value, the expected threshold, and the caller expression.
- **FR-010**: Tests MUST cover the happy path (assertion passes) and failure path (assertion fails with a correctly formatted message) for each of the three new type overloads.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can call `BeGreaterThanOrEqualTo` and `BeLessThan` on `float`, `double`, and `decimal` subjects without compile errors.
- **SC-002**: 100% of primitive numeric types (`int`, `long`, `float`, `double`, `decimal`) resolve `.Should()` to `NumericAssertions<T>` — zero types fall through to `ObjectAssertions<T>`.
- **SC-003**: Assertion failure messages for range comparisons on the three new types contain the actual value, expected threshold, and caller expression, matching the message format produced for `int` and `long`.
- **SC-004**: Test coverage for the three new `Should()` overloads meets or exceeds the project minimum (90% line coverage for extension method surfaces, per the project constitution).
- **SC-005**: No change to `NumericAssertions<T>` internals is required — the feature is delivered entirely via new extension method overloads. *(see also FR-006, which expresses this as a method-availability constraint)*

## Assumptions

- `NumericAssertions<T>` is already generic and constrained to `IComparable<T>, IEquatable<T>`, which `float`, `double`, and `decimal` satisfy — no changes to the assertion type itself are needed.
- Nullable variants (`float?`, `double?`, `decimal?`) are out of scope for this feature; they will be addressed in a separate feature if needed.
- `double.NaN` and infinity values: NaN/infinity edge case behavior is explicitly out of scope for this feature; no tests for these values are required here. A future hardening feature may address special handling if needed.
- The fix applies only to `ShouldExtensions` (the `Should()` dispatch layer); no changes to assertion method logic or error message formatting are needed.
- Other missing primitive numeric types (`short`, `ushort`, `byte`, `sbyte`, `uint`, `ulong`) are out of scope for this feature and may be addressed in a follow-up.
- The project targets .NET Standard / multi-targeting as described in the constitution; the `[CallerArgumentExpression]` attribute requires C# 10 / .NET 6+, which is assumed to already be in use given `int` and `long` overloads already use it.
- Method chaining (`And`) is structurally guaranteed for all three new types by the `AndConstraint<NumericAssertions<T>>` return type already on every `NumericAssertions<T>` method — this is not a new capability and requires no separate formal requirement.
- Custom comparer test scenarios (`IEqualityComparer<T>`, `IComparer<T>` overloads) for the new types are out of scope for this feature. Comparer logic lives entirely in `NumericAssertions<T>` which is unchanged; testing custom comparers for `float`/`double`/`decimal` is deferred to a future dedicated test-coverage feature.
