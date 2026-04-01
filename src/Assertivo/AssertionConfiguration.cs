using System.Diagnostics;

namespace Assertivo;

/// <summary>
/// Static configuration for assertion failure reporting.
/// </summary>
public static class AssertionConfiguration
{
    private static volatile Action<AssertionFailure> _reportFailure = DefaultReportFailure;

    /// <summary>Gets or sets the delegate invoked on assertion failure.</summary>
    public static Action<AssertionFailure> ReportFailure
    {
        get => _reportFailure;
        set => _reportFailure = value ?? throw new ArgumentNullException(nameof(value));
    }

    [StackTraceHidden]
    private static void DefaultReportFailure(AssertionFailure failure)
    {
        throw new AssertionFailedException(
            failure.Message,
            failure.Expected,
            failure.Actual,
            failure.Expression,
            failure.Reason);
    }
}
