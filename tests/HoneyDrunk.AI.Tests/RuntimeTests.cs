using HoneyDrunk.AI.Abstractions;
using HoneyDrunk.AI.Cost;
using HoneyDrunk.AI.Routing;
using Xunit;

namespace HoneyDrunk.AI.Tests;

/// <summary>Runtime unit tests.</summary>
public sealed class RuntimeTests
{
    /// <summary>Routes to the lowest-cost provider among two candidates.</summary>
    [Fact]
    public async Task DefaultModelRouter_routes_with_cost_first_policy()
    {
        var providers = new IModelProvider[] { new TestProvider("expensive", 10m), new TestProvider("cheap", 1m) };
        var router = new DefaultModelRouter(providers);
        var routed = await router.RouteAsync(new ChatRequestSummary(100, 100, []), new CostFirstRoutingPolicy());
        Assert.Equal("cheap", routed.ProviderId);
    }

    /// <summary>Accumulates scoped costs.</summary>
    [Fact]
    public async Task DefaultCostLedger_accumulates_scope()
    {
        var ledger = new DefaultCostLedger();
        var since = DateTimeOffset.UtcNow.AddMinutes(-1);
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 2, 0.10m, "scope:a"));
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 2, 0.20m, "other:a"));
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 2, 0.30m, "scope:b"));

        var summary = await ledger.GetSummaryAsync("scope", since);
        Assert.Equal(0.40m, summary.TotalCost);
        Assert.Equal(2, summary.TotalCalls);
    }

    private sealed class TestProvider : IModelProvider
    {
        private readonly decimal cost;

        public TestProvider(string providerId, decimal cost)
        {
            this.ProviderId = providerId;
            this.cost = cost;
        }

        public string ProviderId { get; }

        public ModelCapabilityDeclaration[] DeclaredCapabilities => [new(this.ProviderId, $"{this.ProviderId}:model", 4096, false, false, false, ["local"], this.cost, 0m)];

        public IChatClient GetChatClient(string modelId) => throw new NotSupportedException();

        public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) => throw new NotSupportedException();
    }
}
