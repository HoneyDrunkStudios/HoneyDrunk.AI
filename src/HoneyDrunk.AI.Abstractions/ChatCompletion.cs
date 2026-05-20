namespace HoneyDrunk.AI.Abstractions;

/// <summary>Represents a completed chat response.</summary>
/// <param name="Message">The assistant message.</param>
/// <param name="ModelId">The model that produced the response.</param>
/// <param name="InputTokens">Estimated input tokens.</param>
/// <param name="OutputTokens">Estimated output tokens.</param>
public sealed record ChatCompletion(ChatMessage Message, string ModelId, int InputTokens = 0, int OutputTokens = 0);
