using Assertivo;

namespace Assertivo.Tests;

public class AllSatisfyDiagnosticsTests
{
	[Fact]
	public void AllSatisfy_FailureDetails_ShouldPreserveAscendingIndexOrder()
	{
		IEnumerable<int> subject = [10, 20, 30];

		var exception = Assert.Throws<AssertionFailedException>(() =>
			subject.Should().AllSatisfy(value => value.Should().Be(0)));

		var firstIndexPosition = exception.Message.IndexOf("[0]", StringComparison.Ordinal);
		var secondIndexPosition = exception.Message.IndexOf("[1]", StringComparison.Ordinal);
		var thirdIndexPosition = exception.Message.IndexOf("[2]", StringComparison.Ordinal);

		Assert.True(firstIndexPosition >= 0);
		Assert.True(firstIndexPosition < secondIndexPosition);
		Assert.True(secondIndexPosition < thirdIndexPosition);
	}

	[Fact]
	public void AllSatisfy_FailureMessage_ShouldIncludeFailingIndexTag()
	{
		IEnumerable<int> subject = [2, 4, 5, 6];

		var exception = Assert.Throws<AssertionFailedException>(() =>
			subject.Should().AllSatisfy(value => (value % 2).Should().Be(0)));

		Assert.Contains("[2]", exception.Message);
		Assert.Contains("Failing indices:", exception.Message);
	}

	[Fact]
	public void AllSatisfy_WhenFailuresExist_ShouldThrowAssertionFailedExceptionFallback()
	{
		IEnumerable<int> subject = [1];

		var exception = Assert.Throws<AssertionFailedException>(() =>
			subject.Should().AllSatisfy(value => value.Should().Be(0)));

		Assert.Contains("1 element(s) failed", exception.Message);
	}

	[Fact]
	public void AllSatisfy_WithBecause_ShouldPropagateReason()
	{
		IEnumerable<int> subject = [1, 2, 3];

		var exception = Assert.Throws<AssertionFailedException>(() =>
			subject.Should().AllSatisfy(value => value.Should().Be(0), "values must all be zero"));

		Assert.Contains("Because: values must all be zero", exception.Message);
	}
}
