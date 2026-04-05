# Feature Specification: Add NuGet Packaging Support via `Directory.Build.props`

**Feature Branch**: `00002-nuget-packaging`
**Created**: 2026-04-04
**Status**: Draft
**Input**: Add repository-level support for creating NuGet packages for packable .NET projects by centralizing packaging configuration in `Directory.Build.props`.

## Clarifications

### Session 2026-04-04

- Q: What should the initial shared package version be? → A: `0.1.0` (pre-release/development version per SemVer convention).
- Q: Should the shared Description metadata be a generic repository-level description, or must each project define its own? → A: Shared generic description at repository level; projects override only if needed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Centralized Packaging Defaults (Priority: P1)

A repository maintainer wants to define common NuGet packaging settings once at the repository level so that all eligible projects inherit the same defaults without duplicating metadata in each project file.

**Why this priority**: This is the foundational capability. Without centralized configuration, every other user story is impossible. All packaging consistency, automation, and maintenance benefits depend on this.

**Independent Test**: Can be fully tested by running the standard pack command against an eligible project and verifying that a package is produced containing the expected shared metadata, without any packaging settings in the project file itself.

**Acceptance Scenarios**:

1. **Given** a repository with shared packaging configuration and a library project that does not define its own packaging metadata, **When** the standard pack command is run against that project, **Then** a NuGet package is produced with the shared metadata values.
2. **Given** a repository with shared packaging configuration, **When** a maintainer inspects the generated package metadata, **Then** it includes the author, description, version, license expression, repository URL, and tags defined at the repository level.
3. **Given** a repository with shared packaging configuration, **When** a maintainer reviews the individual project file for a packable library, **Then** no packaging-related properties are duplicated from the shared configuration.

---

### User Story 2 — Non-Packable Project Exclusion (Priority: P1)

A repository maintainer wants to ensure that test projects, executables, samples, and internal tooling do not accidentally produce NuGet packages when packaging is run at the solution or repository level.

**Why this priority**: Without safe exclusion, packaging operations may produce unintended artifacts. This is a correctness and safety concern that must be in place alongside centralized packaging.

**Independent Test**: Can be tested by running the standard pack command against the entire solution and verifying that only designated packable projects produce packages, while test projects, executables, and samples produce none.

**Acceptance Scenarios**:

1. **Given** a test project in the repository, **When** the standard pack command is run at the solution level, **Then** no NuGet package is produced for the test project.
2. **Given** an executable project (console application) in the repository, **When** the standard pack command is run, **Then** no NuGet package is produced for the executable.
3. **Given** a class library that is explicitly marked as non-packable (internal-only), **When** the standard pack command is run, **Then** no NuGet package is produced for that library.
4. **Given** a newly added project that has not been explicitly designated as packable, **When** the standard pack command is run, **Then** no NuGet package is produced for that project.

---

### User Story 3 — Minimal Setup for New Packable Projects (Priority: P2)

A library author adds a new class library project to the solution and wants it to produce a well-formed NuGet package with minimal extra configuration.

**Why this priority**: New projects should be easy to onboard for packaging. This story validates the maintainability and extensibility promise of the centralized setup.

**Independent Test**: Can be tested by creating a new class library project, marking it as packable, running the pack command, and verifying the package is produced with inherited metadata.

**Acceptance Scenarios**:

1. **Given** a new class library project added to the repository, **When** the maintainer marks it as packable following the repository convention, **Then** the standard pack command produces a NuGet package for that project.
2. **Given** a newly packable project with no project-specific metadata, **When** a package is generated, **Then** the package inherits all shared metadata from the repository-level configuration.
3. **Given** a newly packable project, **When** a maintainer looks at the project file, **Then** the only packaging-related configuration needed is the opt-in designation.

---

### User Story 4 — Per-Project Metadata Overrides (Priority: P2)

A library author has a project that needs a different description, package tags, or package ID than the repository defaults, and wants to override only the differing values while inheriting everything else.

**Why this priority**: Not all projects share identical metadata. Override capability ensures centralized defaults do not force uniformity where it is not appropriate.

**Independent Test**: Can be tested by adding project-specific metadata overrides to a packable project, running the pack command, and verifying the overridden values appear in the package while non-overridden values still match the shared defaults.

**Acceptance Scenarios**:

1. **Given** a packable project that defines a project-specific description, **When** a package is generated, **Then** the package description matches the project-level value, not the shared default.
2. **Given** a packable project that defines a custom package ID, **When** a package is generated, **Then** the package uses the custom ID.
3. **Given** a packable project that overrides only one metadata field, **When** a package is generated, **Then** all other metadata fields are inherited from the shared configuration.

---

### User Story 5 — Predictable Packaging for Automation (Priority: P3)

A build engineer wants the standard pack command to behave predictably and consistently across the repository so that packaging can be reliably integrated into automation workflows without special-case logic per project.

**Why this priority**: Automation readiness is important but depends on the foundational stories being in place first. This story validates the end-to-end packaging behavior from an automation perspective.

**Independent Test**: Can be tested by running the standard pack command at the solution level multiple times against the same source state and verifying the same set of packages with the same metadata is produced each time.

**Acceptance Scenarios**:

