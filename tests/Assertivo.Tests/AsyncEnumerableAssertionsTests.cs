using System.Threading;
using Assertivo;
using Assertivo.Exceptions;

namespace Assertivo.Tests;

public class AsyncEnumerableAssertionsTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async IAsyncEnumerable<int> Throw(Exception ex)
    {
        yield return 1;
        throw ex;
    }

    private static async IAsyncEnumerable<int> Complete()
    {
        yield return 1;
        yield return 2;
    }

    // ── T004: US1 core pass paths + Subject property ─────────────────────────

    [Fact]
    public async Task ThrowAsync_CorrectType_Passes()
    {
        await Throw(new InvalidOperationException()).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_Subtype_Passes()
    {
        await Throw(new ArgumentNullException("p")).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void Should_Subject_ReturnsEnumerable()
    {
        IAsyncEnumerable<int> src = Throw(new InvalidOperationException());
        Assert.Same(src, src.Should().Subject);
    }

    // ── T005: US1 failure paths ───────────────────────────────────────────────

    [Fact]
    public async Task ThrowAsync_NoThrow_Fails()
    {
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Complete().Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("InvalidOperationException", ex.Message);
        Assert.Contains("no exception was thrown", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_WrongType_Fails()
    {
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Throw(new ArgumentException("bad arg")).Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("InvalidOperationException", ex.Message);
        Assert.Contains("ArgumentException", ex.Message);
        Assert.Contains("bad arg", ex.Message);
    }

    // ── T006: US1 AggregateException unwrap paths ────────────────────────────

    [Fact]
    public async Task ThrowAsync_AggregateException_SingleInner_Unwraps()
    {
        await Throw(new AggregateException(new InvalidOperationException("inner-msg")))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_AggregateException_MultipleInners_DoesNotUnwrap()
    {
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Throw(new AggregateException(new InvalidOperationException(), new ArgumentException()))
                .Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("AggregateException", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_AggregateException_ZeroInners_DoesNotUnwrap()
    {
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Throw(new AggregateException())
                .Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("AggregateException", ex.Message);
    }

    // ── T007: US1 null subject + caller expression ────────────────────────────

    [Fact]
    public async Task ThrowAsync_NullSubject_Fails()
    {
        IAsyncEnumerable<int>? nullSrc = null;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await nullSrc.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("source to be non-null", ex.Message);
        Assert.Contains("source was null", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_CallerExpression_AppearsInMessage()
    {
        IAsyncEnumerable<int>? myAsyncSource = null;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await myAsyncSource.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("myAsyncSource", ex.Message);
    }

    [Fact]
    public void Should_NullSubject_SubjectIsNull()
    {
        IAsyncEnumerable<int>? nullSrc = null;
        Assert.Null(nullSrc.Should().Subject);
    }

    // ── T008: US1 DisposeAsync exception-discard path ─────────────────────────

    private sealed class DualFaultEnumerable : IAsyncEnumerable<int>
    {
        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator();

        private sealed class Enumerator : IAsyncEnumerator<int>
        {
            public int Current => 0;

            public ValueTask<bool> MoveNextAsync()
                => ValueTask.FromException<bool>(new InvalidOperationException("iteration error"));

            public ValueTask DisposeAsync()
                => ValueTask.FromException(new InvalidOperationException("dispose error"));
        }
    }

    [Fact]
    public async Task ThrowAsync_DisposeAsync_ExceptionDiscarded_WhenIterationAlreadyThrew()
    {
        IAsyncEnumerable<int> source = new DualFaultEnumerable();
        var result = await source.Should().ThrowAsync<InvalidOperationException>();
        Assert.Equal("iteration error", result.Which.Message);
    }

    // ── T010: US2 .Which chaining ────────────────────────────────────────────

    [Fact]
    public async Task ThrowAsync_Which_ExposesTypedCaughtException()
    {
        var result = await Throw(new ArgumentNullException("paramName")).Should().ThrowAsync<ArgumentNullException>();
        Assert.Equal("paramName", result.Which.ParamName);
    }

    [Fact]
    public async Task ThrowAsync_Which_EnablesMessageChaining()
    {
        (await Throw(new InvalidOperationException("expected text")).Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("expected text");
    }

    // ── T011: US2 AggregateException unwrap .Which ────────────────────────────

    [Fact]
    public async Task ThrowAsync_AggregateUnwrap_WhichReflectsInnerException()
    {
        var result = await Throw(new AggregateException(new InvalidOperationException("inner-msg")))
            .Should().ThrowAsync<InvalidOperationException>();
        Assert.Equal("inner-msg", result.Which.Message);
    }

    // ── T012: US3 because/becauseArgs threading ───────────────────────────────

    [Fact]
    public async Task ThrowAsync_Because_NoThrow_IncludesFormattedReason()
    {
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Complete().Should().ThrowAsync<InvalidOperationException>("because {0} is required", "validation"));
        Assert.Contains("validation", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_Because_WrongType_IncludesReason()
    {
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Throw(new ArgumentException("x")).Should().ThrowAsync<InvalidOperationException>("because of contract X"));
        Assert.Contains("because of contract X", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_Because_NullSubject_IncludesReason()
    {
        IAsyncEnumerable<int>? nullSrc = null;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await nullSrc.Should().ThrowAsync<InvalidOperationException>("because {0}", "the test requires it"));
        Assert.Contains("the test requires it", ex.Message);
    }
}
