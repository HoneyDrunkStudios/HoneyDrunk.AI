namespace HoneyDrunk.AI.Abstractions;

/// <summary>Exposes model clients and capability declarations for one provider.</summary>
public interface IModelProvider
{
    /// <summary>Gets the stable provider identifier.</summary>
    string ProviderId { get; }

    /// <summary>Gets all capabilities declared by the provider.</summary>
    ModelCapabilityDeclaration[] DeclaredCapabilities { get; }

    /// <summary>Gets a chat client for a model.</summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>The chat client.</returns>
    IChatClient GetChatClient(string modelId);

    /// <summary>Gets an embedding generator for a model.</summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>The embedding generator.</returns>
    IEmbeddingGenerator GetEmbeddingGenerator(string modelId);
}
