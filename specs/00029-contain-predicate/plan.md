# Implementation Plan: Predicate-Based Collection Containment

**Branch**: `00029-contain-predicate` | **Date**: 2026-06-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00029-contain-predicate/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Add a predicate-based containment overload to `GenericCollectionAssertions<T>`:

```csharp
public AndConstraint<GenericCollectionAssertions<T>> Contain(
  Func<T, bool> predicate,
  string because = "",
  params object[] becauseArgs);
```

The assertion passes when at least one element matches, fails with a clear message when none match, fails with the standard null-subject message when subject is null, and throws `ArgumentNullException` when `predicate` is null. The method returns `AndConstraint<GenericCollectionAssertions<T>>` to preserve fluent chaining.

This feature also formalizes machine-checkable diagnostics (expected/actual phrases, conditional expression/because lines), canonical null-subject failure contract values, and single-pass early-exit enumeration behavior.

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: BCL only (`System`, `System.Linq`, `System.Collections.Generic`)  
**Storage**: N/A  
**Testing**: xUnit in `tests/Assertivo.Tests`  
**Target Platform**: .NET assertion library (`net10.0`)  
**Project Type**: Library  
**Performance Goals**: Single-pass, early-exit predicate check (`Any(predicate)` equivalent behavior) with no unnecessary materialization; no second-pass enumeration  
**Constraints**: Zero third-party dependencies; preserve fluent readability; keep file under 300 lines; nullable enabled; warnings as errors; AOT-safe implementation  
**Scale/Scope**: One new public method in `GenericCollectionAssertions<T>`, targeted tests in `CollectionAssertionsTests`, updates to `contracts/public-api.md`, `quickstart.md`, and XML docs on the public overload

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate Check

| Gate | Status | Notes |
|------|--------|-------|
| Readability First (II.1) | PASS | API reads naturally: `orders.Should().Contain(o => o.Status == "Shipped")` |
| Zero Surprise (II.2) | PASS | Semantics clearly distinct from `ContainSingle`; one-or-more match passes |
| Pit of Success (II.3) | PASS | Explicit overload avoids boolean workaround (`Any(...).Should().BeTrue()`) |
| Zero Dependencies (II.4) | PASS | Uses only BCL and existing assertion infrastructure |
| API Design Overloads (VII.3) | PASS | Adds overload on existing `Contain` name rather than introducing unrelated method name |
| No Boolean Traps (VII.4) | PASS | No behavior toggles via bool parameters |
| Chaining Composable (VII.7) | PASS | Returns `AndConstraint<GenericCollectionAssertions<T>>` |
| Error Message Quality (III.4) | PASS | Failure contract defines expected/actual mismatch and supports `because` |
| File Length <= 300 (III.2) | PASS | `GenericCollectionAssertions.cs` currently 253 lines; addition remains under limit |

**Gate result: PASS. Proceeding to Phase 0 research.**

## Project Structure

### Documentation (this feature)

```text
specs/00029-contain-predicate/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── public-api.md    # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks - not created here)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
src/
└── Assertivo/
  └── Collections/
    └── GenericCollectionAssertions.cs   # add Contain(Func<T,bool>) overload

tests/
└── Assertivo.Tests/
  └── CollectionAssertionsTests.cs         # add predicate Contain tests

specs/
└── 00029-contain-predicate/
  └── contracts/
    └── public-api.md                    # public API contract update for the new overload
```

**Structure Decision**: Single-project library. No new source projects or namespaces; additive changes in the existing collection assertion type and test suite.

## Post-Design Constitution Check

| Gate | Status | Notes |
|------|--------|-------|
| Readability First (II.1) | PASS | API and examples remain natural-language fluent |
| Zero Surprise (II.2) | PASS | Contract explicitly documents pass/fail/null-subject/null-predicate behavior |
| Pit of Success (II.3) | PASS | Failing message is actionable; `because` is preserved |
| Zero Dependencies (II.4) | PASS | No external dependencies introduced |
| Chaining Composable (VII.7) | PASS | `AndConstraint<GenericCollectionAssertions<T>>` maintained |
| Documentation Standards (VIII) | PASS | Spec, plan, research, data model, quickstart, and public API contract are all produced |

**Post-design result: PASS. Ready for Phase 2 task generation.**

## Complexity Tracking

No constitution violations requiring justification.
