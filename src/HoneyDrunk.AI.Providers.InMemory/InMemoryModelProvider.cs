using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Embeddings;
using HoneyDrunk.AI.Abstractions.Providers;

namespace HoneyDrunk.AI.Providers.InMemory;

/// <summary>Deterministic local model provider for tests, evals, and canary projects.</summary>
/// <remarks>Initializes a new instance of the <see cref="InMemoryModelProvider"/> class.</remarks>
/// <param name="scriptedResponses">Scripted completions keyed by request fingerprint.</param>
public sealed class InMemoryModelProvider(IReadOnlyDictionary<string, ChatCompletion> scriptedResponses) : IModelProvider
{
    private readonly IReadOnlyDictionary<string, ChatCompletion> scriptedResponses = scriptedResponses ?? throw new ArgumentNullException(nameof(scriptedResponses));

    /// <summary>Initializes a new instance of the <see cref="InMemoryModelProvider"/> class.</summary>
    public InMemoryModelProvider()
        : this(new Dictionary<string, ChatCompletion>())
    {
    }

    /// <inheritdoc />
    public string ProviderId => "inmemory";

    /// <inheritdoc />
    public ModelCapabilityDeclaration[] DeclaredCapabilities => InMemoryCapabilities.All;

    /// <inheritdoc />
    public IChatClient GetChatClient(string modelId) => new InMemoryChatClient(modelId, this.scriptedResponses);

    /// <inheritdoc />
    public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) => new InMemoryEmbeddingGenerator(modelId);
}
