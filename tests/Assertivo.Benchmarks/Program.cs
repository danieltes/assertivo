using Assertivo;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ShouldBeBenchmarks>();

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
public class ShouldBeBenchmarks
{
	private readonly int _value = 42;

	[Benchmark]
	public void Should_Be_HappyPath_ZeroAllocation()
	{
		_value.Should().Be(42);
	}
}
