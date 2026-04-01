<!--
  Sync Impact Report
  ===================================================================
  Version change: 0.0.0 (template) → 1.0.0
  Modified principles: N/A (initial adoption)
  Added sections:
    - I. Mission Statement
    - II. Core Principles (4 principles)
    - III. Code Quality Standards
    - IV. Testing Standards
    - V. Developer Experience
    - VI. Performance & Throughput Requirements
    - VII. API Design Rules
    - VIII. Documentation & Specification Standards
    - IX. Versioning & Compatibility
    - X. Governance
  Removed sections: None
  Templates requiring updates:
    - .specify/templates/plan-template.md ✅ no changes needed
      (Constitution Check section is dynamically populated)
    - .specify/templates/spec-template.md ✅ no changes needed
      (generic template; spec-driven workflow enforced by constitution)
    - .specify/templates/tasks-template.md ✅ no changes needed
      (generic template; task categorization is feature-driven)
  Follow-up TODOs: None
  ===================================================================
-->

# Assertivo Constitution

## I. Mission Statement

Deliver a high-performance, open-source .NET library that provides a
comprehensive set of extension methods enabling developers to write
readable, expressive, and maintainable assertions in unit tests using
natural language syntax, supporting both TDD and BDD workflows.

## II. Core Principles

### 1. Readability First

All public API surface MUST read as close to natural English as
possible. Assertions MUST be self-documenting so that test intent is
immediately clear without comments or external documentation.

```csharp
// Target expressiveness
result.Should().Be(42);
name.Should().StartWith("Jane");
collection.Should().ContainExactly(1, 2, 3);
order.Status.Should().Be(OrderStatus.Shipped)
    .And.Not.Be(OrderStatus.Cancelled);
```

### 2. Zero Surprise

The library MUST behave exactly as a developer would expect from
reading the assertion. No implicit conversions, hidden side effects,
or ambiguous overloads. When an assertion fails, the error message
MUST pinpoint the exact mismatch with both expected and actual values
clearly formatted.

### 3. Pit of Success

The API MUST guide developers toward correct usage. It MUST be
difficult or impossible to misuse. Incorrect combinations MUST produce
compile-time errors rather than runtime exceptions wherever feasible.

### 4. Zero Dependencies

The library MUST NOT take a dependency on any third-party library,
package, or framework — at compile time or runtime. All functionality
MUST be implemented from first principles using only the .NET Base
Class Library (BCL) and official .NET SDK components. This applies to
the shipping library and its Roslyn analyzers.

Development-only tooling (test frameworks, benchmark harnesses, build
scripts) used exclusively in the project's own repository
infrastructure is exempt, but MUST NEVER be referenced by the
library's production assemblies.

## III. Code Quality Standards

### 3.1 Architecture

- The library MUST target the broadest practical .NET surface (via
  .NET Standard or multi-targeting) to maximize ecosystem reach, with
  additional optimized code paths for the latest LTS release.
- All public types MUST be in a single root namespace with logical
  sub-namespaces for domain-specific extensions (e.g., `Collections`,
  `Strings`, `Numeric`, `DateTime`, `Async`).
- The core assertion engine MUST be separated from the fluent API
  surface to allow independent evolution and testability.
- No third-party dependencies. The shipped NuGet package MUST have
  zero `<PackageReference>` items that are not part of the official
  .NET SDK.
- The CI pipeline MUST verify that the published package has no
  transitive third-party dependencies.

### 3.2 Code Style & Consistency

- All public and protected members MUST have XML documentation
  comments.
- All code MUST pass static analysis with zero warnings (nullable
  reference types enabled, `TreatWarningsAsErrors`).
- Naming conventions MUST follow the .NET Runtime Coding Guidelines.
- Maximum cyclomatic complexity per method: **10**.
- Maximum file length: **300 lines** (excluding auto-generated code).
- One public type per file.

### 3.3 Immutability & Thread Safety

- Assertion chains MUST be immutable. Each chained call returns a new
  context; no shared mutable state.
- All assertion methods MUST be safe to call concurrently from
  parallel test runners without synchronization.

### 3.4 Error Messages

- Every assertion failure MUST produce a message containing:
  - The expected value or condition.
  - The actual value or condition.
  - The assertion expression (when available via caller attributes).
  - A human-readable description of the mismatch.
