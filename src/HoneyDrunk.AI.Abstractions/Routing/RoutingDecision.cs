namespace HoneyDrunk.AI.Abstractions.Routing;

/// <summary>Represents a routing policy decision.</summary>
/// <param name="Selected">The selected model candidate.</param>
/// <param name="Reason">Human-readable decision reason.</param>
public sealed record RoutingDecision(ModelCandidate Selected, string Reason);
