# Implementation Plan: Assertion Library Core

**Branch**: `00001-assertion-library-core` | **Date**: 2026-03-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00001-assertion-library-core/spec.md`

## Summary

Implement a fluent, strongly-typed assertion library for .NET 10 test suites, distributed as a NuGet package. The library provides a `.Should()` extension-method entry point that returns typed assertion objects (7 categories: object, boolean, numeric, string, collection, dictionary, exception). All assertion methods support fluent chaining via `AndConstraint`/`AndWhichConstraint`, optional `because` reason parameters, and optional custom comparer injection. The core assertion engine is separated from the fluent API surface. Zero external dependencies.

## Technical Context

**Language/Version**: C# 14 / .NET 10
**Primary Dependencies**: None (zero third-party dependencies per constitution)
**Storage**: N/A
**Testing**: xUnit (development-only, not referenced by production assemblies)
**Target Platform**: .NET 10 (single TFM; cross-platform via .NET runtime)
**Project Type**: Library (NuGet package)
**Performance Goals**: ≥ 10M ops/sec simple assertions; zero-allocation happy path for value assertions
**Constraints**: Zero-allocation happy path; < 1 KB allocation for collection assertions under 1,000 elements; AOT-compatible; no reflection at startup
**Scale/Scope**: 22 assertion methods across 7 categories; ~15-20 public types

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Constitution Rule | Status | Notes |
|---|---|---|---|
| 1 | **Readability First** (II.1) — API reads as natural English | ✅ PASS | `.Should().Be(42)` pattern in spec |
| 2 | **Zero Surprise** (II.2) — No implicit conversions; clear failure messages | ✅ PASS | FR-013 mandates expected/actual/reason in failures |
| 3 | **Pit of Success** (II.3) — Compile-time errors for misuse | ✅ PASS | Typed assertion objects prevent invalid method calls; `BeSameAs` constrained to reference types |
| 4 | **Zero Dependencies** (II.4) — No third-party packages | ✅ PASS | FR-015 explicitly requires zero deps |
| 5 | **Single root namespace** (III.1) — Sub-namespaces for domains | ✅ PASS | Planned: `Assertivo`, `Assertivo.Collections`, `Assertivo.Exceptions`, `Assertivo.Primitives` |
| 6 | **Core engine separated from fluent API** (III.1) | ✅ PASS | Two layers planned: engine (failure reporting, message formatting) + fluent surface (assertion classes, extension methods) |
| 7 | **XML docs on all public/protected members** (III.2) | ✅ PASS | Will enforce in implementation |
| 8 | **TreatWarningsAsErrors, nullable enabled** (III.2) | ✅ PASS | Will set in .csproj |
| 9 | **Max cyclomatic complexity 10** (III.2) | ✅ PASS | Simple assertion methods; no complex branching expected |
| 10 | **Max 300 lines per file** (III.2) | ✅ PASS | One type per file, small methods |
| 11 | **Immutable chains, thread-safe** (III.3) | ✅ PASS | FR-021, FR-022 |
| 12 | **Error messages: expected + actual + expression + description** (III.4) | ✅ PASS | FR-013 |
| 13 | **Custom messages append, not replace** (III.4) | ✅ PASS | FR-012 |
| 14 | **Spec-driven workflow** (IV.3) | ✅ PASS | Spec written before plan |
| 15 | **Fluent L→R readable** (VII.1) | ✅ PASS | `actual.Should().Be(expected)` pattern |
| 16 | **Every assertion SHOULD have negation** (VII.2) | ✅ PASS | Spec includes `NotBeNull`, `NotContain`, `NotBeNullOrEmpty`. Constitution VII.2 is SHOULD (phased delivery allowed). Additional negations will expand in subsequent features. |
| 17 | **Overloads over new names** (VII.3) | ✅ PASS | `Be(int)`, `Be(string)` pattern |
| 18 | **No boolean traps** (VII.4) | ✅ PASS | No bool parameters in spec |
| 19 | **Async assertions return Task** (VII.5) | ✅ PASS | FR-009 returns `Task<ExceptionAssertions<T>>` |
| 20 | **Tight generic constraints** (VII.6) | ✅ PASS | `NumericAssertions<T>` constrained to `IComparable<T>` |
| 21 | **Composable chaining** (VII.7) | ✅ PASS | FR-010 (AndConstraint / AndWhichConstraint) |
| 22 | **Injectable custom comparers** (VII.8) | ✅ PASS | Clarification resolved: optional `IEqualityComparer<T>` / `IComparer<T>` on relevant methods |
| 23 | **AOT-compatible** (VI.4) | ✅ PASS | No `MakeGenericType`, no `Activator.CreateInstance`, no codegen |
| 24 | **.NET 10 single TFM** (user override) | ⚠️ DEVIATION | Constitution says "broadest practical .NET surface"; user explicitly requested .NET 10 only. Accepted — .NET 10 is current LTS; multi-targeting can be added later. |
| 25 | **Coverage thresholds enforced in CI** (IV.1) | ✅ PASS | SC-007 + T040 (Coverlet configured to fail below 95% line / 90% branch) |
| 26 | **Mutation testing ≥ 80%** (IV.4) | ⚠️ DEFERRED | Constitution requires mutation testing on release candidates; deferred to CI/release infrastructure feature |
| 27 | **Roslyn analyzer for misuse warnings** (V.1) | ⚠️ DEFERRED | Spec explicitly defers to a separate deliverable; core exposes required extension points |
| 28 | **Stack traces trimmed to test code** (V.3) | ✅ PASS | FR-023 + T008 (`[StackTraceHidden]` on internal failure-path methods) |
| 29 | **Allocation budgets** (VI.2) | ✅ PASS | SC-006 + T044 (BenchmarkDotNet spot-check for zero-allocation happy path) |

**Gate result**: ✅ PASS (4 acknowledged items: partial negation coverage is in-scope for future features; .NET 10 single TFM is user-directed override; mutation testing deferred to CI feature; Roslyn analyzer deferred to separate deliverable)

## Project Structure

### Documentation (this feature)

```text
specs/00001-assertion-library-core/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    ├── Assertivo.csproj
    ├── AssertionFailedException.cs
    ├── AssertionFailure.cs
    ├── AssertionConfiguration.cs
    ├── MessageFormatter.cs
    ├── Should.cs                          # Extension method entry points
    ├── Primitives/
    │   ├── AndConstraint.cs
    │   └── AndWhichConstraint.cs
    ├── ObjectAssertions.cs
    ├── BooleanAssertions.cs
    ├── StringAssertions.cs
    ├── Numeric/
    │   └── NumericAssertions.cs
    ├── Collections/
    │   ├── GenericCollectionAssertions.cs
    │   └── GenericDictionaryAssertions.cs
    └── Exceptions/
        ├── ActionAssertions.cs
        ├── AsyncFunctionAssertions.cs
        └── ExceptionAssertions.cs

tests/
└── Assertivo.Tests/
    ├── Assertivo.Tests.csproj
    ├── ObjectAssertionsTests.cs
    ├── BooleanAssertionsTests.cs
    ├── StringAssertionsTests.cs
    ├── NumericAssertionsTests.cs
    ├── CollectionAssertionsTests.cs
    ├── DictionaryAssertionsTests.cs
    ├── ActionAssertionsTests.cs
    ├── AsyncFunctionAssertionsTests.cs
    ├── ExceptionAssertionsTests.cs
    ├── ChainingTests.cs
    └── MessageFormatterTests.cs
```

**Structure Decision**: Single-project library layout. The `src/Assertivo/` project produces the NuGet package. `tests/Assertivo.Tests/` is an xUnit test project referencing the library via project reference. Sub-folders within `src/Assertivo/` group assertion classes by domain (Numeric, Collections, Exceptions, Primitives) while keeping the root namespace flat (`Assertivo`) with sub-namespaces for domain groupings.

## Complexity Tracking

| Deviation | Why Accepted | Alternative Rejected Because |
|-----------|-------------|------------------------------|
| .NET 10 only (no multi-TFM) | User explicitly requested .NET 10; simplifies build/CI | Multi-targeting adds complexity with no immediate consumer need |
