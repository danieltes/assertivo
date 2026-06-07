# Implementation Plan: NotBe Inequality Assertion

**Branch**: `00028-not-be-assertion` | **Date**: 2026-05-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00028-not-be-assertion/spec.md`

## Summary

Add a `NotBe` method — the symmetric counterpart of `Be` — to `ObjectAssertions<T>`, `NumericAssertions<T>`, and `StringAssertions`. The method passes when the subject does not equal the unexpected value and throws `AssertionFailedException` (via `MessageFormatter.Fail`) when it does. `ObjectAssertions<T>` and `NumericAssertions<T>` accept an optional `IEqualityComparer<T>`; `StringAssertions` uses ordinal (case-sensitive) comparison, consistent with its existing `Be`. The implementation mirrors `Be` in structure — one additional method per type, each decorated with `[StackTraceHidden]` and XML `<summary>`.

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: BCL only — `System.Collections.Generic.EqualityComparer<T>`, `System.String.Equals` with `StringComparison.Ordinal`  
**Storage**: N/A  
**Testing**: xUnit (`Assertivo.Tests` project) — `Assert.Throws<AssertionFailedException>`, `Assert.Contains`  
**Target Platform**: .NET library (`net10.0`)  
**Project Type**: Open-source assertion library  
**Performance Goals**: Zero-allocation on passing path — `AndConstraint<T>` is a `readonly struct`, stack-only; ≥ 10M ops/sec  
**Constraints**: AOT-compatible (`EqualityComparer<T>.Default` is AOT-safe); `TreatWarningsAsErrors`; nullable reference types enabled; max 300 lines/file  
**Scale/Scope**: 3 new methods, modifications to 3 existing source files, additions to 3 existing test files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| Readability First (II.1) | `NotBe` reads as natural English `.Should().NotBe(unexpected)` | ✅ PASS | Natural symmetric counterpart of `Be` |
| Zero Surprise (II.2) | Failure message explicitly shows `not <unexpected>` and `<actual>` | ✅ PASS | Follows existing `MessageFormatter.Fail` pattern |
| Pit of Success (II.3) | Same signature shape as `Be`; impossible to confuse direction | ✅ PASS | No new API surface that can be misused |
| Zero Dependencies (II.4) | Uses only `EqualityComparer<T>.Default` and `string.Equals` from BCL | ✅ PASS | Zero new package references |
| XML Documentation (III.2) | All three methods require `<summary>` XML doc comments | ✅ REQUIRED | Achievable; mirrors existing `Be` docs |
| Zero Warnings (III.2) | `string?` nullable parameter on `StringAssertions.NotBe`; `T` is non-nullable on `NumericAssertions<T>` | ✅ REQUIRED | Proper nullability annotations needed |
| [StackTraceHidden] (V.3) | All three methods must carry `[System.Diagnostics.StackTraceHidden]` | ✅ REQUIRED | Namespace already imported in all three files |
| Zero-Allocation Happy Path (VI.2) | `AndConstraint<T>` is a `readonly struct` — no heap allocation on passing path | ✅ PASS | Identical allocation profile to `Be` |
| AOT-Compatible (VI.4) | No reflection, no `MakeGenericType`; `EqualityComparer<T>.Default` is AOT-safe | ✅ PASS | AOT-safe by design |
| Test Coverage ≥ 95% line / 90% branch (IV.1) | Positive, negative, edge-case, comparer, null, because, chaining tests per method | ✅ REQUIRED | Tests will be added to three existing test files |
| API Negation Exists (VII.2) | `NotBe` IS the negation of `Be` for all three types | ✅ PASS | This feature delivers the negation |
| Custom Comparer at Call Site (VII.8) | `IEqualityComparer<T>? comparer = null` parameter on `ObjectAssertions<T>` and `NumericAssertions<T>` | ✅ PASS | No global state |

**Gate result: PASS — no violations. Proceeding to Phase 0.**

## Project Structure

### Documentation (this feature)

```text
specs/00028-not-be-assertion/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── api.md           # Phase 1 output — public method signatures
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
src/
└── Assertivo/
    ├── ObjectAssertions.cs        ← add NotBe method
    ├── StringAssertions.cs        ← add NotBe method
    └── Numeric/
        └── NumericAssertions.cs   ← add NotBe method

tests/
└── Assertivo.Tests/
    ├── ObjectAssertionsTests.cs   ← add NotBe test cases
    ├── NumericAssertionsTests.cs  ← add NotBe test cases
    └── StringAssertionsTests.cs   ← add NotBe test cases
```

**Structure Decision**: Single-project library — no new files, no new folders. All changes are additive within existing files.

## Complexity Tracking

No constitution violations. Table omitted.
