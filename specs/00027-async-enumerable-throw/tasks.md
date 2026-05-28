# Tasks: ThrowAsync Support for IAsyncEnumerable<T> Subjects

**Input**: Design documents from `/specs/00027-async-enumerable-throw/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · contracts/ ✅ · quickstart.md ✅

**Scope summary**:
- `src/Assertivo/Exceptions/AsyncEnumerableAssertions.cs` — NEW file (~80 lines)
- `src/Assertivo/Should.cs` — +1 extension method overload (`IAsyncEnumerable<T>?`)
- `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs` — NEW file (~130 lines)
- `tests/Assertivo.Tests/ShouldDispatchTests.cs` — +1 dispatch test

---

## Phase 1: Setup

**Purpose**: Verify the baseline is clean before any changes are made.

- [X] T001 Run `dotnet test tests/Assertivo.Tests/` and confirm all pre-existing tests pass with zero failures as a baseline before any edits

**Checkpoint**: Baseline green — proceed to Phase 2.

---

## Phase 2: Foundational

**Purpose**: Create the `AsyncEnumerableAssertions<T>` struct skeleton and the `Should<T>()` extension overload that every user-story task depends on for compilation and dispatch.

**⚠️ CRITICAL**: T002 and T003 must both be complete before any user-story task (T004+) begins. They can be executed in parallel with each other.

- [X] T002 [P] Create `src/Assertivo/Exceptions/AsyncEnumerableAssertions.cs` with: `using System.Diagnostics;` at the top; `namespace Assertivo.Exceptions;`; `public readonly struct AsyncEnumerableAssertions<T>` with struct-level XML `<summary>` doc ("Assertions for an <see cref="IAsyncEnumerable{T}"/> subject. Obtain an instance via <c>source.Should()</c>."); internal constructor `internal AsyncEnumerableAssertions(IAsyncEnumerable<T>? subject, string? expression)` assigning both fields; `public IAsyncEnumerable<T>? Subject { get; }` with XML doc ("Gets the async enumerable under test."); `internal string? Expression { get; }`; and a stubbed `[StackTraceHidden] public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs) where TException : Exception` with full XML docs (`<summary>` "Asserts that the async enumerable throws an exception of type <typeparamref name="TException"/> or a subtype when enumerated. Unwraps <see cref="AggregateException"/> with a single inner exception.", `<typeparam name="TException">` "The expected exception type (exact or base).", `<param name="because">` "An optional reason phrase for the assertion.", `<param name="becauseArgs">` "Optional format arguments for <paramref name="because"/>.", `<returns>` "A task resolving to <see cref="ExceptionAssertions{TException}"/> for inspecting the caught exception via <c>.Which</c>.") and body `throw new NotImplementedException();` — the stub is required so test references compile and produce observable failures before T009

- [X] T003 [P] Add `Should<T>(this IAsyncEnumerable<T>? subject, [CallerArgumentExpression(nameof(subject))] string? caller = null)` overload to `src/Assertivo/Should.cs` immediately after the existing `Should<TResult>(this Task<TResult>?...)` overload (line ~47) and before the `Should<T>(this Func<T> subject, ...)` overload; include XML `<summary>` doc ("Returns an <see cref="AsyncEnumerableAssertions{T}"/> for the specified async enumerable subject.") matching the existing overload style; return type is `AsyncEnumerableAssertions<T>`; body is `=> new(subject, caller);`; no new `using` directives are needed as `Assertivo.Exceptions` is already imported at the top of the file

**Checkpoint**: Foundation ready — project compiles, `source.Should()` resolves to `AsyncEnumerableAssertions<T>`, T004+ can now begin.

---

## Phase 3: User Story 1 — Assert Async Iterator Throws Expected Exception (Priority: P1) 🎯 MVP

**Goal**: `source.Should().ThrowAsync<TException>()` passes when the async iterator throws the expected type (exact or subtype); fails with a descriptive `AssertionFailedException` for every failure condition: no exception thrown, wrong exception type, null subject, `DisposeAsync` exception overlap. The `DisposeAsync`-exception-discard path — implemented via `catch when (caught is not null)` — is the novel code path unique to this entry point.

**Independent Test**: Declare `async IAsyncEnumerable<int> Source() { yield return 1; throw new InvalidOperationException("mid-stream"); }` and call `await Source().Should().ThrowAsync<InvalidOperationException>()` — assert passes with no lambda wrapper, no `await foreach`, no intermediate variable.

### Tests for User Story 1 (write first — MUST fail with `NotImplementedException` before T009) ⚠️

- [X] T004 [P] [US1] Create `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs` with class `public class AsyncEnumerableAssertionsTests`, required usings (`using Assertivo; using Assertivo.Exceptions; using System.Collections.Generic; using System.Threading;`), a private static `async IAsyncEnumerable<int> Throw(Exception ex) { yield return 1; throw ex; }` helper (yields one item then throws — used across most tests), and three tests: `ThrowAsync_CorrectType_Passes` (`await Throw(new InvalidOperationException()).Should().ThrowAsync<InvalidOperationException>()` — pass means no exception propagates from the await); `ThrowAsync_Subtype_Passes` (helper throws `new ArgumentNullException("p")`, call awaits `.Should().ThrowAsync<ArgumentException>()` requesting the base type — verifies subtype matching per FR-002); and `Should_Subject_ReturnsEnumerable` (declare `IAsyncEnumerable<int> src = Throw(new InvalidOperationException()); Assert.Same(src, src.Should().Subject)` — verifies the `Subject` property holds the original reference, constitution §IV.1 positive test)

- [X] T005 [P] [US1] Add `ThrowAsync_NoThrow_Fails` (helper `static async IAsyncEnumerable<int> Complete() { yield return 1; yield return 2; }`, call `await Complete().Should().ThrowAsync<InvalidOperationException>()`, assert `AssertionFailedException` is thrown and its `Message` contains both `"InvalidOperationException"` and `"no exception was thrown"` — matching the exact wording from R-003: `$"source to throw {typeof(TException).Name}"` as expected arg and `"no exception was thrown"` as actual arg) and `ThrowAsync_WrongType_Fails` (helper throws `new ArgumentException("bad arg")`, call awaits `.Should().ThrowAsync<InvalidOperationException>()`, assert `AssertionFailedException` message contains `"InvalidOperationException"` as expected type, `"ArgumentException"` as actual type, and `"bad arg"` — the actual exception's `.Message` text — per FR-006 and R-003 wrong-type template) in `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`

- [X] T006 [P] [US1] Add `ThrowAsync_AggregateException_SingleInner_Unwraps` (helper throws `new AggregateException(new InvalidOperationException("inner-msg"))`, call awaits `.Should().ThrowAsync<InvalidOperationException>()` — assert passes, meaning the single-inner `AggregateException` is correctly unwrapped to its inner `InvalidOperationException` per FR-004 and R-004; `.Which.Message` inspection is deferred to T011 in US2) and `ThrowAsync_AggregateException_MultipleInners_DoesNotUnwrap` (helper throws `new AggregateException(new InvalidOperationException(), new ArgumentException())`, call awaits `.Should().ThrowAsync<InvalidOperationException>()`, assert `AssertionFailedException` is thrown and message contains `"AggregateException"` as the actual type — multi-inner aggregate is not unwrapped and falls through to the wrong-type failure path per FR-004); and `ThrowAsync_AggregateException_ZeroInners_DoesNotUnwrap` (helper throws `new AggregateException()` — an `AggregateException` with zero inner exceptions — call awaits `.Should().ThrowAsync<InvalidOperationException>()`, assert `AssertionFailedException` is thrown and message contains `"AggregateException"` as the actual type — the zero-inner condition is not met, so no unwrapping occurs, and the `AggregateException` itself is the caught exception per FR-004 and spec edge case 4) in `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`

- [X] T007 [P] [US1] Add `ThrowAsync_NullSubject_Fails` (`IAsyncEnumerable<int>? nullSrc = null;`, call `await nullSrc.Should().ThrowAsync<InvalidOperationException>()`, assert `AssertionFailedException` is thrown and `Message` contains both `"source to be non-null"` and `"source was null"` — the exact wording from R-003 null-subject template; verifies FR-011 and R-006: null guard inside `ThrowAsync`, not at `.Should()` call site); `ThrowAsync_CallerExpression_AppearsInMessage` (declare `IAsyncEnumerable<int>? myAsyncSource = null;`, call `myAsyncSource.Should().ThrowAsync<InvalidOperationException>()`, catch `AssertionFailedException`, assert `ex.Message` contains `"myAsyncSource"` — verifies `[CallerArgumentExpression]` threading from `Should<T>()` through the struct to `MessageFormatter.Fail`); and `Should_NullSubject_SubjectIsNull` (`IAsyncEnumerable<int>? nullSrc = null; Assert.Null(nullSrc.Should().Subject)` — verifies null is accepted and stored, not thrown at `.Should()` call time per R-006) in `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`

- [X] T008 [P] [US1] Add `ThrowAsync_DisposeAsync_ExceptionDiscarded_WhenIterationAlreadyThrew` to `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`: declare a private nested class `DualFaultEnumerable : IAsyncEnumerable<int>` with inner `Enumerator : IAsyncEnumerator<int>` where `Current => 0`, `MoveNextAsync()` returns `ValueTask.FromException<bool>(new InvalidOperationException("iteration error"))` — note: throws on the very first `MoveNextAsync()` call before any item is yielded, which also covers spec edge case EC2 —, and `DisposeAsync()` returns `ValueTask.FromException(new InvalidOperationException("dispose error"))`; `GetAsyncEnumerator(CancellationToken cancellationToken = default)` returns `new Enumerator()`; then in the test: `var result = await new DualFaultEnumerable().Should().ThrowAsync<InvalidOperationException>();` — assert the call **passes** (no exception propagates) and `result.Which.Message == "iteration error"` (the iteration exception is preserved, not replaced by the disposal exception). This is the direct test of the `catch when (caught is not null)` exception filter required by R-001 and spec edge case 7.

### Implementation for User Story 1

- [X] T009 [US1] Replace the `throw new NotImplementedException();` stub with the complete `ThrowAsync<TException>` implementation in `src/Assertivo/Exceptions/AsyncEnumerableAssertions.cs` using the following structure (all 7 code paths per data-model §Code Paths): **(1)** null guard at top — `if (Subject is null) MessageFormatter.Fail("source to be non-null", "source was null", Expression, because, becauseArgs);`; **(2)** declare `Exception? caught = null;` and `var enumerator = Subject!.GetAsyncEnumerator();`; **(3)** outer `try { while (true) { bool hasNext; try { hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false); } catch (Exception ex) { caught = ex; break; } if (!hasNext) break; } } finally { try { await enumerator.DisposeAsync().ConfigureAwait(false); } catch when (caught is not null) { /* discard — original iteration exception takes precedence */ } }` — the `catch when (caught is not null)` filter is the critical mechanism that preserves the iteration exception over any `DisposeAsync` exception (R-001; spec edge case 7); **(4)** after the `try/finally`, check `if (caught is not null)` — inside: `var target = caught;` then AggregateException single-inner unwrap: `if (caught is AggregateException { InnerExceptions.Count: 1 } ae) target = ae.InnerExceptions[0];` then type check: `if (target is TException typed) return new ExceptionAssertions<TException>(typed, Expression);` then wrong-type failure: `MessageFormatter.Fail(typeof(TException).Name, $"{target.GetType().Name}: {target.Message}", Expression, because, becauseArgs); return default!;`; **(5)** after the `if (caught is not null)` block: `MessageFormatter.Fail($"source to throw {typeof(TException).Name}", "no exception was thrown", Expression, because, becauseArgs); return default!;` — the `return default!;` after every `Fail` call satisfies the compiler since `MessageFormatter.Fail` is `[DoesNotReturn]` but the method returns a `Task<...>`; keep `[StackTraceHidden]` and all XML docs from the T002 stub unchanged

**Checkpoint**: US1 fully functional and independently testable. MVP deliverable complete — `source.Should().ThrowAsync<X>()` works for all 7 code paths including the `DisposeAsync`-exception-discard path.

---

## Phase 4: User Story 2 — Chain Further Assertions on the Caught Exception (Priority: P2)

**Goal**: The `ExceptionAssertions<TException>` returned by `ThrowAsync` exposes `.Which` providing direct access to the caught exception for further assertion chaining, with `Expression` threaded through so that chained failures identify the originating source. No new production code is needed — the return type (`ExceptionAssertions<TException>`) was implemented in T009; this phase adds tests verifying the chaining API.

**Independent Test**: Await `source.Should().ThrowAsync<InvalidOperationException>()`, then call `.Which.Message.Should().Contain("expected-text")` — no manual `try/catch`, no re-enumeration.

### Tests for User Story 2

- [X] T010 [P] [US2] Add `ThrowAsync_Which_ExposesTypedCaughtException` (helper throws `new ArgumentNullException("paramName")`, `var result = await Throw(new ArgumentNullException("paramName")).Should().ThrowAsync<ArgumentNullException>();`, assert `result.Which.ParamName == "paramName"` — verifies `.Which` provides the strongly-typed caught exception, not a base `Exception` reference, per FR-008 and SC-004) and `ThrowAsync_Which_EnablesMessageChaining` (helper throws `new InvalidOperationException("expected text")`, assert `(await Throw(new InvalidOperationException("expected text")).Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("expected text")` — verifies fluent chaining through `.Which.Message.Should()` compiles and passes without re-enumerating the source) in `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`

- [X] T011 [P] [US2] Add `ThrowAsync_AggregateUnwrap_WhichReflectsInnerException` (helper throws `new AggregateException(new InvalidOperationException("inner-msg"))`, `var result = await Throw(new AggregateException(new InvalidOperationException("inner-msg"))).Should().ThrowAsync<InvalidOperationException>();`, assert `result.Which.Message == "inner-msg"` — verifies that after single-inner `AggregateException` unwrapping, `.Which` holds the inner exception (not the `AggregateException` wrapper), satisfying US2 acceptance scenario 2 and SC-004) in `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`

**Checkpoint**: US2 verified — `.Which` chaining provides the expected exception instance with full type fidelity and no re-enumeration.

---

## Phase 5: User Story 3 — Provide Contextual Failure Messages with Because (Priority: P3)

**Goal**: `because` and `becauseArgs` are threaded through `MessageFormatter.Fail` for every failure code path in `ThrowAsync`, so CI output includes developer-provided context without extra tooling. No new production code is needed — the `because`/`becauseArgs` parameters were wired in T009.

**Independent Test**: Call `.ThrowAsync<InvalidOperationException>("because {0} is required", "the contract")` on a source that completes without throwing; catch the `AssertionFailedException` and assert its message contains `"the contract"`.

### Tests for User Story 3

- [X] T012 [P] [US3] Add `ThrowAsync_Because_NoThrow_IncludesFormattedReason` (call `Complete().Should().ThrowAsync<InvalidOperationException>("because {0} is required", "validation")` where `Complete()` yields without throwing, catch `AssertionFailedException`, assert `ex.Message` contains `"validation"` — verifies `becauseArgs` are formatted and included in the no-throw failure path); `ThrowAsync_Because_WrongType_IncludesReason` (helper throws `new ArgumentException("x")`, call `.ThrowAsync<InvalidOperationException>("because of contract X")`, catch `AssertionFailedException`, assert `ex.Message` contains `"because of contract X"` — verifies `because` literal appears in the wrong-type failure path); and `ThrowAsync_Because_NullSubject_IncludesReason` (`IAsyncEnumerable<int>? nullSrc = null;`, call `nullSrc.Should().ThrowAsync<InvalidOperationException>("because {0}", "the test requires it")`, catch `AssertionFailedException`, assert `ex.Message` contains `"the test requires it"` — verifies `because` threading covers the null-subject failure path, all three failure paths per FR-007 and US3 acceptance scenarios 1 and 2) in `tests/Assertivo.Tests/AsyncEnumerableAssertionsTests.cs`

**Checkpoint**: US3 verified — `because`/`becauseArgs` confirmed across all three observable failure paths (no-throw, wrong type, null subject).

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Add the dispatch regression test for `IAsyncEnumerable<T>`, confirm zero regressions across all pre-existing assertion types, and verify the new overload does not shadow any existing dispatch path.

- [X] T013 [P] Add the following test to `tests/Assertivo.Tests/ShouldDispatchTests.cs`: `Should_AsyncEnumerableSubject_ReturnsAsyncEnumerableAssertions` (`async IAsyncEnumerable<int> Source() { yield return 1; }`, assert `Assert.IsType<AsyncEnumerableAssertions<int>>(Source().Should())` — verifies that `IAsyncEnumerable<int>` no longer falls through to `ObjectAssertions<IAsyncEnumerable<int>>` and instead resolves to the new `AsyncEnumerableAssertions<T>` overload at compile time per FR-001 and FR-012); add `using Assertivo.Exceptions;` to the file if not already present

- [X] T014 Run `dotnet test tests/Assertivo.Tests/ /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=95 /p:ThresholdType=line /p:ThresholdStat=total` to confirm SC-002 zero regressions AND enforce the constitution §IV.4.1 coverage gate — all pre-existing `AsyncFunctionAssertionsTests`, `TaskAssertionsTests`, `ShouldDispatchTests`, and all other test classes must pass alongside the new `AsyncEnumerableAssertionsTests`; the build step MUST fail if line coverage falls below 95%

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on T001 passing — **BLOCKS all user story tasks**
- **User Story phases (Phase 3–5)**: All depend on T002 + T003 completion
  - T004–T008 (US1 tests) can be authored in parallel as soon as T002 + T003 are done
  - T009 (US1 implementation) depends on T004–T008 existing with observed `NotImplementedException` failures
  - T010–T011 (US2 tests) and T012 (US3 tests) depend on T009 (test the completed implementation)
- **Polish (Phase 6)**: T013 depends on T003 (overload exists to test dispatch); T014 depends on T009 + T013

### User Story Dependencies

```
US1 (P1) ──────────────────────────────────── Independent. Delivers standalone MVP value.
US2 (P2) ──── depends on T009 (no new prod code; tests only)
US3 (P3) ──── depends on T009 (no new prod code; tests only)
```

### Within User Story 1 (Phase 3)

- T004–T008 MUST be written and confirmed to fail with `NotImplementedException` before T009 is implemented (Red-Green per constitution §IV.3)
- T004–T008 can be authored in parallel (independent `[Fact]` methods targeting the same new file)
- T009 MUST follow after all test tasks are authored and confirmed failing

### Parallel Execution Examples

**Phase 2** (parallel — different files):
```
T002 → src/Assertivo/Exceptions/AsyncEnumerableAssertions.cs
T003 → src/Assertivo/Should.cs
```

**Phase 3 tests** (parallel — same file, independent [Fact] methods):
```
T004 → ThrowAsync_CorrectType_Passes, ThrowAsync_Subtype_Passes, Should_Subject_ReturnsEnumerable
T005 → ThrowAsync_NoThrow_Fails, ThrowAsync_WrongType_Fails
T006 → ThrowAsync_AggregateException_SingleInner_Unwraps, ThrowAsync_AggregateException_MultipleInners_DoesNotUnwrap
T007 → ThrowAsync_NullSubject_Fails, ThrowAsync_CallerExpression_AppearsInMessage, Should_NullSubject_SubjectIsNull
T008 → ThrowAsync_DisposeAsync_ExceptionDiscarded_WhenIterationAlreadyThrew
```

**Phase 4 + 5** (parallel — no production code changes):
```
T010 → ThrowAsync_Which_ExposesTypedCaughtException, ThrowAsync_Which_EnablesMessageChaining
T011 → ThrowAsync_AggregateUnwrap_WhichReflectsInnerException
T012 → ThrowAsync_Because_NoThrow_IncludesFormattedReason, ThrowAsync_Because_WrongType_IncludesReason, ThrowAsync_Because_NullSubject_IncludesReason
```

---

## Implementation Strategy

**MVP scope**: Phase 1 → Phase 2 → Phase 3 (US1) alone delivers the primary value. A developer can assert async iterator faults without any lambda wrapper.

**Incremental delivery order**:
1. Phase 1+2: Foundation — compile-clean, `source.Should()` dispatches correctly
2. Phase 3: US1 — full `ThrowAsync` with all 7 code paths working (T001–T009)
3. Phase 4: US2 — `.Which` chaining verified (no prod changes, tests only)
4. Phase 5: US3 — `because` phrasing verified (no prod changes, tests only)
5. Phase 6: Dispatch regression + full suite green
