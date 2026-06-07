# Implementation Plan: ThrowAsync Support for IAsyncEnumerable<T> Subjects

**Branch**: `00027-async-enumerable-throw` | **Date**: 2026-05-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/00027-async-enumerable-throw/spec.md`

## Summary

Add an `IAsyncEnumerable<T>.Should()` extension that returns a new `AsyncEnumerableAssertions<T>` readonly struct. The struct exposes `ThrowAsync<TException>()` which manually drives enumeration via `GetAsyncEnumerator()`/`MoveNextAsync()`/`DisposeAsync()` — rather than `await foreach` — to correctly implement the `DisposeAsync`-exception-preservation behaviour resolved in clarification §2026-05-27 Q1. It applies the same single-inner `AggregateException` unwrapping as `AsyncFunctionAssertions` and `TaskAssertions`, and returns `ExceptionAssertions<TException>` for further chaining via `.Which`. Failure messages follow the same `MessageFormatter` templates as `TaskAssertions.ThrowAsync`, substituting "source" for "task". A null subject is forwarded into the struct and produces an `AssertionFailedException` from inside `ThrowAsync`.

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: BCL only — `System.Collections.Generic.IAsyncEnumerable<T>`, `System.AggregateException`, `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute`, `System.Diagnostics.StackTraceHiddenAttribute`  
**Storage**: N/A  
**Testing**: xUnit 2.9.3 + Coverlet (line ≥ 95%, branch ≥ 90% — constitution §IV.4.1 core-engine threshold)  
**Target Platform**: .NET 10 class library (`net10.0`)  
**Project Type**: Open-source assertion library  
**Performance Goals**: Each `ThrowAsync` call must complete within 100 ms per constitution §IV.2  
**Constraints**: Zero third-party dependencies; `TreatWarningsAsErrors`; nullable enabled; max 300 lines/file; cyclomatic complexity ≤ 10  
**Scale/Scope**: 1 new production file (~80 lines), 1 new test file (~130 lines), 1 overload added to `Should.cs`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| Zero Dependencies | No new `<PackageReference>` in production assembly | ✅ PASS | `IAsyncEnumerable<T>` is BCL (`System.Collections.Generic`, available in `net10.0`) |
| Sub-namespace structure | New type in `Assertivo.Exceptions` | ✅ PASS | Placed alongside `TaskAssertions` and `AsyncFunctionAssertions` |
| One public type per file | `AsyncEnumerableAssertions.cs` contains only `AsyncEnumerableAssertions<T>` | ✅ PASS | |
| XML docs on all public members | All public types/methods/properties documented | ✅ PASS | |
| Zero warnings / Nullable enabled | `IAsyncEnumerable<T>?` stored; no suppression needed | ✅ PASS | |
| Max cyclomatic complexity 10 | `ThrowAsync` has 6 decision points (null, MoveNextAsync throws, DisposeAsync filter, AggregateException unwrap, type match, no exception) | ✅ PASS | Well under limit |
| Max file length 300 lines | Expected ~80 lines | ✅ PASS | |
| Immutability / Thread safety | `readonly struct`; no mutable fields | ✅ PASS | |
| Error message format | `MessageFormatter.Fail(expected, actual, expression, because, becauseArgs)` | ✅ PASS | Same templates as `TaskAssertions`, "source" substituted for "task" |
| Coverage ≥ 95% line / ≥ 90% branch | All code paths covered in test plan | ✅ PASS | 7 distinct paths tested; enforced via Coverlet threshold args in T014 |
| Spec-driven workflow | Spec + 3 clarifications complete before implementation | ✅ PASS | |
| AOT compatibility | Manual enumerator pattern; no reflection, no code generation | ✅ PASS | |

**Pre-design gate: ALL PASS. Proceeding to Phase 0.**

**Post-design gate: Re-evaluated after Phase 1 — no violations introduced.**

## Project Structure

### Documentation (this feature)

```text
specs/00027-async-enumerable-throw/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── async-enumerable-assertions.md  # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
src/Assertivo/
├── Exceptions/
│   ├── ActionAssertions.cs              (existing — unmodified)
│   ├── AsyncEnumerableAssertions.cs     (NEW — ~80 lines)
│   ├── AsyncFunctionAssertions.cs       (existing — unmodified)
│   ├── ExceptionAssertions.cs           (existing — unmodified)
│   └── TaskAssertions.cs                (existing — unmodified)
└── Should.cs                            (existing — 1 new overload added)

tests/Assertivo.Tests/
├── AsyncEnumerableAssertionsTests.cs    (NEW — ~130 lines)
└── ShouldDispatchTests.cs               (existing — 1 new dispatch test added)
```

**Structure Decision**: Single-project library layout. `AsyncEnumerableAssertions<T>` belongs in `src/Assertivo/Exceptions/` alongside the existing async assertion types. One new test file mirrors the production type per constitution §IV.2. `ShouldDispatchTests.cs` is extended (not replaced) with one test covering `IAsyncEnumerable<T>` dispatch.
