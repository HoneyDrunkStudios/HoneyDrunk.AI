using HoneyDrunk.AI.Routing;

namespace HoneyDrunk.AI;

/// <summary>Options for HoneyDrunk.AI runtime registration.</summary>
public sealed class AIOptions
{
    /// <summary>Gets or sets the default routing policy name.</summary>
    public string DefaultPolicyName { get; set; } = CostFirstRoutingPolicy.Name;
}
