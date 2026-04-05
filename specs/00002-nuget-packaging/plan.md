# Implementation Plan: Add NuGet Packaging Support via `Directory.Build.props`

**Branch**: `00002-nuget-packaging` | **Date**: 2026-04-04 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/00002-nuget-packaging/spec.md`

## Summary

Centralize NuGet packaging configuration in a repository-level `Directory.Build.props` file using standard MSBuild properties. Projects are non-packable by default (opt-in via `<IsPackable>true</IsPackable>`). Shared metadata (version `0.1.0`, authors, description, license, repo URL, tags) is defined once and inherited by all opted-in projects. Existing project files are cleaned up to remove duplicated packaging properties. Non-packable projects (tests, benchmarks, executables, samples) are explicitly excluded via an unconditional global `IsPackable=false` default.

## Technical Context

**Language/Version**: C# / .NET 10.0 (MSBuild/SDK-style projects)
**Primary Dependencies**: None (standard .NET SDK MSBuild only)
**Storage**: N/A
**Testing**: Manual verification via `dotnet pack` + NuGet package inspection
**Target Platform**: .NET 10.0 (cross-platform)
**Project Type**: Library (NuGet package)
**Performance Goals**: N/A (build-time configuration, not runtime)
**Constraints**: Must use only standard MSBuild properties; no custom targets or third-party tooling
**Scale/Scope**: 1 packable library project (`Assertivo`), 4 non-packable projects (tests, benchmarks, console smoke, quickstart smoke)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **II.4 Zero Dependencies** | PASS | No new dependencies introduced. `Directory.Build.props` uses only standard MSBuild/SDK properties. |
| **III.1 Architecture — single root namespace** | N/A | No code changes; configuration only. |
| **III.1 Architecture — broadest practical .NET surface** | NOTE | Spec targets .NET 10.0 single TFM per feature-001 decision. Constitution notes broadest surface; existing feature-001 spec already documented this override. No change needed. |
| **III.1 Architecture — zero PackageReference in shipped package** | PASS | No new package references added. Shared config does not introduce dependencies. |
| **III.2 Code Style — TreatWarningsAsErrors** | N/A | No code changes. |
| **V.1 Discoverability** | PASS | FR-008 requires packaging convention to be discoverable. |
| **VI.2 Allocation Budgets** | N/A | No runtime code changes. |
| **VII.8 Custom comparers injectable** | N/A | No API changes. |
| **IX Versioning — Semantic Versioning** | PASS | Initial version `0.1.0` follows SemVer 2.0.0. |
| **X Governance — specs before implementation** | PASS | Spec created and clarified before plan. |

**Gate result: PASS** — No violations. Proceeding to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/00002-nuget-packaging/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
./                               # Repository root
├── Directory.Build.props        # NEW — shared packaging configuration (FR-001, FR-003)
├── src/
│   └── Assertivo/
│       └── Assertivo.csproj     # MODIFIED — remove duplicated packaging props, retain IsPackable=true
└── tests/
    ├── Assertivo.Tests/
    │   └── Assertivo.Tests.csproj          # MODIFIED — IsPackable=false already present, verify
    ├── Assertivo.Benchmarks/
    │   └── Assertivo.Benchmarks.csproj     # UNCHANGED — Exe OutputType, non-packable by default
    └── Validation/
        ├── ConsoleSmoke/
        │   └── ConsoleSmoke.csproj         # UNCHANGED — Exe OutputType, non-packable by default
        └── QuickstartSmoke/
            └── QuickstartSmoke.csproj      # MODIFIED — already has IsPackable=false, verify
```

**Structure Decision**: Single new file (`Directory.Build.props`) at the repository root. Existing `.csproj` files are modified only to remove duplicated packaging metadata and ensure opt-in/opt-out is explicit where needed. No new projects or directories created beyond spec documentation.

## Complexity Tracking

No constitution violations to justify. Feature is pure MSBuild configuration — no new projects, no new code, no new dependencies.
