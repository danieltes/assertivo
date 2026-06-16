# Implementation Plan: Add BeGreaterThan and BeLessThanOrEqualTo to NumericAssertions

**Branch**: `00050-numeric-comparisons` | **Date**: 2026-06-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00050-numeric-comparisons/spec.md`

## Summary

Add two missing comparison methods — `BeGreaterThan` and `BeLessThanOrEqualTo` — to `NumericAssertions<T>` to complete the four-method symmetric comparison set. Both methods follow the exact same pattern as the two existing methods (`BeGreaterThanOrEqualTo` and `BeLessThan`) already present in `src/Assertivo/Numeric/NumericAssertions.cs`. No structural changes, new files, or additional dependencies are required.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0  
**Primary Dependencies**: None (zero external dependencies — constitution requirement)  
**Storage**: N/A  
**Testing**: xUnit 2.9.3 with Coverlet; coverage thresholds enforced at 93% line / 90% branch  
**Target Platform**: net10.0; AOT-compatible (`IsAotCompatible=true`)  
**Project Type**: Library (NuGet package)  
**Performance Goals**: ≥10M ops/sec simple assertions; zero-allocation on the happy path (constitution §VI.2–VI.3)  
**Constraints**: Zero third-party dependencies; `TreatWarningsAsErrors`; XML docs on all public members; one public type per file; max 300 lines per file; `[StackTraceHidden]` on all assertion methods  
**Scale/Scope**: 2 methods added to 1 existing 90-line file (resulting file remains well under 300 lines)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle / Rule | Status | Notes |
|---|---|---|
| **Readability First** | ✅ Pass | `BeGreaterThan` / `BeLessThanOrEqualTo` read as natural English |
| **Zero Surprise** | ✅ Pass | `BeGreaterThan` fails on equal (strict `>`); `BeLessThanOrEqualTo` passes on equal (inclusive `<=`) — exactly what the names imply |
| **Pit of Success** | ✅ Pass | No boolean traps; separate named methods enforce correct semantics at compile time |
| **Zero Dependencies** | ✅ Pass | Uses only BCL types (`IComparer<T>`, `Comparer<T>.Default`) |
| **XML Docs** | ✅ Pass | Summary XML doc required on each new method |
| **TreatWarningsAsErrors** | ✅ Pass | No new nullable or warning risks; pattern mirrors existing clean methods |
| **AOT Compatibility** | ✅ Pass | No reflection; generic constraint (`struct, IComparable<T>`) fully AOT-safe |
| **File Length ≤ 300 lines** | ✅ Pass | Current file: 90 lines + ~25 new lines = ~115 total |
| **One public type per file** | ✅ Pass | Adding methods to existing type, no new type created |
| **API Rule 6 — tight generic constraint** | ✅ Pass | `T : struct, IComparable<T>` already on `NumericAssertions<T>`; `IComparer<T>` always resolvable |
| **API Rule 8 — injectable comparer** | ✅ Pass | Optional `IComparer<T>? comparer` parameter on both methods |
| **API Rule 2 — negation** | ✅ Deferred | Negation variants (`Not.BeGreaterThan`, `Not.BeLessThanOrEqualTo`) out of scope; constitution permits incremental delivery |
| **StackTraceHidden** | ✅ Pass | Both methods will be decorated with `[StackTraceHidden]` |
| **Testing — positive/negative/edge per method** | ✅ Pass | Spec covers pass, fail, boundary, custom comparer, and because per method |
| **Coverage thresholds (93/90)** | ✅ Pass | New tests aligned to spec acceptance scenarios; MessageFormatter.Fail behavior (always-throw) matches existing Coverlet workaround pattern |

No violations. No Complexity Tracking entry required.

## Project Structure

### Documentation (this feature)

```text
specs/00050-numeric-comparisons/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← N/A (no entities)
├── contracts/
│   └── public-api.md    ← Phase 1 output
├── quickstart.md        ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
src/Assertivo/
└── Numeric/
    └── NumericAssertions.cs     ← add BeGreaterThan and BeLessThanOrEqualTo here

tests/Assertivo.Tests/
└── NumericAssertionsTests.cs    ← add test cases for both new methods
```

**Structure Decision**: Single-project library layout. Changes are confined to one production file and one test file. No new files are created in the library source.