1. **Given** the repository in a known source state, **When** the standard pack command is run at the solution level, **Then** only the designated packable projects produce NuGet package artifacts.
2. **Given** the same source state, **When** the pack command is run twice, **Then** the same set of packages with the same metadata is produced both times.
3. **Given** a solution containing packable libraries, test projects, executables, and samples, **When** the pack command is run, **Then** no custom flags or project-specific commands are required to produce the correct packaging outcome.

---

### Edge Cases

- A class library project exists but is intended for internal use only and must not be packaged, even though it matches the structural profile of a packable project.
- A project needs a package ID that differs from its project name or assembly name.
- A project requires a description or tags different from the repository-level defaults.
- A test project inherits shared repository settings but must remain non-packable regardless.
- An executable project (console app, tool) exists in the repository and must not be treated as a library package.
- A solution-level pack operation is run against a mix of packable libraries, test projects, executables, and sample applications.
- A newly added project has not been explicitly designated as packable or non-packable.
- A project overrides the version property at the project level while other projects use the shared version.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The repository MUST define shared NuGet packaging configuration at the repository level so that eligible projects inherit the same packaging defaults automatically without duplicating settings in individual project files.
- **FR-002**: Projects MUST be non-packable by default. Only projects explicitly opted in as packable SHALL produce NuGet packages.
- **FR-003**: The shared configuration MUST support the following repository-level package metadata: package version, authors, description (shared generic default), repository URL, package tags, and license expression. The shared description applies to all packable projects unless overridden at the project level.
- **FR-004**: Individual projects MUST be able to override any inherited packaging metadata when project-specific values are needed, with the override taking precedence over shared defaults.
- **FR-005**: The standard pack command MUST produce NuGet package artifacts for all opted-in packable projects without requiring custom commands or additional flags.
- **FR-006**: Test projects, executable projects, sample applications, and internal tooling MUST NOT produce NuGet packages, even when packaging is run at the solution level.
- **FR-007**: A newly added library project MUST be able to become packable by adding only the opt-in designation, inheriting all other packaging settings from the shared configuration.
- **FR-008**: The packaging convention MUST be discoverable: maintainers must be able to determine which projects produce packages, how to opt in or out, where shared metadata is defined, and where project-specific overrides belong.
- **FR-009**: The packaging configuration MUST be compatible with the standard .NET CLI pack workflow — no proprietary tooling or custom build scripts required for the common case.
- **FR-010**: The package ID for each packable project MUST default to the project name (assembly name) unless explicitly overridden at the project level.

### Key Entities

- **Shared Packaging Configuration**: The repository-level file that defines common NuGet packaging defaults inherited by all eligible projects. Contains metadata such as version, authors, description, license, repository URL, and tags.
- **Packable Project**: A library project that has been explicitly opted in to produce a NuGet package. Inherits shared packaging defaults and may override specific metadata values.
- **Non-Packable Project**: Any project that does not opt in to packaging. Includes test projects, executables, samples, and internal-only libraries. These never produce NuGet package artifacts.
- **Package Metadata**: The set of descriptive properties embedded in a NuGet package, including package ID, version, authors, description, license expression, repository URL, and tags.
- **Per-Project Override**: A project-level packaging metadata value that takes precedence over the corresponding shared default, allowing a project to customize specific fields while inheriting the rest.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Common packaging metadata is defined in exactly one location — not duplicated across any project files.
- **SC-002**: A maintainer can make a new library project produce a well-formed NuGet package by adding only a single opt-in property to the project file.
- **SC-003**: Running the pack command at the solution level produces packages only for designated packable projects and zero packages for test projects, executables, or samples.
- **SC-004**: Every generated package contains the expected shared metadata (version, authors, description, license expression, repository URL, tags) when inspected.
- **SC-005**: A project-level metadata override is reflected in the generated package, while all non-overridden fields still match the shared defaults.
- **SC-006**: The same pack command produces the same packaging outcome when run repeatedly against the same source state — no variance in which projects are packaged or what metadata they contain.

## Assumptions

- The repository uses standard .NET SDK build conventions and `Directory.Build.props` is the appropriate location for shared MSBuild properties.
- Projects are non-packable by default (opt-in model). This is the safest approach to prevent accidental packaging of test projects, executables, and internal libraries. The open question about opt-in vs. opt-out is resolved in favor of opt-in.
- Package versioning uses a version property defined in the shared packaging configuration. The initial shared version is `0.1.0`, following SemVer convention for a new library that has not yet committed to a stable public API. The version can be overridden per project and will be incremented as the library matures.
- Package ID defaults to the project/assembly name, which is the standard .NET SDK behavior. A project may override this if a different public package identity is needed.
- The mandatory shared metadata fields for all packages in this repository are: version, authors, description, license expression, repository URL, and tags. Additional fields may be added at the project level.
- Symbol packages, source link integration, README file inclusion, and license file embedding are deferred to a future feature. This feature covers the core packaging configuration and metadata only.
- The repository contains multiple project types (libraries, test projects, executables, samples). Only library projects explicitly opted in will produce NuGet packages.
- Standard .NET SDK pack behavior is available in the target development environment.
- Packaging uses the Release configuration (`dotnet pack -c Release`) as the standard practice for producing distributable NuGet packages. Debug configuration packaging is not a requirement.
- Package output uses the standard SDK default path (`bin/<Configuration>/`) — no custom output directory is required.