- Custom messages supplied by the user MUST append to, never replace,
  the structured failure output.

## IV. Testing Standards

### 4.1 Coverage & Confidence

- Minimum **95% line coverage** and **90% branch coverage** for the
  core assertion engine.
- Minimum **90% line coverage** for extension method surfaces.
- Every public method MUST have at least:
  - One positive test (assertion passes).
  - One negative test (assertion fails with correct message).
  - One edge-case test (null, empty, boundary values).
- Coverage metrics MUST be enforced in CI; builds MUST fail if
  thresholds are not met.

### 4.2 Test Organization

- Tests MUST follow the **Arrange-Act-Assert** pattern.
- Test class names MUST mirror the class under test with a `Tests`
  suffix (e.g., `StringAssertionExtensionsTests`).
- Test method names MUST follow the pattern:
  `MethodName_Scenario_ExpectedOutcome`.
- Tests MUST NOT depend on execution order, shared state, or external
  resources.
- Each test MUST complete in under **100 ms** unless explicitly marked
  as a performance benchmark.

### 4.3 Spec-Driven Workflow

All development MUST follow a strict spec-driven cycle:

1. **Spec** — Write a natural-language specification describing the
   expected behavior.
2. **Red** — Translate the spec into one or more failing tests.
3. **Green** — Write the minimal implementation to make the tests
   pass.
4. **Refactor** — Improve the implementation without changing
   behavior; all tests MUST remain green.
5. **Document** — Update XML docs and usage examples to reflect the
   new behavior.

No code may be merged without a corresponding spec and passing tests.
Specs are the source of truth.

### 4.4 Mutation Testing

- Mutation testing MUST be run on every release candidate.
- Mutation score MUST be **≥ 80%** for the core engine.
- Surviving mutants MUST be triaged and either killed with new tests
  or documented as acceptable.

## V. Developer Experience

### 5.1 Discoverability

- The primary entry point MUST be a single `.Should()` extension
  method on `object` (and generic `T`), returning a fluent assertion
  context.
- IntelliSense MUST surface only contextually valid assertions (e.g.,
  `.BeEmpty()` appears for strings and collections but not for
  integers).
- The library MUST ship with a **Roslyn analyzer** (implemented using
  only the official .NET Compiler SDK — no third-party analyzer
  helpers) that warns on common misuse patterns (e.g., calling
  `.Should()` without a terminal assertion).

### 5.2 Extensibility

- Third-party developers MUST be able to add custom assertion methods
  by implementing a well-defined interface or inheriting from a base
  assertion class.
- Extension points MUST be documented with examples in the project
  README and a dedicated extensibility guide.
- Custom assertions MUST integrate seamlessly with the existing fluent
  chain and error-reporting infrastructure.

### 5.3 Failure Diagnostics

- Stack traces in assertion failures MUST be trimmed to exclude
  internal library frames, pointing directly to the test code.
- When asserting on collections, diffs MUST highlight added, removed,
  and mismatched elements.
- When asserting on objects, diffs MUST show property-by-property
  comparison with paths to divergent values.

### 5.4 Framework Compatibility

- The library MUST integrate transparently with **xUnit**, **NUnit**,
  and **MSTest**.
- Framework-specific exception types MUST be thrown so that test
  runners report failures correctly (not as unhandled exceptions).
- An adapter layer MUST abstract framework-specific concerns; adding
  support for a new runner MUST require only a new adapter, not
  changes to core logic.
- Test framework adapters MUST detect the active runner at runtime via
  reflection or assembly probing — they MUST NOT take a compile-time
  dependency on any test framework package. If a framework is not
  present at runtime, the adapter MUST gracefully fall back to a
  generic assertion exception.

## VI. Performance & Throughput Requirements

### 6.1 Benchmarks

- The library MUST maintain a **BenchmarkDotNet** benchmark suite
  committed to the repository.
- Benchmarks MUST cover:
  - Simple value assertions (equality, comparison).
  - Collection assertions (contains, ordering, set operations).
  - Object graph assertions (deep equality).
  - String assertions (contains, regex, wildcard).

### 6.2 Allocation Budgets

- Simple value assertions (e.g., `x.Should().Be(5)`) on the happy
  path (assertion passes) MUST be **zero-allocation**.
