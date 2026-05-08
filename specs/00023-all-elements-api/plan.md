# Implementation Plan: First-Class All-Elements Assertions

**Branch**: `00023-all-elements-api` | **Date**: 2026-05-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00023-all-elements-api/spec.md`

## Summary

Evolve the existing `AllSatisfy(Action<T>)` collection assertion into a first-class all-elements API with deterministic, bounded diagnostics and an index-aware overload. Implementation will preserve existing fluent behavior while adding: (1) `AllSatisfy(Action<T, int>)`, (2) aggregation semantics with explicit failure-type handling, (3) deterministic ordering guarantees, and (4) adaptive failing-index rendering rules. Design must remain zero-dependency, AOT-compatible, and testable across all collection dispatch paths.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (`net10.0`)  
**Primary Dependencies**: .NET BCL only (zero third-party dependencies)  
**Storage**: N/A  
**Testing**: xUnit in `tests/Assertivo.Tests` via `dotnet test` (+ constitution-aligned coverage thresholds in CI: core engine >=95% line and >=90% branch, extension surfaces >=90% line)  
**Target Platform**: Cross-platform .NET runtime (library package consumption)  
**Project Type**: Open-source assertion library (NuGet package)  
**Performance Goals**: Single-pass enumeration; no additional work on success path beyond inspector invocation and loop bookkeeping; bounded diagnostic detail expansion (first 50 detailed failures)  
**Constraints**: `TreatWarningsAsErrors`; nullable enabled; XML docs for public API; no third-party packages; file length guideline <= 300 lines for non-generated files  
**Scale/Scope**: One core source file enhancement (`GenericCollectionAssertions<T>`), one required internal helper file (`AllSatisfyFailureFormatter.cs`), targeted test additions (new dedicated all-elements test files for assertions, diagnostics, large-failure, enumeration-failure, index-aware behavior, and API-surface guards plus dispatch regression updates), repository-root quality configuration updates (`.editorconfig`), and documentation updates

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0 Gate

| Principle | Status | Notes |
|-----------|--------|-------|
| Readability First | âœ… PASS | Public API stays fluent and English-like: `collection.Should().AllSatisfy(...)`. |
| Zero Surprise | âœ… PASS | Failure ordering, framework-compatible thrown-type behavior, and truncation rules are explicitly specified in spec clarifications. |
| Pit of Success | âœ… PASS | Index-aware overload improves correctness and readability; misuse remains compile-time constrained by signatures. |
| Zero Dependencies | âœ… PASS | Implementation uses only existing library infrastructure + BCL. |
| Error Message Structure | âœ… PASS | Design keeps expected/actual + expression + because via existing failure pipeline. |
| Testing Standards | âœ… PASS | Spec contains explicit pass/fail/edge expectations and measurable outcomes SC-001..SC-011. |
| Performance & Throughput | âœ… PASS | Bounded detail expansion and single-pass design avoid unbounded diagnostic costs. |
| File Size / Maintainability | âš ï¸ WATCH | `GenericCollectionAssertions.cs` is currently ~257 lines; implementation must keep file <=300 and use the dedicated helper file for diagnostics formatting to stay within line/complexity limits. |

**Gate Result (Pre-Phase 0)**: PASS

### Post-Phase 1 Design Re-check

| Principle | Status | Notes |
|-----------|--------|-------|
| Readability First | âœ… PASS | Contract preserves `AllSatisfy(Action<T>)` and adds clear `AllSatisfy(Action<T, int>)`. |
| Zero Surprise | âœ… PASS | Deterministic ordering, explicit exception behavior, and adaptive index formatting are fully specified. |
| Pit of Success | âœ… PASS | API remains strongly typed and naturally discoverable from collection assertion context. |
| Zero Dependencies | âœ… PASS | No external interfaces or packages introduced in design artifacts. |
| Testing Standards | âœ… PASS | Quickstart and contract map directly to measurable test outcomes in SC-001..SC-011. |
| Performance & Throughput | âœ… PASS | Research fixes bounded-detail and range-compression strategy with deterministic limits. |

**Gate Result (Post-Phase 1)**: PASS

## Project Structure

### Documentation (this feature)

```text
specs/00023-all-elements-api/
â”œâ”€â”€ plan.md              # This file (/speckit.plan command output)
â”œâ”€â”€ research.md          # Phase 0 output (/speckit.plan command)
â”œâ”€â”€ data-model.md        # Phase 1 output (/speckit.plan command)
â”œâ”€â”€ quickstart.md        # Phase 1 output (/speckit.plan command)
â”œâ”€â”€ contracts/
â”‚   â””â”€â”€ public-api.md    # Phase 1 output (/speckit.plan command)
â”œâ”€â”€ checklists/
â”‚   â””â”€â”€ requirements.md  # Spec quality checklist (already created)
â””â”€â”€ tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
â””â”€â”€ Assertivo/
    â””â”€â”€ Collections/
        â”œâ”€â”€ GenericCollectionAssertions.cs      # Enhance AllSatisfy behavior + add Action<T,int> overload
        â””â”€â”€ AllSatisfyFailureFormatter.cs       # Required internal diagnostics formatter helper

tests/
â””â”€â”€ Assertivo.Tests/
    â”œâ”€â”€ CollectionAssertionsTests.cs           # Existing baseline tests (retain)
    â”œâ”€â”€ ShouldDispatchTests.cs                 # Dispatch compatibility checks (retain/update as needed)
    â”œâ”€â”€ AllSatisfyAssertionsTests.cs           # New focused tests for Action<T> behavior
    â”œâ”€â”€ AllSatisfyDiagnosticsTests.cs          # Ordering, because propagation, and thrown-type tests
    â”œâ”€â”€ AllSatisfyLargeFailureTests.cs         # 50/51 detail threshold and 100/101 index threshold tests
    â”œâ”€â”€ AllSatisfyEnumerationFailureTests.cs   # Source-enumeration exception passthrough tests
    â”œâ”€â”€ AllSatisfyIndexAwareTests.cs           # Action<T,int> functional behavior tests
    â”œâ”€â”€ AllSatisfyIndexAwareEdgeCasesTests.cs  # Index-aware null/empty/chaining edge cases
    â””â”€â”€ AllSatisfyApiSurfaceGuardsTests.cs     # FR-013/FR-014/FR-018 API-surface guard tests

README.md                                       # Add first-class assertion-body example and diagnostics notes
.editorconfig                                   # Enforce analyzer severity and complexity policy gates
```

**Structure Decision**: Single-project library layout. Keep behavioral flow in collection assertions, require a dedicated diagnostics formatter helper for maintainability, and isolate expanded all-elements behavioral tests in dedicated test files.

## Complexity Tracking

No unresolved design-time constitution violations. Implementation-time constitution gates are tracked by T030 (warnings), T031 (core line coverage), T034 (core branch coverage), T036 (extension-surface line coverage), T035 (cyclomatic complexity), and T040 (test naming convention compliance).
