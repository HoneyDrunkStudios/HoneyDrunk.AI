namespace HoneyDrunk.AI.Abstractions.Embeddings;

/// <summary>Generates embeddings for text values.</summary>
public interface IEmbeddingGenerator
{
    /// <summary>Generates deterministic or provider-backed embeddings for values.</summary>
    /// <param name="values">The input text values.</param>
    /// <param name="options">Optional embedding options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated embeddings.</returns>
    Task<IReadOnlyList<Embedding>> GenerateAsync(IEnumerable<string> values, EmbeddingOptions? options = null, CancellationToken cancellationToken = default);
}
