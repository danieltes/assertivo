# Implementation Plan: Should() Entry Point for Task Subjects

**Branch**: `00026-task-should-assertions` | **Date**: 2026-05-17 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/00026-task-should-assertions/spec.md`

## Summary

Add a `Task.Should()` extension method that returns a new `TaskAssertions` readonly struct. The struct exposes a single async method `ThrowAsync<TException>()` that awaits the already-started task, applies the same single-inner `AggregateException` unwrapping used by `AsyncFunctionAssertions`, and returns `ExceptionAssertions<TException>` for further chaining via `.Which`. Failure messages follow the existing `MessageFormatter` pattern; the wrong-type failure message includes the actual exception type name and its `.Message` text. A null `Task` subject is forwarded into the struct and produces an `AssertionFailedException` from inside `ThrowAsync` (not at the `.Should()` call site).

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: BCL only — `System.Threading.Tasks.Task`, `System.AggregateException`, `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute`, `System.Diagnostics.StackTraceHiddenAttribute`  
**Storage**: N/A  
**Testing**: xUnit 2.9.3 + Coverlet (line ≥ 93%, branch ≥ 90%)  
**Target Platform**: .NET 10 class library (`net10.0`)  
**Project Type**: Open-source assertion library  
**Performance Goals**: Each `ThrowAsync` call must complete within 100 ms (test timeout budget per constitution §IV.2)  
**Constraints**: Zero third-party dependencies in production assembly; `TreatWarningsAsErrors`; nullable reference types enabled; max 300 lines per file; max cyclomatic complexity 10  
**Scale/Scope**: 1 new production file, 1 new test file, 1 line changed in `Should.cs`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| Zero Dependencies | No new `<PackageReference>` in production assembly | ✅ PASS | Uses BCL `Task`, `AggregateException`, `CallerArgumentExpressionAttribute`, `StackTraceHiddenAttribute` only |
| Sub-namespace structure | New type in `Assertivo.Exceptions` | ✅ PASS | `TaskAssertions` placed alongside `AsyncFunctionAssertions` |
| One public type per file | `TaskAssertions.cs` contains only `TaskAssertions` | ✅ PASS | |
| XML docs on all public members | All public types/methods/properties documented | ✅ PASS | Required; reflected in contracts |
| Zero warnings / Nullable enabled | `Task?` stored; no suppression needed | ✅ PASS | Null is valid and intentionally stored |
| Max cyclomatic complexity 10 | `ThrowAsync` has 4 branches (null, no-throw, unwrap, wrong-type) | ✅ PASS | Well under limit |
| Max file length 300 lines | `TaskAssertions.cs` expected ~60 lines | ✅ PASS | |
| Immutability / Thread safety | `readonly struct` with no mutable fields | ✅ PASS | |
| Error message format | Uses `MessageFormatter.Fail(expected, actual, expression, because, becauseArgs)` | ✅ PASS | |
| Coverage ≥ 93% line / ≥ 90% branch | Full test coverage planned for all code paths | ✅ PASS | 5 distinct code paths covered |
| Spec-driven workflow | Spec + clarifications complete before implementation | ✅ PASS | |
| AOT compatibility | No reflection; struct-based | ✅ PASS | `IsAotCompatible` is set on the library project |

**Pre-design gate: ALL PASS. Proceeding to Phase 0.**

**Post-design gate: Re-evaluated after Phase 1 — no violations introduced.**

## Project Structure

### Documentation (this feature)

```text
specs/00026-task-should-assertions/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/
│   └── task-assertions.md  # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Assertivo/
├── Exceptions/
│   ├── ActionAssertions.cs          (existing — unmodified)
│   ├── AsyncFunctionAssertions.cs   (existing — unmodified)
│   ├── ExceptionAssertions.cs       (existing — unmodified)
│   └── TaskAssertions.cs            (NEW — ~60 lines)
└── Should.cs                        (existing — add 1 new overload, ~5 lines)

tests/Assertivo.Tests/
├── TaskAssertionsTests.cs           (NEW — ~90 lines)
└── ShouldDispatchTests.cs           (existing — add 2 dispatch tests)
```

**Structure Decision**: Single-project library layout. The new `TaskAssertions` type belongs in `src/Assertivo/Exceptions/` alongside the existing async assertion types. One new test file mirrors the production type per constitution §IV.2. `ShouldDispatchTests.cs` is extended (not replaced) with two new tests covering `Task` and `Task<T>` dispatch.
