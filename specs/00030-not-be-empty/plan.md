# Implementation Plan: NotBeEmpty Assertion

**Branch**: `00030-not-be-empty` | **Date**: 2026-06-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00030-not-be-empty/spec.md`

## Summary

Add `NotBeEmpty` as the logical complement of `BeEmpty` on two assertion types:

```csharp
// StringAssertions
public AndConstraint<StringAssertions> NotBeEmpty(
    string because = "", params object[] becauseArgs);

// GenericCollectionAssertions<T>
public AndConstraint<GenericCollectionAssertions<T>> NotBeEmpty(
    string because = "", params object[] becauseArgs);
```

String variant passes for any subject that is not `""` (including `null`); fails with expected `"a non-empty string"` and actual `"\"\""` when the subject is the empty string.

Collection variant calls `GuardNull()` first (null subject fails), then passes when at least one element exists; fails with expected `"a non-empty collection"` and actual `"an empty collection"` for zero-element subjects.

Both methods are decorated with `[StackTraceHidden]` and return `AndConstraint<T>` for fluent chaining. The public API contract is updated accordingly.

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: BCL only (`System`, `System.Linq`, `System.Collections.Generic`)  
**Storage**: N/A  
**Testing**: xUnit in `tests/Assertivo.Tests`  
**Target Platform**: .NET assertion library (`net10.0`)  
**Project Type**: Library  
**Performance Goals**: Zero-allocation happy path for both methods; single-pass early-exit enumeration for collection variant  
**Constraints**: Zero third-party dependencies; nullable enabled; warnings as errors; AOT-safe; `GenericCollectionAssertions.cs` already exceeds 300 lines — `NotBeEmpty` goes into a new partial file  
**Scale/Scope**: Two new public methods across two existing types; no new types or namespaces introduced

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate Check

| Gate | Status | Notes |
|------|--------|-------|
| Readability First (II.1) | PASS | `value.Should().NotBeEmpty()` reads as natural English |
| Zero Surprise (II.2) | PASS | Strict logical complement of `BeEmpty`; null behavior documented and consistent with existing negation patterns |
| Pit of Success (II.3) | PASS | Additive change; no new types; compile-time guidance via IntelliSense |
| Zero Dependencies (II.4) | PASS | Uses only BCL and existing assertion infrastructure |
| Every Assertion Has a Negation (VII.2) | PASS | This feature directly satisfies the principle for `BeEmpty` |
| No Boolean Traps (VII.4) | PASS | No bool parameters |
| Chaining Composable (VII.7) | PASS | Returns `AndConstraint<T>` consistent with all existing assertion methods |
| Error Message Quality (III.4) | PASS | Failure contract defines expected/actual; `because` supported |
| File Length <= 300 (III.2) | PARTIAL | `GenericCollectionAssertions.cs` is at 307 lines (pre-existing overage from prior feature). Mitigation: `NotBeEmpty` will go into `GenericCollectionAssertions.NotBeEmpty.cs` — a new partial file, following the precedent of `GenericCollectionAssertions.Equal.cs`. `StringAssertions.cs` is at 117 lines; addition keeps it under limit. |

**Gate result: PASS with mitigation. Proceeding to Phase 0 research.**

## Project Structure

### Documentation (this feature)

```text
specs/00030-not-be-empty/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── public-api.md    # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit-tasks — not created here)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    ├── StringAssertions.cs                                # add NotBeEmpty after BeEmpty
    └── Collections/
        ├── GenericCollectionAssertions.cs                 # unchanged (already at 307 lines)
        └── GenericCollectionAssertions.NotBeEmpty.cs      # new partial file — add NotBeEmpty

tests/
└── Assertivo.Tests/
    ├── StringAssertionsTests.cs                           # add NotBeEmpty tests
    └── CollectionAssertionsTests.cs                       # add NotBeEmpty tests

specs/
└── 00030-not-be-empty/
    └── contracts/
        └── public-api.md                                  # public API contract for both new methods
```

**Structure Decision**: Single-project library. No new namespaces or projects. String `NotBeEmpty` is added inline to `StringAssertions.cs` (well within limit). Collection `NotBeEmpty` is extracted to a new partial file to avoid further growth of the already-overlimit main file, consistent with the existing `GenericCollectionAssertions.Equal.cs` pattern.

## Post-Design Constitution Check

| Gate | Status | Notes |
|------|--------|-------|
| Readability First (II.1) | PASS | API examples in quickstart read naturally |
| Zero Surprise (II.2) | PASS | Contract explicitly documents all pass/fail/null-subject behaviors for both types |
| Pit of Success (II.3) | PASS | Failure messages are actionable; `because` preserved |
| Zero Dependencies (II.4) | PASS | No external dependencies introduced |
| Every Assertion Has a Negation (VII.2) | PASS | `BeEmpty` ↔ `NotBeEmpty` symmetry established for both string and collection types |
| Chaining Composable (VII.7) | PASS | `AndConstraint<T>` maintained for both methods |
| Documentation Standards (VIII) | PASS | Spec, plan, research, quickstart, and public API contract all produced; XML docs required on both public methods |
| File Length <= 300 (III.2) | PASS | Partial-file extraction keeps `GenericCollectionAssertions.NotBeEmpty.cs` at ~30 lines; `StringAssertions.cs` stays under 140 lines |

**Post-design result: PASS. Ready for Phase 2 task generation.**

## Complexity Tracking

No constitution violations requiring justification. File-length mitigation (partial file extraction) follows existing project precedent.
