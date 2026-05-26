namespace HoneyDrunk.AI.Abstractions.Embeddings;

/// <summary>Represents an embedding vector for a source value.</summary>
/// <param name="Value">The source value.</param>
/// <param name="Vector">The embedding vector.</param>
/// <param name="ModelId">The model that generated the embedding.</param>
public sealed record Embedding(string Value, IReadOnlyList<float> Vector, string ModelId);