- Collection assertions MUST NOT allocate beyond a single enumerator
  and the result object.
- No assertion method may allocate more than **1 KB** on the happy
  path for inputs under 1,000 elements.

### 6.3 Throughput Targets

- Simple assertions MUST execute at **≥ 10 million ops/sec** on a
  modern single core.
- Collection assertions on 1,000-element lists MUST execute at
  **≥ 100,000 ops/sec**.
- Deep object-graph equality on 100-property objects MUST execute at
  **≥ 50,000 ops/sec**.
- Performance MUST NOT regress by more than **5%** between releases;
  CI MUST enforce this via benchmark comparison.

### 6.4 Startup & Load

- The library MUST NOT use reflection at startup. Any
  reflection-based features (e.g., deep object comparison) MUST be
  lazy-initialized and cached.
- Assembly load time contribution MUST be **< 10 ms**.
- The library MUST be **AOT-compatible** (no `Type.MakeGenericType`,
  no unconstrained `Activator.CreateInstance`, no runtime code
  generation).
- Because the library has zero third-party dependencies, assembly load
  MUST involve only the single library assembly (plus optional
  analyzer), keeping the dependency graph minimal and startup
  predictable.

## VII. API Design Rules

1. **Fluent chains MUST be left-to-right readable.**
   `actual.Should().Be(expected)` — never
   `expected.ShouldEqual(actual)`.
2. **Every assertion SHOULD have a negation.**
   `Should().Be()` ↔ `Should().Not.Be()`.
   Negations may be delivered incrementally across features; the
   initial release of an assertion category is not blocked by missing
   negations.
3. **Overloads over new names.** Prefer `Be(int)`, `Be(string)` over
   `BeInt()`, `BeString()`.
4. **No boolean traps.** MUST NOT use `bool` parameters to toggle
   behavior. Use separate methods or a builder pattern.
5. **Async assertions MUST return `Task`.**
   `await result.Should().CompleteWithin(TimeSpan.FromSeconds(1))`.
6. **Generic constraints MUST be as tight as possible.** Do not offer
   `.BeGreaterThan()` for types that do not implement
   `IComparable<T>`.
7. **Chaining MUST be composable.**
   `x.Should().BePositive().And.BeLessThan(100)` MUST work naturally.
8. **Custom comparers MUST be injectable** at the assertion call site
   via an optional parameter, never via global state.

## VIII. Documentation & Specification Standards

- Every public API MUST have a corresponding **spec file** in the
  `/specs` directory before implementation begins.
- Spec files MUST use a structured format:
  ```
  Feature: <Name>
  As a <role>
  I want <behavior>
  So that <benefit>

  Scenario: <description>
    Given <precondition>
    When <action>
    Then <expected outcome>
  ```
- The project README MUST contain a quick-start guide that gets a
  developer from install to first assertion in under 60 seconds.
- A changelog MUST be maintained following Keep a Changelog format.
- Breaking changes MUST be documented with migration guides.

## IX. Versioning & Compatibility

- The library MUST follow **Semantic Versioning 2.0.0**.
- Public API changes MUST go through a deprecation cycle: deprecated
  in minor version N, removed no earlier than major version N+1.
- Binary compatibility MUST be verified using a tool (e.g.,
  Microsoft.DotNet.ApiCompat) in CI on every pull request.

## X. Governance

- All changes MUST be submitted via pull request with at least
  **one approving review**.
- Specs MUST be reviewed and approved **before** implementation
  begins.
- Performance-sensitive changes MUST include benchmark results in the
  PR description.
- This constitution may be amended by a proposal-and-review process;
  amendments require explicit consensus from maintainers.
- Constitution amendments MUST follow semantic versioning:
  - MAJOR: Backward-incompatible governance/principle removals or
    redefinitions.
  - MINOR: New principle/section added or materially expanded
    guidance.
  - PATCH: Clarifications, wording, typo fixes, non-semantic
    refinements.

*This constitution is the governing document for all design,
implementation, and contribution decisions in this project. When in
doubt, refer to the principles above. Readability, correctness, and
performance — in that order.*

**Version**: 1.0.0 | **Ratified**: 2026-03-31 | **Last Amended**: 2026-03-31
