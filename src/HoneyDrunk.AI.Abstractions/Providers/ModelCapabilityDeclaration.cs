namespace HoneyDrunk.AI.Abstractions.Providers;

/// <summary>Declares a model's provider-owned capabilities and cost hints.</summary>
/// <param name="ProviderId">The provider identifier.</param>
/// <param name="ModelId">The model identifier.</param>
/// <param name="MaxContextTokens">Maximum supported context tokens.</param>
/// <param name="SupportsStreaming">Whether streaming chat is supported.</param>
/// <param name="SupportsVision">Whether vision inputs are supported.</param>
/// <param name="SupportsFunctionCalling">Whether function/tool calling is supported.</param>
/// <param name="SupportedRegions">Regions where the model may run.</param>
/// <param name="InputCostPerKToken">Optional input token cost per thousand tokens.</param>
/// <param name="OutputCostPerKToken">Optional output token cost per thousand tokens.</param>
public sealed record ModelCapabilityDeclaration(
    string ProviderId,
    string ModelId,
    int MaxContextTokens,
    bool SupportsStreaming,
    bool SupportsVision,
    bool SupportsFunctionCalling,
    string[] SupportedRegions,
    decimal? InputCostPerKToken,
    decimal? OutputCostPerKToken);
