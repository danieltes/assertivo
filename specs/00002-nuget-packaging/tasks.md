# Tasks: Add NuGet Packaging Support via `Directory.Build.props`

**Input**: Design documents from `/specs/00002-nuget-packaging/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: No test tasks generated — feature specification does not request automated tests. Validation is manual via `dotnet pack` + package inspection.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the shared packaging configuration file at the repository root

- [x] T001 Create `Directory.Build.props` at the repository root with shared packaging properties: `IsPackable=false` (global default), `Version=0.1.0`, `Authors`, `Description`, `PackageLicenseExpression=MIT`, `RepositoryUrl`, and `PackageTags` in `Directory.Build.props`. Include inline XML comments documenting the opt-in convention (`IsPackable=true` in project file) and where shared metadata is defined (FR-008)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Clean up existing project files to remove duplicated packaging metadata now provided by the shared configuration. MUST be complete before user story validation.

**⚠️ CRITICAL**: No user story validation can proceed until project files are consistent with the shared configuration.

- [x] T002 Remove redundant `<PackageId>Assertivo</PackageId>` from `src/Assertivo/Assertivo.csproj` (package ID defaults to project/assembly name per FR-010; `<IsPackable>true</IsPackable>` must remain as the explicit opt-in)
- [x] T003 [P] Verify `tests/Assertivo.Tests/Assertivo.Tests.csproj` retains `<IsPackable>false</IsPackable>` for defense-in-depth alongside the shared default
- [x] T004 [P] Verify `tests/Validation/QuickstartSmoke/QuickstartSmoke.csproj` retains `<IsPackable>false</IsPackable>` for defense-in-depth alongside the shared default
- [x] T005 [P] Verify `tests/Assertivo.Benchmarks/Assertivo.Benchmarks.csproj` has `OutputType=Exe` and is covered by the shared `IsPackable=false` default (no changes needed)
- [x] T006 [P] Verify `tests/Validation/ConsoleSmoke/ConsoleSmoke.csproj` has `OutputType=Exe` and is covered by the shared `IsPackable=false` default (no changes needed)

**Checkpoint**: All project files are consistent with the shared packaging configuration. Shared metadata is defined in exactly one location (SC-001).

---

## Phase 3: User Story 1 — Centralized Packaging Defaults (Priority: P1) 🎯 MVP

**Goal**: Verify that a packable library project produces a NuGet package with all shared metadata inherited from `Directory.Build.props`, without any packaging metadata in the project file.

**Independent Test**: Run `dotnet pack src/Assertivo/Assertivo.csproj -c Release` and inspect the generated `.nupkg` metadata.

### Implementation for User Story 1

- [x] T007 [US1] Run `dotnet pack src/Assertivo/Assertivo.csproj -c Release` and verify a `.nupkg` file is produced in `src/Assertivo/bin/Release/`
- [x] T008 [US1] Inspect the generated package metadata (extract `.nuspec` from `.nupkg` or use `dotnet nuget` tooling) and verify it contains: Version=`0.1.0`, Authors, Description, PackageLicenseExpression=`MIT`, RepositoryUrl, and PackageTags as defined in `Directory.Build.props`
- [x] T009 [US1] Confirm `src/Assertivo/Assertivo.csproj` contains no packaging metadata properties other than `<IsPackable>true</IsPackable>` — all metadata comes from the shared configuration (SC-001)

**Checkpoint**: User Story 1 is complete — packable project inherits all shared metadata and produces a valid package.

---

## Phase 4: User Story 2 — Non-Packable Project Exclusion (Priority: P1)

**Goal**: Verify that test projects, executables, samples, and benchmarks do NOT produce NuGet packages when packaging is run at the solution level.

**Independent Test**: Run `dotnet pack` at the solution level and verify only `Assertivo.nupkg` is produced.

### Implementation for User Story 2

- [x] T010 [US2] Run `dotnet pack` at the solution root and verify only `src/Assertivo/` produces a `.nupkg` file
- [x] T011 [US2] Verify no `.nupkg` file is produced for `tests/Assertivo.Tests/`
- [x] T012 [P] [US2] Verify no `.nupkg` file is produced for `tests/Assertivo.Benchmarks/`
- [x] T013 [P] [US2] Verify no `.nupkg` file is produced for `tests/Validation/ConsoleSmoke/`
- [x] T014 [P] [US2] Verify no `.nupkg` file is produced for `tests/Validation/QuickstartSmoke/`

**Checkpoint**: User Story 2 is complete — only designated packable projects produce packages; all others are excluded.

---

## Phase 5: User Story 3 — Minimal Setup for New Packable Projects (Priority: P2)

**Goal**: Verify that a newly added library project can produce a well-formed NuGet package by adding only `<IsPackable>true</IsPackable>`.

**Independent Test**: Temporarily add `<IsPackable>true</IsPackable>` to a test library project, run `dotnet pack`, verify the package inherits shared metadata, then revert.

### Implementation for User Story 3

- [x] T015 [US3] Verify and update `specs/00002-nuget-packaging/quickstart.md` to ensure the opt-in procedure is accurate: a new library project only needs `<IsPackable>true</IsPackable>` to inherit all shared packaging metadata (FR-007, SC-002)
- [x] T015b [US3] Hands-on verification: create a temporary class library project, add only `<IsPackable>true</IsPackable>`, run `dotnet pack`, verify the package is produced with all inherited shared metadata, then delete the temporary project (FR-007, SC-002)

**Checkpoint**: User Story 3 is complete — the onboarding procedure is documented and verified end-to-end with a real project.

---

## Phase 6: User Story 4 — Per-Project Metadata Overrides (Priority: P2)

**Goal**: Verify that a packable project can override specific metadata fields while inheriting the rest from the shared configuration.

**Independent Test**: Temporarily add a `<Description>` override to `src/Assertivo/Assertivo.csproj`, run `dotnet pack`, verify the override appears in the package and all other fields remain inherited, then revert.

### Implementation for User Story 4

- [x] T016 [US4] Verify MSBuild property evaluation order: (a) add a temporary `<Description>` override to `src/Assertivo/Assertivo.csproj`, run `dotnet pack`, confirm the description override takes precedence over the shared default; (b) add a temporary `<Version>` override, run `dotnet pack`, confirm the version override takes precedence; then revert all changes

**Checkpoint**: User Story 4 is complete — per-project overrides work via standard MSBuild property evaluation.

---

## Phase 7: User Story 5 — Predictable Packaging for Automation (Priority: P3)

**Goal**: Verify that `dotnet pack` at the solution level is deterministic and requires no special flags.

**Independent Test**: Run `dotnet pack` twice against the same source state and compare outputs.

### Implementation for User Story 5

- [x] T017 [US5] Run `dotnet pack` at the solution root twice in sequence, compare the set of produced `.nupkg` files, and verify identical package set and metadata both times (SC-006)

**Checkpoint**: User Story 5 is complete — packaging is deterministic and automation-ready.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup and discoverability improvements

- [x] T018 [P] Verify the existing `README.md` installation section is consistent with the package name and version produced by the new packaging setup
- [x] T019 Run the full quickstart.md validation: follow the quickstart guide end-to-end to confirm packaging instructions are accurate and complete (FR-008)
- [x] T020 [P] Add a brief "Packaging" section to `README.md` explaining the packaging convention: where shared config lives, how to opt in, how to override metadata, and a link to the quickstart guide (FR-008 discoverability)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user story validation
- **User Story 1 (Phase 3)**: Depends on Phase 2 — first validation of the core packaging flow
- **User Story 2 (Phase 4)**: Depends on Phase 2 — can run in parallel with Phase 3
- **User Story 3 (Phase 5)**: Depends on Phase 2 — can run in parallel with Phases 3–4
- **User Story 4 (Phase 6)**: Depends on Phase 2 — can run in parallel with Phases 3–5
- **User Story 5 (Phase 7)**: Depends on Phase 2 — can run in parallel with Phases 3–6
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories — core MVP
- **User Story 2 (P1)**: No dependencies on other stories — independent exclusion validation
- **User Story 3 (P2)**: No dependencies — documentation task
- **User Story 4 (P2)**: No dependencies — independent override validation
- **User Story 5 (P3)**: No dependencies — independent determinism validation

### Parallel Opportunities

- T003, T004, T005, T006 can all run in parallel (Phase 2 verification tasks)
- T012, T013, T014 can run in parallel (Phase 4 non-packable project checks)
- All user story phases (3–7) can run in parallel after Phase 2 completes
- T018 can run in parallel with T019 (Phase 8)

---

## Parallel Example: User Story 2

```
# These exclusion checks can run simultaneously:
T012: Verify no .nupkg for Assertivo.Benchmarks
T013: Verify no .nupkg for ConsoleSmoke
T014: Verify no .nupkg for QuickstartSmoke
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup — create `Directory.Build.props`
2. Complete Phase 2: Foundational — clean up existing project files
3. Complete Phase 3: User Story 1 — validate packable project produces correct package
4. **STOP and VALIDATE**: Run `dotnet pack` on `src/Assertivo/Assertivo.csproj` and inspect metadata
5. If valid, proceed to remaining user stories

### Incremental Delivery

6. Phase 4: User Story 2 — validate exclusion of non-packable projects
7. Phase 5: User Story 3 — document the opt-in procedure
8. Phase 6: User Story 4 — validate per-project overrides
9. Phase 7: User Story 5 — validate deterministic packaging
10. Phase 8: Polish — README consistency and quickstart validation
