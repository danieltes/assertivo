namespace Assertivo.Collections;

internal readonly record struct AllSatisfyElementFailure(int Index, Exception Error);

internal static class AllSatisfyFailureFormatter
{
	private const int DetailLimit = 50;
	private const int ExplicitIndexLimit = 100;

	internal static string BuildActual(IReadOnlyList<AllSatisfyElementFailure> failures)
	{
		var details = BuildDetails(failures);
		var indices = BuildFailingIndices(failures.Select(f => f.Index).ToList());

		return string.Join(
			"\n",
			$"{failures.Count} element(s) failed.",
			$"Details: {details}",
			$"Failing indices: {indices}");
	}

	internal static string BuildDetails(IReadOnlyList<AllSatisfyElementFailure> failures)
	{
		var rendered = failures
			.Take(DetailLimit)
			.Select(FormatDetail)
			.ToList();

		var details = string.Join("; ", rendered);
		if (failures.Count > DetailLimit)
		{
			details += $" (showing first {DetailLimit} failure(s))";
		}

		return details;
	}

	internal static string BuildFailingIndices(IReadOnlyList<int> indices)
	{
		if (indices.Count <= ExplicitIndexLimit)
		{
			return $"[{string.Join(", ", indices)}]";
		}

		var segments = new List<string>();
		var start = indices[0];
		var previous = indices[0];

		for (var i = 1; i < indices.Count; i++)
		{
			var current = indices[i];
			if (current == previous + 1)
			{
				previous = current;
				continue;
			}

			segments.Add(RenderSegment(start, previous));
			start = current;
			previous = current;
		}

		segments.Add(RenderSegment(start, previous));
		return $"[{string.Join(", ", segments)}]";
	}

	private static string FormatDetail(AllSatisfyElementFailure failure)
	{
		string renderedMessage = failure.Error switch
		{
			AssertionFailedException assertionFailure => Normalize(assertionFailure.Message),
			_ => $"{failure.Error.GetType().Name}: {Normalize(failure.Error.Message)}"
		};

		return $"[{failure.Index}]: {renderedMessage}";
	}

	private static string RenderSegment(int start, int end)
	{
		return start == end ? start.ToString() : $"{start}-{end}";
	}

	private static string Normalize(string message)
	{
		return message
			.Replace("\r\n", " ", StringComparison.Ordinal)
			.Replace('\n', ' ')
			.Trim();
	}
}
