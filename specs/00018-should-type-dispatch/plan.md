# Implementation Plan: Should() Type-Aware Dispatch

**Branch**: `00018-should-type-dispatch` | **Date**: 2026-04-25 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/00018-should-type-dispatch/spec.md`

## Summary

Add three explicit `.Should()` extension method overloads to `ShouldExtensions` in
`src/Assertivo/Should.cs` so that `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, and
`Func<T>` (single type parameter, non-Task) dispatch to their correct specialized
assertion classes at compile time. `Func<T>` is adapted internally to `Action` via
`() => subject()` before being passed to the existing `ActionAssertions` constructor.
No new assertion classes are required. `IReadOnlyDictionary<TKey,TValue>` already has
an explicit overload and is not in scope.

## Technical Context

**Language/Version**: C# 13 / .NET 10  
**Primary Dependencies**: .NET BCL only (zero third-party; constitution constraint)  
**Storage**: N/A  
**Testing**: xUnit 2.x (`Assertivo.Tests`)  
**Target Platform**: .NET 10 (library; multi-targeting is a separate concern)  
**Project Type**: Open-source .NET assertion library (NuGet package)  
**Performance Goals**: New `Func<T>` overload allocates exactly one `Action` delegate on the heap per call; all collection overloads are zero-allocation pass-throughs  
**Constraints**: AOT-compatible (no reflection, no `MakeGenericType`); zero third-party dependencies; `TreatWarningsAsErrors`; nullable reference types enabled; XML docs on all public members  
**Scale/Scope**: Three new extension method overloads in one existing file (~67 LOC); one new test file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **Readability First** | ✅ PASS | `readonlyList.Should().HaveCount(3)` reads naturally |
| **Zero Surprise** | ✅ PASS | Dispatch is compile-time; no implicit conversions hidden from caller |
| **Pit of Success** | ✅ PASS | Incorrect method calls (e.g., `.HaveCount()` on non-collection) produce compiler errors |
| **Zero Dependencies** | ✅ PASS | No new packages; the `() => subject()` lambda uses only BCL |
| **Single type per file** | ✅ PASS | No new types introduced |
| **XML doc comments** | ⚠️ REQUIRED | All three new overloads must have `<summary>` XML docs matching the existing pattern in `Should.cs` |
| **Zero warnings / nullable** | ✅ PASS | Collection overloads use nullable `IReadOnlyList<T>?` / `IReadOnlyCollection<T>?`; `Func<T>` is non-nullable (ArgumentNullException guard) |
| **Cyclomatic complexity ≤ 10** | ✅ PASS | All three overloads are single-expression methods |
| **File length ≤ 300 lines** | ✅ PASS | `Should.cs` is ~67 LOC; three new overloads keep it well under 300 |
| **95% line / 90% branch coverage** | ⚠️ REQUIRED | New overloads must be covered by positive, negative, and edge-case tests |
| **AOT compatible** | ✅ PASS | No reflection; generic constraints are static |
| **Test naming** | ⚠️ REQUIRED | `MethodName_Scenario_ExpectedOutcome` pattern; test class mirrors class under test |
| **IReadOnlyDictionary overload** | ℹ️ ALREADY EXISTS | `Should.cs` already has an explicit `IReadOnlyDictionary<TKey,TValue>` overload — User Story 3 (P3) is pre-satisfied; no code change needed for it |

**Post-design re-check**: Re-run this table after Phase 1 to confirm no violations introduced.

## Project Structure

### Documentation (this feature)

```text
specs/00018-should-type-dispatch/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── public-api.md    # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — not created here)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    └── Should.cs        # +3 new extension method overloads (IReadOnlyList<T>,
                         #   IReadOnlyCollection<T>, Func<T>)

tests/
└── Assertivo.Tests/
    └── ShouldDispatchTests.cs   # NEW — compile-time dispatch regression tests
                                 #   for all Should() overloads including IEnumerable<T>
```

**Structure Decision**: Single-project library. All changes are confined to the existing `src/Assertivo/Should.cs` and a new test file. No new source files in `src/`.
