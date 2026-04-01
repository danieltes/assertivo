using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Assertivo;

internal static class MessageFormatter
{
    [StackTraceHidden]
    [DoesNotReturn]
    internal static void Fail(string expected, string actual, string? expression, string because, object[] becauseArgs)
    {
        string? reason = FormatReason(because, becauseArgs);
        string message = BuildMessage(expected, actual, expression, reason);
        var failure = new AssertionFailure(expected, actual, expression, reason, message);
        AssertionConfiguration.ReportFailure(failure);
        throw new InvalidOperationException("Unreachable: ReportFailure must throw");
    }

    internal static string? FormatReason(string because, object[] becauseArgs)
    {
        if (string.IsNullOrEmpty(because))
            return null;

        return becauseArgs.Length > 0
            ? string.Format(because, becauseArgs)
            : because;
    }

    internal static string BuildMessage(string expected, string actual, string? expression, string? reason)
    {
        var parts = new List<string>(4)
        {
            $"Expected {expected} but found {actual}."
        };

        if (expression is not null)
            parts.Add($"Expression: {expression}");

        if (reason is not null)
            parts.Add($"Because: {reason}");

        return string.Join("\n", parts);
    }

    internal static string FormatValue(object? value)
    {
        return value switch
        {
            null => "<null>",
            string s => $"\"{s}\"",
            _ => value.ToString() ?? "<null>"
        };
    }
}
