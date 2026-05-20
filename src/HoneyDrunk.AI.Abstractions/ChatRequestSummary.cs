namespace HoneyDrunk.AI.Abstractions;

/// <summary>Summarizes a chat request for model routing.</summary>
/// <param name="EstimatedInputTokens">Estimated input tokens.</param>
/// <param name="MaxOutputTokens">Requested maximum output tokens.</param>
/// <param name="RequiredCapabilities">Capabilities required by the caller.</param>
public sealed record ChatRequestSummary(int EstimatedInputTokens, int MaxOutputTokens, IReadOnlyList<string> RequiredCapabilities);
