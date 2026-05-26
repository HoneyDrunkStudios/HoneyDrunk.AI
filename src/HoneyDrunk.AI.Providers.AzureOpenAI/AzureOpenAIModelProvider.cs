using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Embeddings;
using HoneyDrunk.AI.Abstractions.Providers;

namespace HoneyDrunk.AI.Providers.AzureOpenAI;

/// <summary>ADR-0016 follow-up provider slot for AzureOpenAI.</summary>
public sealed class AzureOpenAIModelProvider : IModelProvider
{
    /// <inheritdoc />
    public string ProviderId => "azureopenai";

    /// <inheritdoc />
    public ModelCapabilityDeclaration[] DeclaredCapabilities => [];

    /// <inheritdoc />
    public IChatClient GetChatClient(string modelId) => throw new NotImplementedException("ADR-0016 follow-up — AzureOpenAI provider is not yet implemented.");

    /// <inheritdoc />
    public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) => throw new NotImplementedException("ADR-0016 follow-up — AzureOpenAI provider is not yet implemented.");
}
