using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Routing;

namespace HoneyDrunk.AI.Routing;

/// <summary>Chooses the lowest-cost eligible candidate.</summary>
public sealed class CostFirstRoutingPolicy : IRoutingPolicy
{
    /// <summary>The canonical policy name.</summary>
    public const string Name = "cost-first";

    /// <inheritdoc />
    public string PolicyName => Name;

    /// <inheritdoc />
    public RoutingDecision Choose(IReadOnlyList<ModelCandidate> candidates, ChatRequestSummary request)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("Cost-first routing policy received no candidates.");
        }

        var selected = candidates.OrderBy(candidate => candidate.CostPerKToken).ThenBy(candidate => candidate.ProviderId, StringComparer.Ordinal).ThenBy(candidate => candidate.ModelId, StringComparer.Ordinal).First();
        return new RoutingDecision(selected, "Selected lowest blended cost candidate.");
    }
}
