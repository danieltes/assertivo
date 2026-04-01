namespace Assertivo;

/// <summary>
/// Exception thrown when an assertion fails.
/// </summary>
public class AssertionFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionFailedException"/> class.
    /// </summary>
    public AssertionFailedException(string message, string expected, string actual, string? expression, string? reason)
        : base(message)
    {
        Expected = expected;
        Actual = actual;
        Expression = expression;
        Reason = reason;
    }

    /// <summary>Gets the expected value or condition.</summary>
    public string Expected { get; }
    /// <summary>Gets the actual value or condition.</summary>
    public string Actual { get; }
    /// <summary>Gets the captured subject expression, if available.</summary>
    public string? Expression { get; }
    /// <summary>Gets the user-supplied reason, if provided.</summary>
    public string? Reason { get; }
}
