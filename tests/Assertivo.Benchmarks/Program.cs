using Assertivo;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ShouldBeBenchmarks>();

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
public class ShouldBeBenchmarks
{
	private readonly int _value = 42;
	private readonly int[] _subject = Enumerable.Range(0, 1000).ToArray();
	private readonly int[] _expected = Enumerable.Range(0, 1000).ToArray();

	[Benchmark]
	public void Should_Be_HappyPath_ZeroAllocation()
	{
		_value.Should().Be(42);
	}

	[Benchmark]
	public void Equal_HappyPath_1000Elements()
	{
		_subject.Should().Equal(_expected);
	}
}
