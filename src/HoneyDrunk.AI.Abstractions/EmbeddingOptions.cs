namespace HoneyDrunk.AI.Abstractions;

/// <summary>Options for embedding generation.</summary>
/// <param name="Dimensions">Optional embedding dimensions.</param>
public sealed record EmbeddingOptions(int? Dimensions = null);
