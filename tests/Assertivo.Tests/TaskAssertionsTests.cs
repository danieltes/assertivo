using Assertivo;
using Assertivo.Exceptions;

namespace Assertivo.Tests;

public class TaskAssertionsTests
{
    // ── T004: Positive and subject property ──────────────────────────────────

    [Fact]
    public async Task ThrowAsync_CorrectType_Passes()
    {
        Task task = Task.FromException(new InvalidOperationException());
        await task.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_Subtype_Passes()
    {
        Task task = Task.FromException(new ArgumentNullException());
        await task.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Should_Subject_ReturnsOriginalTask()
    {
        Task task = Task.CompletedTask;
        Assert.Same(task, task.Should().Subject);
    }

    // ── T005: No-throw and wrong-type failures ───────────────────────────────

    [Fact]
    public async Task ThrowAsync_NoThrow_Fails()
    {
        Task task = Task.CompletedTask;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            task.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("InvalidOperationException", ex.Message);
        Assert.Contains("no exception was thrown", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_WrongType_Fails()
    {
        Task task = Task.FromException(new ArgumentException("bad arg"));
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            task.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("InvalidOperationException", ex.Message);
        Assert.Contains("ArgumentException", ex.Message);
        Assert.Contains("bad arg", ex.Message);
    }

    // ── T006: AggregateException unwrapping ──────────────────────────────────

    [Fact]
    public async Task ThrowAsync_AggregateException_SingleInner_Unwraps()
    {
        Task task = Task.FromException(new AggregateException(new InvalidOperationException("inner-msg")));
        await task.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_AggregateException_MultipleInners_Fails()
    {
        Task task = Task.FromException(new AggregateException(new InvalidOperationException(), new ArgumentException()));
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            task.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("AggregateException", ex.Message);
    }

    // ── T007: Null subject and CallerArgumentExpression ──────────────────────

    [Fact]
    public async Task ThrowAsync_NullSubject_Fails()
    {
        Task? nullTask = null;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            nullTask.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("null", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_CallerExpression_AppearsInFailureMessage()
    {
        Task? namedTask = Task.CompletedTask;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            namedTask.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("namedTask", ex.Message);
    }

    [Fact]
    public void Should_NullSubject_SubjectIsNull()
    {
        Task? nullTask = null;
        Assert.Null(nullTask.Should().Subject);
    }

    // ── T008: Cancellation ───────────────────────────────────────────────────

    [Fact]
    public async Task ThrowAsync_CancelledTask_MatchesCancellationException()
    {
        Task task = Task.FromCanceled(new CancellationToken(canceled: true));
        await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ThrowAsync_CancelledTask_WrongType_Fails()
    {
        Task task = Task.FromCanceled(new CancellationToken(canceled: true));
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            task.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("CanceledException", ex.Message);
    }

    // ── T010: US2 — .Which chaining ──────────────────────────────────────────

    [Fact]
    public async Task ThrowAsync_Which_ExposesException()
    {
        Task task = Task.FromException(new ArgumentNullException("paramName"));
        var result = await task.Should().ThrowAsync<ArgumentNullException>();
        Assert.Equal("paramName", result.Which.ParamName);
    }

    [Fact]
    public async Task ThrowAsync_Which_ChainsToShould()
    {
        Task task = Task.FromException(new InvalidOperationException("expected text"));
        (await task.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("expected text");
    }

    // ── T011: US2 — .Which after AggregateException unwrap ───────────────────

    [Fact]
    public async Task ThrowAsync_AggregateUnwrap_WhichReflectsInnerMessage()
    {
        Task task = Task.FromException(new AggregateException(new InvalidOperationException("inner-msg")));
        var result = await task.Should().ThrowAsync<InvalidOperationException>();
        Assert.Equal("inner-msg", result.Which.Message);
    }

    // ── T012: US3 — because / becauseArgs threading ──────────────────────────

    [Fact]
    public async Task ThrowAsync_Because_NoThrow_IncludesFormattedReason()
    {
        Task task = Task.CompletedTask;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            task.Should().ThrowAsync<InvalidOperationException>("because {0} requires it", "validation"));
        Assert.Contains("validation", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_Because_WrongType_IncludesReason()
    {
        Task task = Task.FromException(new ArgumentException("x"));
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            task.Should().ThrowAsync<InvalidOperationException>("because of contract X"));
        Assert.Contains("because of contract X", ex.Message);
    }

    [Fact]
    public async Task ThrowAsync_Because_NullSubject_IncludesReason()
    {
        Task? nullTask = null;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(() =>
            nullTask.Should().ThrowAsync<InvalidOperationException>("because {0}", "the test requires it"));
        Assert.Contains("the test requires it", ex.Message);
    }
}
