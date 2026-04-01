using Assertivo;

"abc".Should().Contain("a");
new[] { 1, 2, 3 }.Should().HaveCount(3);
42.Should().Be(42);

try
{
	1.Should().Be(2);
	Console.WriteLine("Expected AssertionFailedException was not thrown.");
	Environment.ExitCode = 1;
}
catch (AssertionFailedException)
{
	Console.WriteLine("SC-005 validation passed: AssertionFailedException thrown as expected.");
}
