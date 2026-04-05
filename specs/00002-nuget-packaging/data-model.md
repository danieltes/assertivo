# Data Model: Add NuGet Packaging Support via `Directory.Build.props`

**Feature**: 00002-nuget-packaging
**Date**: 2026-04-04

## Entities

### Shared Packaging Configuration (`Directory.Build.props`)

The repository-root MSBuild properties file. Defines default packaging behavior and metadata for all projects.

| Property | Type | Default Value | Purpose |
|----------|------|---------------|---------|
| `IsPackable` | boolean | `false` | Global default: no project is packable unless explicitly opted in |
| `Version` | string | `0.1.0` | Shared package version (SemVer) |
| `Authors` | string | `Daniel Eduardo Testa` | Package author (from LICENSE) |
| `Description` | string | `A high-performance, fluent assertion library for .NET` | Shared description for all packages |
| `PackageLicenseExpression` | string | `MIT` | SPDX license identifier |
| `RepositoryUrl` | string | `https://github.com/danieltes/assertivo` | Source repository link |
| `PackageTags` | string | `testing assertions fluent dotnet` | Discovery tags for NuGet |

### Packable Project (per-project `.csproj` override)

A library project that opts in to packaging by setting `<IsPackable>true</IsPackable>`.

| Property | Type | Required | Purpose |
|----------|------|----------|---------|
| `IsPackable` | boolean | Yes (`true`) | Opts the project into package generation |
| Any shared property | string | No | Optional override for any inherited metadata |

### Non-Packable Project

Any project that does not set `<IsPackable>true</IsPackable>`. Inherits the global `false` default. No NuGet package artifact produced.

## Relationships

```
Directory.Build.props (1) ──inherits──▶ (N) Project .csproj files
                                            │
                                            ├── Packable: IsPackable=true → produces .nupkg
                                            └── Non-Packable: IsPackable=false (default) → no .nupkg
```

## Validation Rules

- A project produces a `.nupkg` if and only if `IsPackable` evaluates to `true` after MSBuild property evaluation.
- `Version` must be a valid SemVer string.
- `PackageLicenseExpression` must be a valid SPDX expression.
- `RepositoryUrl` must be a valid URL.

## State Transitions

This feature has no runtime state. The "state" is the MSBuild property evaluation result at pack time:

1. **Default state**: All projects inherit `IsPackable=false` from `Directory.Build.props`.
2. **Opt-in**: A project sets `<IsPackable>true</IsPackable>` in its `.csproj`.
3. **Pack**: `dotnet pack` evaluates final properties and produces `.nupkg` only for `IsPackable=true` projects.
