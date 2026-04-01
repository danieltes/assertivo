using Assertivo;
using Assertivo.Exceptions;

namespace Assertivo.Tests;

public class AsyncFunctionAssertionsTests
{
    [Fact]
    public async Task ThrowAsync_CorrectType_Passes()
    {
        Func<Task> act = () => throw new InvalidOperationException("async error");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_Subclass_Passes()
    {
        Func<Task> act = () => throw new ArgumentNullException("param");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ThrowAsync_WrongType_Fails()
    {
        Func<Task> act = () => throw new InvalidOperationException("wrong");
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(async () =>
            await act.Should().ThrowAsync<ArgumentException>());
        Assert.Contains("ArgumentException", ex.Expected);
        Assert.Contains("InvalidOperationException", ex.Actual);
    }

    [Fact]
    public async Task ThrowAsync_NoThrow_Fails()
    {
        Func<Task> act = () => Task.CompletedTask;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(async () =>
            await act.Should().ThrowAsync<InvalidOperationException>());
        Assert.Contains("no exception was thrown", ex.Actual);
    }

    [Fact]
    public async Task ThrowAsync_AggregateException_Unwraps()
    {
        Func<Task> act = () => throw new AggregateException(new InvalidOperationException("inner"));
        var result = await act.Should().ThrowAsync<InvalidOperationException>();
        Assert.Equal("inner", result.Which.Message);
    }

    [Fact]
    public async Task ThrowAsync_Which_ExposesException()
    {
        Func<Task> act = () => throw new ArgumentNullException("myParam");
        var result = await act.Should().ThrowAsync<ArgumentNullException>();
        Assert.Equal("myParam", result.Which.ParamName);
    }

    [Fact]
    public async Task ThrowAsync_Which_ChainsToShould()
    {
        Func<Task> act = () => throw new InvalidOperationException("detailed error");
        var result = await act.Should().ThrowAsync<InvalidOperationException>();
        result.Which.Message.Should().Contain("detailed");
    }

    [Fact]
    public async Task ThrowAsync_Because_IncludesReason()
    {
        Func<Task> act = () => Task.CompletedTask;
        var ex = await Assert.ThrowsAsync<AssertionFailedException>(async () =>
            await act.Should().ThrowAsync<InvalidOperationException>(because: "async should throw"));
        Assert.Contains("async should throw", ex.Message);
    }
}
