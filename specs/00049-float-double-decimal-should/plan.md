# Implementation Plan: Float/Double/Decimal Should Dispatch

**Branch**: `00049-float-double-decimal-should` | **Date**: 2026-06-10 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `specs/00049-float-double-decimal-should/spec.md`

## Summary

Add three explicit `Should()` extension method overloads for `float`, `double`, and `decimal` in `Should.cs` so these types dispatch to `NumericAssertions<T>` instead of falling through to `ObjectAssertions<T>`. No changes to `NumericAssertions<T>` are needed — the generic constraint `T : struct, IComparable<T>, IEquatable<T>` is already satisfied by all three types. Scope is limited to the dispatch layer and test coverage.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0  
**Primary Dependencies**: xUnit 2.x (tests only); zero runtime dependencies  
**Storage**: N/A  
**Testing**: xUnit (`Assertivo.Tests` project)  
**Target Platform**: .NET 10.0 (library targets .NET Standard via multi-targeting per constitution)  
**Project Type**: Library  
**Performance Goals**: Simple value assertions ≥ 10 million ops/sec on a single core (constitution §6.3); zero-allocation happy path (constitution §6.2)  
**Constraints**: Zero third-party runtime dependencies; AOT-compatible; `NumericAssertions<T>` is already a `readonly struct` — new overloads must remain allocation-free  
**Scale/Scope**: 3 new extension method overloads + ~30 new test cases

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle / Rule | Status | Notes |
|---|---|---|
| §II.1 Readability First | ✅ Pass | New overloads follow the identical pattern as `int`/`long` overloads |
| §II.2 Zero Surprise | ✅ Pass | Fix eliminates the silent fallback to `ObjectAssertions<T>` — the current behaviour is the violation |
| §II.3 Pit of Success | ✅ Pass | After fix, incorrect combinations produce compile-time errors; `float`/`double`/`decimal` now route to the correct type |
| §II.4 Zero Dependencies | ✅ Pass | New code uses only BCL types; no new references |
| §III.1 Architecture — namespace | ✅ Pass | New overloads live in the same `ShouldExtensions` class in the root namespace |
| §III.2 Code Style — XML docs | ✅ Required | Each new public overload must have XML documentation comments |
| §III.3 Immutability | ✅ Pass | `NumericAssertions<T>` is already a `readonly struct`; new overloads return by value |
| §IV.1 Coverage — 90% extension | ✅ Required | Tests must cover happy path, failure path, and caller expression for each new type |
| §V.1 Discoverability | ✅ Pass | Fix directly implements the IntelliSense contract — correct assertion type surfaced |
| §VI.2 Zero-allocation happy path | ✅ Pass | `new NumericAssertions<T>(subject, caller)` is a struct stack allocation |
| §VII.6 Tight generic constraints | ✅ Pass | `float`, `double`, `decimal` all implement `IComparable<T>` and `IEquatable<T>` |

**Gate result: PASS — no violations.**

## Project Structure

### Documentation (this feature)

```text
specs/00049-float-double-decimal-should/
├── plan.md              ← This file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   └── public-api.md    ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit-tasks)
```

### Source Code

```text
src/Assertivo/
├── Should.cs                        ← ADD 3 overloads here (after long, before string)
└── Numeric/
    └── NumericAssertions.cs         ← NO CHANGES NEEDED

tests/Assertivo.Tests/
├── NumericAssertionsTests.cs        ← ADD tests here (float, double, decimal sections)
└── ShouldDispatchTests.cs           ← ADD dispatch-type verification tests if not already present
```

**Structure Decision**: Single project layout. Feature is confined to the dispatch layer (`Should.cs`) and the test project. No new files in `src/` are created.

## Complexity Tracking

No constitution violations — this section is not applicable.
