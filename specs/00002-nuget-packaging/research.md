# Research: Add NuGet Packaging Support via `Directory.Build.props`

**Feature**: 00002-nuget-packaging
**Date**: 2026-04-04

## Research Task 1: Directory.Build.props MSBuild Property Inheritance

**Context**: FR-001 requires centralized packaging configuration. Need to confirm how `Directory.Build.props` property inheritance works with .NET SDK-style projects.

**Decision**: Use a single `Directory.Build.props` at the repository root to define shared packaging metadata. All projects in the directory tree automatically inherit these properties via MSBuild's implicit import mechanism.

**Rationale**:
- MSBuild automatically imports `Directory.Build.props` from the directory containing the project file, walking up to the repository root. A single file at the root covers all projects.
- Properties defined in `Directory.Build.props` are imported *before* the project file, so project-level properties naturally override shared defaults (FR-004).
- This is the standard, documented .NET SDK approach — no custom tooling needed (FR-009).

**Alternatives considered**:
- **`Directory.Packages.props`**: Only for centralized package version management (Central Package Management), not for packaging metadata.
- **Custom `.targets` file with explicit import**: More flexible but non-standard and requires manual import in each project.
- **NuGet `.nuspec` files per project**: Legacy approach, not compatible with SDK-style projects' integrated packaging.

---

## Research Task 2: Opt-In vs. Opt-Out Packaging Model

**Context**: FR-002 specifies projects are non-packable by default (opt-in model). Need to confirm how to enforce this with MSBuild.

**Decision**: Set `<IsPackable>false</IsPackable>` as the global default in `Directory.Build.props`. Packable projects override with `<IsPackable>true</IsPackable>` in their own `.csproj`.

**Rationale**:
- .NET SDK default for `IsPackable` is `true` for library projects and `false` for Exe projects. Setting it explicitly to `false` in `Directory.Build.props` ensures no project accidentally produces a package.
- Projects that should be packable explicitly set `<IsPackable>true</IsPackable>` — this is the single opt-in property referenced in SC-002.
- Project-level properties override `Directory.Build.props` properties because `.csproj` is evaluated after the implicit import.

**Alternatives considered**:
- **Opt-out model (IsPackable=true by default)**: Risky — new test projects or executables could accidentally produce packages. Rejected per spec Assumptions.
- **Conditional IsPackable based on project path**: Fragile — depends on directory naming conventions, breaks if projects are moved.
- **Custom MSBuild property (e.g., `<PublishAsNuGet>`)**: Non-standard, would not integrate with `dotnet pack` without custom targets.

---

## Research Task 3: Shared NuGet Package Metadata Properties

**Context**: FR-003 requires specific shared metadata fields. Need to map spec requirements to MSBuild property names.

**Decision**: Use the following standard MSBuild properties in `Directory.Build.props`:

| Spec Requirement | MSBuild Property | Value Strategy |
|-----------------|------------------|----------------|
| Package version | `<Version>` | `0.1.0` (per clarification) |
| Authors | `<Authors>` | Repository author name |
| Description | `<Description>` | Generic shared description |
| Repository URL | `<RepositoryUrl>` | GitHub repository URL |
| Package tags | `<PackageTags>` | Shared tags (e.g., "testing assertions fluent dotnet") |
| License expression | `<PackageLicenseExpression>` | `MIT` (per LICENSE file) |

**Rationale**:
- All properties are standard NuGet/MSBuild properties recognized by `dotnet pack`. No custom properties needed.
- `<Version>` is the canonical property for package version in SDK-style projects. It sets both `PackageVersion` and `AssemblyVersion` (with appropriate defaults).
- Using `<PackageLicenseExpression>` over `<PackageLicenseFile>` for simplicity — the MIT license is a well-known SPDX expression.

**Alternatives considered**:
- **`<PackageVersion>` instead of `<Version>`**: `PackageVersion` only sets the NuGet version, not assembly version. `Version` is more comprehensive and is the standard practice.
- **Including `<PackageLicenseFile>`**: Requires including the LICENSE file in the package. Deferred to a future feature (symbol packages / enriched packaging).
- **Including `<PackageReadmeFile>`**: Requires readme embedding. Deferred per spec Assumptions.

---

## Research Task 4: Per-Project Override Mechanism

**Context**: FR-004 requires projects to override inherited metadata. Need to confirm the override mechanism.

**Decision**: Standard MSBuild property evaluation order handles this automatically. Properties in `.csproj` files override same-named properties from `Directory.Build.props`.

**Rationale**:
- MSBuild evaluates `Directory.Build.props` → project `.csproj` → `Directory.Build.targets`. Properties set later win (last-write-wins).
- A project that needs a custom `<Description>` simply defines `<Description>` in its own `<PropertyGroup>`. No special mechanism needed.
- This is standard MSBuild behavior — no documentation gap for experienced .NET developers.

**Alternatives considered**:
- **Conditional properties with `Condition="'$(Description)' == ''"`**: Would allow per-project to set a property and have `Directory.Build.props` not override it. But since `.csproj` is evaluated *after* `Directory.Build.props`, this is unnecessary — the project's value naturally wins.
- **Separate `Directory.Build.targets` for defaults**: Useful for defaults that should only apply when the project hasn't set them, but adds complexity. Standard property evaluation is sufficient here.

---

## Research Task 5: Non-Packable Project Enforcement for Existing Projects

**Context**: FR-006 requires test projects, executables, samples, and internal tooling to never produce packages. Need to audit existing projects.

**Decision**: The global `<IsPackable>false</IsPackable>` in `Directory.Build.props` covers all projects by default. Existing explicit `<IsPackable>false</IsPackable>` in test projects can be removed as redundant (but keeping them is harmless and provides explicit documentation).

**Current project audit**:

| Project | Type | Current IsPackable | Action Needed |
|---------|------|-------------------|---------------|
| `src/Assertivo` | Library | `true` (explicit) | Keep `<IsPackable>true</IsPackable>`, remove `<PackageId>` (defaults to project name) |
| `tests/Assertivo.Tests` | Test project | `false` (explicit) | Redundant with shared default; keep for clarity |
| `tests/Assertivo.Benchmarks` | Exe | Not set | Covered by shared `<IsPackable>false</IsPackable>` default |
| `tests/Validation/ConsoleSmoke` | Exe | Not set | Covered by shared default |
| `tests/Validation/QuickstartSmoke` | Test project | `false` (explicit) | Redundant with shared default; keep for clarity |

**Rationale**:
- Setting the global default to `false` is the safest approach — any new project is non-packable unless explicitly opted in.
- Keeping existing explicit `<IsPackable>false</IsPackable>` in test projects provides defense-in-depth and self-documentation, even though the shared default makes it redundant.

**Alternatives considered**:
- **Remove all redundant `<IsPackable>false</IsPackable>` from non-packable projects**: Cleaner but less obvious when reading individual project files. Recommended to keep for clarity.
- **Use MSBuild conditions based on project SDK or OutputType**: Possible but fragile and non-standard. Explicit opt-in is simpler.
