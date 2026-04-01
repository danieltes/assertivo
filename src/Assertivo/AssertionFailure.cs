namespace Assertivo;

/// <summary>
/// Value type encapsulating structured assertion failure data.
/// </summary>
public readonly struct AssertionFailure
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionFailure"/> struct.
    /// </summary>
    public AssertionFailure(string expected, string actual, string? expression, string? reason, string message)
    {
        Expected = expected;
        Actual = actual;
        Expression = expression;
        Reason = reason;
        Message = message;
    }

    /// <summary>Gets the expected value or condition.</summary>
    public string Expected { get; }
    /// <summary>Gets the actual value or condition.</summary>
    public string Actual { get; }
    /// <summary>Gets the captured subject expression, if available.</summary>
    public string? Expression { get; }
    /// <summary>Gets the user-supplied reason, if provided.</summary>
    public string? Reason { get; }
    /// <summary>Gets the pre-formatted human-readable failure message.</summary>
    public string Message { get; }
}
