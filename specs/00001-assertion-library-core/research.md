# Research: Assertion Library Core

**Feature**: 00001-assertion-library-core
**Date**: 2026-03-31

## Topic 1: Zero-allocation assertion pattern for value types

- **Decision**: Use `readonly struct` for assertion objects (e.g., `NumericAssertions<T>`) and `readonly struct` for `AndConstraint<T>` / `AndWhichConstraint<T, TSub>`. `.Should()` returns a struct by value; assertion methods return constraint structs by value. The entire happy path stays on the stack. Allocations only occur on the failure path (exception + message string).
- **Rationale**: `ref struct` was considered but rejected — it cannot be a generic type argument (blocks `AndConstraint<T>`), cannot be returned from async methods, and cannot implement interfaces. `readonly struct` gives zero-allocation on the happy path without ergonomic penalties. Struct copies are cheap (assertion objects hold only a subject value/reference + a captured expression string).
- **Alternatives Considered**: (1) `ref struct` — eliminates heap allocation entirely but cannot be used as generic type arguments, blocks fluent chaining pattern. Rejected. (2) `class` — allocates on every `.Should()` call even on the happy path, violating SC-006. Rejected for assertion objects; acceptable for singleton infrastructure types (strategy, formatter).

## Topic 2: Fluent chaining immutability (AndConstraint / AndWhichConstraint)

- **Decision**: Implement `AndConstraint<TAssertions>` and `AndWhichConstraint<TAssertions, TSubject>` as independent `readonly struct` types (no inheritance). `AndConstraint<T>` exposes a readonly `And` property returning `TAssertions`. `AndWhichConstraint<T, TSub>` exposes `And` (returning `TAssertions`) and `Which` (returning `TSubject`). Immutability is enforced structurally — all fields are `readonly` and set in the constructor.
- **Rationale**: FluentAssertions uses class-based inheritance (`AndWhichConstraint<T,S> : AndConstraint<T>`) but this forces heap allocation. Our types have exactly 1-2 fields; struct approach ensures zero-allocation chaining. `readonly struct` prevents field mutation after construction, satisfying FR-021 (immutability) and FR-022 (thread safety).
- **Alternatives Considered**: (1) Class with inheritance — cleaner code but allocates per call. Rejected. (2) Single struct with optional `Which` — loses type safety. Rejected. (3) Interface `IAndConstraint<T>` — boxing concerns. Rejected.

## Topic 3: Pluggable failure reporting without external dependencies

- **Decision**: Use a static `AssertionConfiguration` class with a `static Action<string> ReportFailure` delegate, defaulting to throwing `AssertionFailedException`. Framework adapter packages set the delegate at `[ModuleInitializer]` time. No runtime assembly probing.
- **Rationale**: A static delegate is simpler, AOT-safe, and zero-reflection. `[ModuleInitializer]` in adapter packages runs automatically when loaded, requiring zero configuration from the user. The delegate pattern avoids interface dispatch + allocation.
- **Alternatives Considered**: (1) Assembly probing at first use (à la FluentAssertions) — uses reflection, breaks AOT. Rejected. (2) `IAssertionStrategy` interface with DI — over-engineers a single-method contract. Rejected. (3) Thread-local strategy — unnecessary; test frameworks use one failure mechanism per process. Rejected.

## Topic 4: CallerArgumentExpression for assertion expression capture

- **Decision**: Apply `[CallerArgumentExpression("subject")]` to a `string? caller = null` parameter on every `.Should()` extension method overload. Store the captured expression in the assertion struct. Plumb into failure messages as "Expression: `result.Count`" when non-null. Do NOT apply to assertion method parameters.
- **Rationale**: `[CallerArgumentExpression]` captures the source text at compile time with zero runtime cost. Applying at `.Should()` captures the subject expression, which is the most valuable debugging info. The expected value is already in the message, so capturing it again would clutter signatures.
- **Alternatives Considered**: (1) `[CallerArgumentExpression]` on every assertion parameter — clutters API. Rejected. (2) `Expression<Func<T>>` capture — allocates, prevents inlining, breaks AOT. Rejected.

## Topic 5: NuGet package structure for .NET 10

- **Decision**: Single `.csproj` targeting `net10.0` with: `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `IsPackable=true`, `GenerateDocumentationFile=true`, `IsAotCompatible=true`, `PackageId=Assertivo`.
- **Rationale**: .NET 10 is current LTS. Single TFM avoids conditional compilation. `IsAotCompatible` enables trim/AOT/single-file analyzers automatically. `GenerateDocumentationFile` ships XML docs for IntelliSense.
- **Alternatives Considered**: (1) Multi-target `netstandard2.0;net10.0` — user explicitly requested .NET 10 only. Deferred. (2) Source-only package — prevents binary distribution. Rejected.

## Topic 6: AOT compatibility

- **Decision**: The generic type hierarchy is AOT-safe — all closed generic instantiations are statically visible from `.Should()` extension methods. Enable `<IsAotCompatible>true</IsAotCompatible>` to activate all AOT analyzers at build time. Avoid: `Type.MakeGenericType`, `Activator.CreateInstance`, `dynamic`, `Reflection.Emit`, `Assembly.Load*`. Safe: constrained generics, static delegates, `typeof(T)`, `Comparer<T>.Default`, `EqualityComparer<T>.Default`, `string.Format`.
- **Rationale**: Extension methods on concrete types produce known generic instantiations the compiler can analyze. Message formatting uses `string.Format`. Pluggable failure strategy uses a static delegate. Each struct generic instantiation produces specialized AOT code — acceptable for a small type surface (~15-20 types).
- **Alternatives Considered**: (1) Non-generic types with runtime casting — loses type safety. Rejected. (2) Source generators — enormous complexity for marginal binary size savings. Rejected.
