using Assertivo;

namespace Assertivo.Tests;

public class AllSatisfyIndexAwareTests
{
	[Fact]
	public void AllSatisfy_IndexAwareInspector_ShouldReceiveZeroBasedIndices()
	{
		IEnumerable<int> subject = [10, 20, 30];
		var observedIndices = new List<int>();

		subject.Should().AllSatisfy((value, index) =>
		{
			observedIndices.Add(index);
			value.Should().Be((index + 1) * 10);
		});

		Assert.Equal([0, 1, 2], observedIndices);
	}

	[Fact]
	public void AllSatisfy_IndexAwareViolation_ShouldReportMatchingFailureIndex()
	{
		IEnumerable<int> subject = [0, 99, 2];

		var exception = Assert.Throws<AssertionFailedException>(() =>
			subject.Should().AllSatisfy((value, index) => value.Should().Be(index)));

		Assert.Contains("[1]", exception.Message);
	}
}
