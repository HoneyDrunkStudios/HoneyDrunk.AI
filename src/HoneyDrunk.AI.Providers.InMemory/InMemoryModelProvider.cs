using HoneyDrunk.AI.Abstractions;

namespace HoneyDrunk.AI.Providers.InMemory;

/// <summary>Deterministic local model provider for tests, evals, and canary projects.</summary>
public sealed class InMemoryModelProvider : IModelProvider
{
    private readonly IReadOnlyDictionary<string, ChatCompletion> scriptedResponses;

    /// <summary>Initializes a new instance of the <see cref="InMemoryModelProvider"/> class.</summary>
    public InMemoryModelProvider()
        : this(new Dictionary<string, ChatCompletion>())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InMemoryModelProvider"/> class.</summary>
    /// <param name="scriptedResponses">Scripted completions keyed by request fingerprint.</param>
    public InMemoryModelProvider(IReadOnlyDictionary<string, ChatCompletion> scriptedResponses)
    {
        this.scriptedResponses = scriptedResponses;
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
