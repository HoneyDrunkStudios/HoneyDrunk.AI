namespace HoneyDrunk.AI.Abstractions;

/// <summary>Represents the routed provider/model selection.</summary>
/// <param name="ProviderId">The selected provider.</param>
/// <param name="ModelId">The selected model.</param>
/// <param name="Capability">The selected capability declaration.</param>
public sealed record RoutedModel(string ProviderId, string ModelId, ModelCapabilityDeclaration Capability);
