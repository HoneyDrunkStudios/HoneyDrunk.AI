using HoneyDrunk.AI.Abstractions.Cost;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.AI.Abstractions.Routing;
using Xunit;

namespace HoneyDrunk.AI.Abstractions.Tests;

/// <summary>Compile-only smoke tests for public contracts.</summary>
public sealed class AbstractionsSmokeTests
{
    /// <summary>Constructs the D3 contract records.</summary>
    [Fact]
    public void Contracts_are_constructible()
    {
        var capability = new ModelCapabilityDeclaration("provider", "model", 1024, true, false, false, ["local"], 1m, 2m);
        var candidate = new ModelCandidate("provider", "model", capability, 3m);
        var decision = new RoutingDecision(candidate, "because");
        var routed = new RoutedModel(decision.Selected.ProviderId, decision.Selected.ModelId, capability);
        var cost = new InferenceCost("provider", "model", 10, 20, 0.1m, "scope:op");
        var summary = new CostSummary("scope", DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, cost.EstimatedCost, 1);

        Assert.Equal("model", routed.ModelId);
        Assert.Equal("scope", summary.Scope);
    }
}
