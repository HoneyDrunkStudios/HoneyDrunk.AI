using HoneyDrunk.AI.Abstractions;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>ADR-0016 follow-up provider slot for Anthropic.</summary>
public sealed class AnthropicModelProvider : IModelProvider
{
    /// <inheritdoc />
    public string ProviderId => "anthropic";

    /// <inheritdoc />
    public ModelCapabilityDeclaration[] DeclaredCapabilities => [];

    /// <inheritdoc />
    public IChatClient GetChatClient(string modelId) => throw new NotImplementedException("ADR-0016 follow-up — Anthropic provider is not yet implemented.");

    /// <inheritdoc />
    public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) => throw new NotImplementedException("ADR-0016 follow-up — Anthropic provider is not yet implemented.");
}
