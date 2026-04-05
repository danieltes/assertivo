# Quickstart: NuGet Packaging Support

**Feature**: 00002-nuget-packaging

## How Packaging Works

This repository uses a centralized `Directory.Build.props` file at the repository root to define shared NuGet packaging settings. All projects inherit these defaults automatically.

**Key rule**: Projects are **non-packable by default**. Only projects that explicitly opt in produce NuGet packages.

## Making a Project Packable

To make a library project produce a NuGet package, add `<IsPackable>true</IsPackable>` to its `.csproj`:

```xml
<PropertyGroup>
  <IsPackable>true</IsPackable>
</PropertyGroup>
```

That's it. The project inherits version, author, description, license, and all other metadata from the shared configuration.

## Overriding Metadata

To customize a specific metadata field for one project, set the property in the project's `.csproj`:

```xml
<PropertyGroup>
  <IsPackable>true</IsPackable>
  <Description>My custom package description</Description>
</PropertyGroup>
```

Only the overridden field changes. All other metadata is still inherited from the shared defaults.

## Building Packages

```bash
# Pack a single project
dotnet pack src/Assertivo/Assertivo.csproj

# Pack all packable projects in the solution
dotnet pack
```

Only projects with `<IsPackable>true</IsPackable>` will produce `.nupkg` files.

## Where Things Are Defined

| What | Where |
|------|-------|
| Shared packaging defaults | `Directory.Build.props` (repository root) |
| Opt-in to packaging | `<IsPackable>true</IsPackable>` in project `.csproj` |
| Per-project metadata overrides | Project `.csproj` `<PropertyGroup>` |
| Package output | `bin/Debug/*.nupkg` or `bin/Release/*.nupkg` |

## Which Projects Produce Packages

| Project | Packable | Reason |
|---------|----------|--------|
| `src/Assertivo` | Yes | Explicitly opted in |
| `tests/Assertivo.Tests` | No | Test project |
| `tests/Assertivo.Benchmarks` | No | Benchmark executable |
| `tests/Validation/ConsoleSmoke` | No | Validation executable |
| `tests/Validation/QuickstartSmoke` | No | Validation test project |
