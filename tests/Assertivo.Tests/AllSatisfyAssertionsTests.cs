using Assertivo;
using Assertivo.Primitives;

namespace Assertivo.Tests;

public class AllSatisfyAssertionsTests
{
	[Fact]
	public void AllSatisfy_AllElementsPass_ShouldSucceed()
	{
		IEnumerable<int> subject = [2, 4, 6];

		subject.Should().AllSatisfy(value =>
		{
			value.Should().BeGreaterThanOrEqualTo(2);
			(value % 2).Should().Be(0);
		});
	}

	[Fact]
	public void AllSatisfy_EmptyCollection_ShouldPassVacuously()
	{
		IEnumerable<int> subject = [];

		subject.Should().AllSatisfy(_ => { });
	}

	[Fact]
	public void AllSatisfy_PassingInspector_ShouldReturnAndConstraintForChaining()
	{
		IEnumerable<int> subject = [1, 2, 3];

		AndConstraint<Collections.GenericCollectionAssertions<int>> chained =
			subject.Should().AllSatisfy(value => value.Should().BeGreaterThanOrEqualTo(1));

		chained.And.HaveCount(3);
	}

	[Fact]
	public void AllSatisfy_NullSubject_ShouldFailThroughAssertionPipeline()
	{
		IEnumerable<int>? subject = null;

		Assert.Throws<AssertionFailedException>(() => subject.Should().AllSatisfy(_ => { }));
	}

	[Fact]
	public void AllSatisfy_NullInspector_ShouldThrowArgumentNullException()
	{
		IEnumerable<int> subject = [1, 2, 3];

		var exception = Assert.Throws<ArgumentNullException>(() =>
			subject.Should().AllSatisfy((Action<int>)null!));

		Assert.Equal("inspector", exception.ParamName);
	}
}
