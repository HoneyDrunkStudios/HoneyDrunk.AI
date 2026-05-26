namespace HoneyDrunk.AI.Abstractions.Chat;

/// <summary>Options for a chat completion request.</summary>
/// <param name="Temperature">Optional sampling temperature.</param>
/// <param name="MaxOutputTokens">Optional maximum output tokens.</param>
/// <param name="RequiredCapabilities">Capabilities required by the request.</param>
public sealed record ChatOptions(double? Temperature = null, int? MaxOutputTokens = null, IReadOnlyList<string>? RequiredCapabilities = null);
