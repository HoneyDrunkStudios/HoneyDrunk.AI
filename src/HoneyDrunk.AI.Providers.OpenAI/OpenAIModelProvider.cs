using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Embeddings;
using HoneyDrunk.AI.Abstractions.Providers;

namespace HoneyDrunk.AI.Providers.OpenAI;

/// <summary>ADR-0016 follow-up provider slot for OpenAI.</summary>
public sealed class OpenAIModelProvider : IModelProvider
{
    /// <inheritdoc />
    public string ProviderId => "openai";

    /// <inheritdoc />
    public ModelCapabilityDeclaration[] DeclaredCapabilities => [];

    /// <inheritdoc />
    public IChatClient GetChatClient(string modelId) => throw new NotImplementedException("ADR-0016 follow-up — OpenAI provider is not yet implemented.");

    /// <inheritdoc />
    public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) => throw new NotImplementedException("ADR-0016 follow-up — OpenAI provider is not yet implemented.");
}
