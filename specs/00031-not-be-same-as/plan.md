# Implementation Plan: NotBeSameAs — Object Reference Inequality Assertion

**Branch**: `00031-not-be-same-as` | **Date**: 2026-06-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/00031-not-be-same-as/spec.md`

## Summary

Add `NotBeSameAs(object? unexpected, string because = "", params object[] becauseArgs)` to `ObjectAssertions<T>`, completing the `BeSameAs` / `NotBeSameAs` negation pair. The method uses `ReferenceEquals` for comparison, guards against value-type subjects with an `InvalidOperationException`, and returns `AndConstraint<ObjectAssertions<T>>` for fluent chaining. The implementation is a direct mirror of `BeSameAs` with the boolean logic inverted.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (`net10.0`)
**Primary Dependencies**: None — zero third-party dependencies (BCL only: `ReferenceEquals`, `typeof(T).IsValueType`)
**Storage**: N/A
**Testing**: xUnit 2.9.3 + Coverlet (line ≥ 93 %, branch ≥ 90 %)
**Target Platform**: .NET 10 (`net10.0`); AOT-compatible. Multi-targeting (constitution §III.1) is a library-level deferred decision; this feature does not change the single-TFM posture.
**Project Type**: Library
**Performance Goals**: Zero-allocation on the passing path; ≥ 10 M ops/sec on a single core (constitution §6)
**Constraints**: `TreatWarningsAsErrors`; `Nullable` enabled; `IsAotCompatible`; max cyclomatic complexity 10; max 300 lines per file; XML docs on all public members
**Scale/Scope**: Single method addition (~15 LOC in `ObjectAssertions.cs`); 6 new test cases in `ObjectAssertionsTests.cs`; one line added to `specs/00001-assertion-library-core/contracts/public-api.md`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|---------|
| Readability First (§II.1) | ✅ Pass | `a.Should().NotBeSameAs(b)` reads as natural English |
| Zero Surprise (§II.2) | ✅ Pass | Negation of `BeSameAs`; failure message shows "not the same reference" vs "same reference" |
| Pit of Success (§II.3) | ✅ Pass | Value-type guard throws `InvalidOperationException` before any comparison |
| Zero Dependencies (§II.4) | ✅ Pass | Uses only `ReferenceEquals` and `typeof(T).IsValueType` — BCL only |
| Architecture (§III.1) | ✅ Pass | Method lives in `Assertivo` namespace alongside `BeSameAs`; no new files needed |
| Code Style (§III.2) | ✅ Pass | XML doc required; nullable annotations correct (`object?`); must pass static analysis |
| Immutability (§III.3) | ✅ Pass | `ObjectAssertions<T>` is a `readonly struct`; method returns a new `AndConstraint` |
| Error Messages (§III.4) | ✅ Pass | Delegates to `MessageFormatter.Fail`; includes `because`/`becauseArgs` |
| Coverage (§IV.1) | ✅ Pass | 6 test cases covering positive, negative, null semantics, value-type guard, and `because` |
| Test Naming (§IV.2) | ✅ Pass | `MethodName_Scenario_ExpectedOutcome` convention applied to all new tests |
| Spec-Driven (§IV.3) | ✅ Pass | Spec approved before implementation |
| AOT Compatibility (§VI.4) | ✅ Pass | No reflection at call time; `typeof(T).IsValueType` is compile-time-constant for generic JIT |
| API Design — Negation (§VII.2) | ✅ Pass | This method IS the negation of `BeSameAs` |
| API Design — Chaining (§VII.7) | ✅ Pass | Returns `AndConstraint<ObjectAssertions<T>>` |
| Public API Docs (§VIII) | ✅ Pass | `contracts/public-api.md` updated in Phase 1 |

**Gate result**: All checks pass. No violations to track.

## Project Structure

### Documentation (this feature)

```text
specs/00031-not-be-same-as/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── public-api.md    # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    └── ObjectAssertions.cs   ← add NotBeSameAs method

tests/
└── Assertivo.Tests/
    └── ObjectAssertionsTests.cs   ← add 6 new test methods

specs/
└── 00001-assertion-library-core/
    └── contracts/
        └── public-api.md   ← add NotBeSameAs signature to ObjectAssertions<T>
```

**Structure Decision**: Single-project library. The change is self-contained to one source file, one test file, and one contract document. No new files required in `src/`.
