# Implementation Plan: Ordered Sequence Equality Assertion

**Branch**: `00025-sequence-equal` | **Date**: 2026-05-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00025-sequence-equal/spec.md`

## Summary

Add an `Equal` assertion to `GenericCollectionAssertions<T>` that verifies two sequences contain identical elements in the same order. A count mismatch is reported first; if counts match, the first differing index is reported. A `params T[]` convenience overload delegates to the primary `IEnumerable<T>` overload. Because `GenericCollectionAssertions.cs` is at 278 lines and the new methods would breach the 300-line constitution limit, the implementation splits into a `partial struct` file `GenericCollectionAssertions.Equal.cs`.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (multi-target, same as existing codebase)  
**Primary Dependencies**: BCL only — zero third-party dependencies (constitution requirement)  
**Storage**: N/A  
**Testing**: xUnit 2.9 via `Assertivo.Tests` project  
**Target Platform**: .NET library (netstandard2.0 + net8.0)  
**Project Type**: Library  
**Performance Goals**: Collection assertions ≥ 100,000 ops/sec on 1,000-element lists (constitution §6.3); happy-path allocation bounded by two `List<T>` + one `AndConstraint<T>` (constitution §6.2 collection budget)  
**Constraints**: Zero deps; AOT-compatible; max 300 lines/file; max cyclomatic complexity 10; `TreatWarningsAsErrors`; nullable reference types enabled  
**Scale/Scope**: Two new public methods on `GenericCollectionAssertions<T>`; one new partial source file; one new test file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| Zero third-party dependencies | PASS | Implementation uses only BCL (`List<T>`, `EqualityComparer<T>`, `ArgumentNullException`) |
| Single root namespace | PASS | `Assertivo.Collections` — existing namespace |
| AOT-compatible | PASS | No reflection, no `Activator`, no runtime code gen |
| Max 300 lines/file | **NEEDS ACTION** | `GenericCollectionAssertions.cs` is at 278 lines; adding ~50 lines for `Equal` breaches limit. **Resolution: declare existing struct `partial` and split `Equal` into `GenericCollectionAssertions.Equal.cs`** |
| Max cyclomatic complexity 10 | PASS | `Equal` core loop has complexity ≤ 4 |
| XML docs on all public members | PASS | All new public methods will carry `<summary>/<param>/<returns>` |
| Failure messages include expected + actual | PASS | Count mismatch and element mismatch both use `MessageFormatter.Fail` with explicit expected/actual strings |
| `[StackTraceHidden]` on public assertion methods | PASS | Applied to both overloads following existing pattern |
| Assertion chain returns `AndConstraint<T>` | PASS | Both overloads return `AndConstraint<GenericCollectionAssertions<T>>` |

**Post-research re-evaluation**: No new violations found. File-split strategy resolves the 300-line gate.

## Project Structure

### Documentation (this feature)

```text
specs/00025-sequence-equal/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── public-api.md    # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    └── Collections/
        ├── GenericCollectionAssertions.cs          # Add `partial` keyword to struct declaration
        └── GenericCollectionAssertions.Equal.cs    # NEW — Equal overloads (partial struct continuation)

tests/
└── Assertivo.Tests/
    └── EqualAssertionsTests.cs                     # NEW — full test coverage for Equal
```

**Structure Decision**: Single-project library. No new projects, namespaces, or sub-directories. The only structural change is splitting `GenericCollectionAssertions<T>` across two files using `partial struct`, consistent with how large assertion classes are managed when they approach the 300-line limit.

## Complexity Tracking

*No constitution violations requiring justification. The file-split is a structural compliance measure, not a complexity addition.*
