namespace HoneyDrunk.AI.Abstractions.Chat;

/// <summary>Represents an incremental streaming chat update.</summary>
/// <param name="Delta">The streamed text delta.</param>
/// <param name="IsComplete">Whether the stream is complete.</param>
/// <param name="ModelId">The model that produced the update.</param>
public sealed record ChatCompletionUpdate(string Delta, bool IsComplete, string ModelId);
