using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Embeddings;
using HoneyDrunk.AI.Abstractions.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Anthropic model provider. Models, capabilities and cost rates come from options.</summary>
/// <param name="options">The provider options.</param>
/// <param name="api">The Messages API seam.</param>
/// <param name="loggerFactory">The logger factory.</param>
public sealed class AnthropicModelProvider(IOptions<AnthropicProviderOptions> options, IAnthropicMessagesApi api, ILoggerFactory loggerFactory) : IModelProvider
{
    /// <summary>The provider identifier.</summary>
    public const string Id = "anthropic";

    private readonly AnthropicProviderOptions options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    private readonly IAnthropicMessagesApi api = api ?? throw new ArgumentNullException(nameof(api));
    private readonly ILoggerFactory loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

    /// <inheritdoc />
    public string ProviderId => Id;

    /// <inheritdoc />
    public ModelCapabilityDeclaration[] DeclaredCapabilities => [.. this.options.Models.Select(ToDeclaration)];

    /// <inheritdoc />
    public IChatClient GetChatClient(string modelId)
    {
        var model = this.FindModel(modelId);
        return new AnthropicChatClient(model.ModelId, model, this.api, this.loggerFactory.CreateLogger<AnthropicModelProvider>());
    }

    /// <inheritdoc />
    public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) =>
        throw new NotSupportedException("Anthropic does not provide embeddings; route embeddings to another provider.");

    private static ModelCapabilityDeclaration ToDeclaration(AnthropicModelOptions model) => new(
        Id,
        model.ModelId,
        model.MaxContextTokens,
        SupportsStreaming: true,
        model.SupportsVision,
        model.SupportsFunctionCalling,
        model.SupportedRegions,
        model.InputCostPerKToken,
        model.OutputCostPerKToken);

    private AnthropicModelOptions FindModel(string modelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        return this.options.Models.FirstOrDefault(model => string.Equals(model.ModelId, modelId, StringComparison.Ordinal))
            ?? throw new ArgumentException(
                $"Model '{modelId}' is not configured for the Anthropic provider. Configured: {string.Join(", ", this.options.Models.Select(model => model.ModelId))}.",
                nameof(modelId));
    }
}
