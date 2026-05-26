using HoneyDrunk.AI.Abstractions.Providers;

namespace HoneyDrunk.AI.Abstractions.Routing;

/// <summary>Represents a candidate model for policy evaluation.</summary>
/// <param name="ProviderId">The candidate provider.</param>
/// <param name="ModelId">The candidate model.</param>
/// <param name="Capability">The candidate capability declaration.</param>
/// <param name="CostPerKToken">Estimated blended cost per thousand tokens.</param>
public sealed record ModelCandidate(string ProviderId, string ModelId, ModelCapabilityDeclaration Capability, decimal CostPerKToken);
