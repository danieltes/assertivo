# assertivo Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-05-17

## Active Technologies
- C# / .NET 10.0 (MSBuild/SDK-style projects) + None (standard .NET SDK MSBuild only) (00002-nuget-packaging)
- C# 13 / .NET 10 + .NET BCL only (zero third-party; constitution constraint) (00018-should-type-dispatch)
- C# 13 / .NET 10.0 + .NET BCL only (zero third-party; AOT-compatible) (00021-fix-dict-not-be-null)
- C# 13 / .NET 10 (`net10.0`) + .NET BCL only (zero third-party dependencies) (00023-all-elements-api)
- C# 12 / .NET 8 (multi-target, same as existing codebase) + BCL only — zero third-party dependencies (constitution requirement) (00025-sequence-equal)
- C# 13 / .NET 10 + BCL only — `System.Threading.Tasks.Task`, `System.AggregateException`, `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute`, `System.Diagnostics.StackTraceHiddenAttribute` (00026-task-should-assertions)

- C# 14 / .NET 10 + None (zero third-party dependencies per constitution) (00001-assertion-library-core)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for C# 14 / .NET 10

## Code Style

C# 14 / .NET 10: Follow standard conventions

## Recent Changes
- 00026-task-should-assertions: Added C# 13 / .NET 10 + BCL only — `System.Threading.Tasks.Task`, `System.AggregateException`, `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute`, `System.Diagnostics.StackTraceHiddenAttribute`
- 00025-sequence-equal: Added C# 12 / .NET 8 (multi-target, same as existing codebase) + BCL only — zero third-party dependencies (constitution requirement)
- 00023-all-elements-api: Added C# 13 / .NET 10 (`net10.0`) + .NET BCL only (zero third-party dependencies)


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
