namespace HoneyDrunk.AI.Abstractions.Embeddings;

/// <summary>Options for embedding generation.</summary>
/// <param name="Dimensions">Optional embedding dimensions.</param>
public sealed record EmbeddingOptions(int? Dimensions = null);
