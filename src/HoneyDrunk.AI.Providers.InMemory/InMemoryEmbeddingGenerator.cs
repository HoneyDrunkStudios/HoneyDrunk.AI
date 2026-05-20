using HoneyDrunk.AI.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace HoneyDrunk.AI.Providers.InMemory;

/// <summary>Deterministic embedding generator for tests and evals.</summary>
public sealed class InMemoryEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly string modelId;

    /// <summary>Initializes a new instance of the <see cref="InMemoryEmbeddingGenerator"/> class.</summary>
    /// <param name="modelId">The synthetic model id.</param>
    public InMemoryEmbeddingGenerator(string modelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        this.modelId = modelId;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Embedding>> GenerateAsync(IEnumerable<string> values, EmbeddingOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(values);
        var dimensions = options?.Dimensions ?? 8;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);
        var embeddings = values.Select(value => new Embedding(value, BuildVector(value, dimensions), this.modelId)).ToArray();
        return Task.FromResult<IReadOnlyList<Embedding>>(embeddings);
    }

    private static IReadOnlyList<float> BuildVector(string value, int dimensions)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var vector = new float[dimensions];
        for (var i = 0; i < dimensions; i++)
        {
            vector[i] = (bytes[i % bytes.Length] - 128) / 128f;
        }

        return vector;
    }
}
