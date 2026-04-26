# Implementation Plan: Collection and Dictionary Null-Guard Assertions

**Branch**: `00021-fix-dict-not-be-null` | **Date**: 2026-04-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00021-fix-dict-not-be-null/spec.md`

## Summary

Add `BeNull()` and `NotBeNull()` to both `GenericDictionaryAssertions<TKey, TValue>` and `GenericCollectionAssertions<T>`, bringing them to full symmetry with `ObjectAssertions<T>`. Both methods follow the existing `MessageFormatter.Fail` pattern, accept an optional `because` parameter, and return `AndConstraint<T>` to allow fluent chaining (e.g. `.NotBeNull().And.ContainKey(...)`). No changes to `Should.cs` or the inheritance hierarchy are needed.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0  
**Primary Dependencies**: .NET BCL only (zero third-party; AOT-compatible)  
**Storage**: N/A  
**Testing**: xUnit via `Assertivo.Tests` (dotnet test)  
**Target Platform**: .NET 10.0 (`net10.0`), `IsAotCompatible=true`, `TreatWarningsAsErrors=true`  
**Project Type**: Open-source .NET assertion library (NuGet package)  
**Performance Goals**: Happy-path (passing assertion) MUST be zero-allocation; `≥ 10M ops/sec` simple assertions  
**Constraints**: No allocations on passing path; `<200ms` per test; XML doc on every public member; zero nullable warnings  
**Scale/Scope**: Two files modified (one add per type); four new test methods minimum per type (positive, negative, `because`, chaining)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status | Notes |
|-----------|-------------|--------|-------|
| **Readability First** | Public API reads as natural English | ✅ PASS | `.Should().NotBeNull()` / `.Should().BeNull()` are idiomatic English assertions |
| **Zero Surprise** | Failures pinpoint the mismatch with expected + actual | ✅ PASS | Uses `MessageFormatter.Fail("not <null>", "<null>", …)` exactly as `ObjectAssertions<T>` does |
| **Pit of Success** | Correct usage obvious; misuse caught at compile time where possible | ✅ PASS | No behavior changes; new overloads fill a gap that currently causes a compile error |
| **Zero Dependencies** | No third-party packages added | ✅ PASS | Change is purely additive within existing source files; no new references |
| **Code Quality 3.2** | XML doc on every public member; zero nullable warnings; `TreatWarningsAsErrors` | ✅ PASS | Both new methods will carry `<summary>` XML doc; subject type is nullable (`IEnumerable?` / `IEnumerable<KVP>?`); return is non-null |
| **Code Quality 3.3** | Immutable chains; no shared mutable state | ✅ PASS | Returns `new AndConstraint<T>(this)` — same pattern as all existing methods |
| **Code Quality 3.4** | Failure message contains expected, actual, expression, and because | ✅ PASS | `MessageFormatter.Fail` produces all four fields |
| **Testing 4.1** | Positive + negative + edge-case test per method | ✅ PASS | Plan includes passing, failing, `because`, and chaining tests for each method |
| **Testing 4.2** | `MethodName_Scenario_ExpectedOutcome` naming; AAA pattern | ✅ PASS | Test names follow `NotBeNull_NonNullSubject_Passes` etc. |
| **Performance 6.2** | Happy-path zero-allocation | ✅ PASS | `new AndConstraint<T>(this)` is a single struct copy on stack — no heap allocation; subject is already captured in the struct |
| **API Design 7.2** | Assertions should have negations (`Be` ↔ `Not.Be`) | ✅ PASS | Adding both `BeNull` and `NotBeNull` together satisfies symmetry; no `.Not.` chaining pattern used elsewhere in this library |
| **API Design 7.7** | Chaining must be composable | ✅ PASS | `NotBeNull()` returns `AndConstraint<GenericDictionaryAssertions<K,V>>` whose `.And` exposes `.ContainKey(…)` |

**Constitution gate: PASS — no violations. Proceeding to Phase 0.**

## Project Structure

### Documentation (this feature)

```text
specs/00021-fix-dict-not-be-null/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    └── Collections/
        ├── GenericCollectionAssertions.cs   ← add BeNull() + NotBeNull()
        └── GenericDictionaryAssertions.cs   ← add BeNull() + NotBeNull()

tests/
└── Assertivo.Tests/
    ├── CollectionAssertionsTests.cs         ← add 8 new tests (4 per method)
    └── DictionaryAssertionsTests.cs         ← add 8 new tests (4 per method)
```

**Structure Decision**: Single-project library (Option 1). No new files created; all changes are additive within the four existing files listed above.

## Complexity Tracking

> **No violations.** Constitution Check passed fully; no justifications required.
