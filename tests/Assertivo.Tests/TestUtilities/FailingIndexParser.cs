using System.Text.RegularExpressions;

namespace Assertivo.Tests.TestUtilities;

internal static class FailingIndexParser
{
    private static readonly Regex FailingIndicesRegex = new("Failing indices:\\s*\\[(?<content>[^\\]]*)\\]", RegexOptions.Compiled);

    internal static IReadOnlyList<int> Parse(string message)
    {
        var match = FailingIndicesRegex.Match(message);
        if (!match.Success)
        {
            return [];
        }

        var content = match.Groups["content"].Value;
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var values = new List<int>();
        foreach (var rawSegment in content.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (rawSegment.Contains('-', StringComparison.Ordinal))
            {
                var bounds = rawSegment.Split('-', StringSplitOptions.TrimEntries);
                if (bounds.Length != 2)
                {
                    continue;
                }

                var start = int.Parse(bounds[0]);
                var end = int.Parse(bounds[1]);
                for (var i = start; i <= end; i++)
                {
                    values.Add(i);
                }
            }
            else
            {
                values.Add(int.Parse(rawSegment));
            }
        }

        return values;
    }
}
