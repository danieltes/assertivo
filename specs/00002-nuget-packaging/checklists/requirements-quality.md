# Requirements Quality Checklist: Add NuGet Packaging Support via `Directory.Build.props`

**Purpose**: Validate the completeness, clarity, consistency, and measurability of requirements in the feature specification before implementation begins.
**Created**: 2026-04-04
**Feature**: [spec.md](../spec.md)

## Requirement Completeness

- [x] CHK001 - Are all six metadata fields listed in FR-003 (version, authors, description, repository URL, tags, license expression) individually specified with expected values or value strategies in the spec or plan? [Completeness, Spec §FR-003]
- [x] CHK002 - Are requirements defined for what happens when `dotnet pack` is run in Release vs. Debug configuration? [Gap]
- [x] CHK003 - Are requirements defined for the package output location (default `bin/` or a custom output path)? [Gap]
- [x] CHK004 - Is the exact opt-in mechanism documented — specifically which property and which value a project must set? [Completeness, Spec §FR-002]
- [x] CHK005 - Are requirements specified for all five existing project types in the repository (library, test, benchmark, console smoke, quickstart smoke)? [Completeness, Spec §FR-006]

## Requirement Clarity

- [x] CHK006 - Is "standard pack command" quantified — does it mean `dotnet pack` with no arguments, `dotnet pack <solution>`, or `dotnet pack <project>`? [Clarity, Spec §FR-005]
- [x] CHK007 - Is "shared generic description" defined with the actual text or a strategy for deriving it? [Clarity, Spec §FR-003, Clarifications §Session 2026-04-04]
- [x] CHK008 - Is "repository URL" specified as a concrete value or a derivation rule (e.g., from git remote)? [Clarity, Spec §FR-003]
- [x] CHK009 - Is "package tags" defined with the actual tag values or a selection strategy? [Clarity, Spec §FR-003]
- [x] CHK010 - Is "minimal extra configuration" in FR-007 quantified — does "single opt-in property" mean exactly one XML element added to the project file? [Clarity, Spec §FR-007, SC-002]
- [x] CHK011 - Is "discoverable" in FR-008 defined with specific artifacts (e.g., a README section, inline XML comments, a contributing guide)? [Ambiguity, Spec §FR-008]

## Requirement Consistency

- [x] CHK012 - Are the metadata fields listed in FR-003 consistent with those referenced in SC-004? [Consistency, Spec §FR-003, SC-004]
- [x] CHK013 - Does the "non-packable by default" rule in FR-002 align with edge case #7 (newly added project not designated as packable)? [Consistency, Spec §FR-002, Edge Cases]
- [x] CHK014 - Is the package ID default in FR-010 ("project name / assembly name") consistent with the Assumptions section ("the standard .NET SDK behavior")? [Consistency, Spec §FR-010, Assumptions]
- [x] CHK015 - Are User Story 2 acceptance scenarios consistent with FR-006 — do both cover the same project categories (test, executable, sample, internal)? [Consistency, Spec §US-2, FR-006]

## Acceptance Criteria Quality

- [x] CHK016 - Can SC-001 ("defined in exactly one location") be objectively measured — is the verification method specified (e.g., grep for metadata properties across all `.csproj` files)? [Measurability, Spec §SC-001]
- [x] CHK017 - Can SC-004 ("expected shared metadata") be objectively verified — is the inspection method specified (e.g., `dotnet nuget inspect`, extracting `.nuspec` from `.nupkg`)? [Measurability, Spec §SC-004]
- [x] CHK018 - Can SC-006 ("same packaging outcome") be objectively measured — does the spec define what "same outcome" means (identical file set, identical metadata, identical binary content)? [Measurability, Spec §SC-006]
- [x] CHK019 - Are success criteria SC-002 through SC-005 each traceable to at least one specific functional requirement? [Traceability, Spec §SC-002 through SC-005]

## Scenario Coverage

- [x] CHK020 - Are requirements defined for the scenario where a project that was previously packable is changed to non-packable (opt-out after opt-in)? [Coverage, Gap]
- [x] CHK021 - Are requirements defined for what happens when `dotnet pack` is run at the solution level vs. targeting a specific project? [Coverage, Spec §FR-005]
- [x] CHK022 - Are requirements defined for the scenario where a project inherits shared metadata AND has its own `Directory.Build.props` in a subdirectory? [Coverage, Gap]
- [x] CHK023 - Are the acceptance scenarios in all five user stories sufficient to validate FR-001 through FR-010 — is every FR covered by at least one scenario? [Coverage]

## Edge Case Coverage

- [x] CHK024 - Is the behavior specified when the shared `Version` property is empty or missing from `Directory.Build.props`? [Edge Case, Gap]
- [x] CHK025 - Is the behavior specified when a project sets `<IsPackable>true</IsPackable>` but also sets `<OutputType>Exe</OutputType>`? [Edge Case, Gap]
- [x] CHK026 - Is the edge case of conflicting properties documented — e.g., what if a future intermediate `Directory.Build.props` in `tests/` overrides the root settings? [Edge Case, Gap]
- [x] CHK027 - Does the spec address what happens when multiple metadata fields are overridden in a single project — partial override vs. full override? [Edge Case, Spec §FR-004]

## Non-Functional Requirements

- [x] CHK028 - Are maintainability requirements (NFR-1) specific enough to be evaluated — is "minimize duplication" defined with a threshold or measurable criterion? [Measurability, Spec §NFR-1]
- [x] CHK029 - Are extensibility requirements (NFR-4) specified with concrete extension scenarios (e.g., "adding source link should require adding N properties")? [Clarity, Spec §NFR-4]

## Dependencies & Assumptions

- [x] CHK030 - Is the assumption that `Directory.Build.props` does not already exist in the repository validated against the current repository state? [Assumption]
- [x] CHK031 - Is the assumption about MSBuild property evaluation order (`.csproj` overrides `Directory.Build.props`) documented as a dependency on standard .NET SDK behavior? [Dependency, Assumptions]
- [x] CHK032 - Are deferred items (symbol packages, source link, README inclusion, license file embedding) documented clearly enough that a future feature can pick them up without ambiguity? [Completeness, Assumptions]

## Notes

- Focus: Broad coverage across all requirement quality dimensions
- Depth: Standard (~30 items)
- Audience: Author self-review before implementation
- All items test **requirement quality** (completeness, clarity, consistency, measurability), not implementation correctness
