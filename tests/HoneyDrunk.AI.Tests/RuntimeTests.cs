using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Cost;
using HoneyDrunk.AI.Abstractions.Embeddings;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.AI.Cost;
using HoneyDrunk.AI.Routing;
using HoneyDrunk.Vault.Abstractions;
using Microsoft.Extensions.Options;
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
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 2, 0.50m, "scope2:a"));

        var summary = await ledger.GetSummaryAsync("scope", since);
        Assert.Equal(0.40m, summary.TotalCost);
        Assert.Equal(2, summary.TotalCalls);
    }

    /// <summary>Prunes old entries when the bounded ledger reaches capacity.</summary>
    [Fact]
    public async Task DefaultCostLedger_prunes_oldest_entries()
    {
        var ledger = new DefaultCostLedger(maxEntries: 2);
        var since = DateTimeOffset.UtcNow.AddMinutes(-1);
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 1, 1m, "scope:first"));
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 1, 2m, "scope:second"));
        await ledger.RecordAsync(new InferenceCost("p", "m", 1, 1, 3m, "scope:third"));

        var summary = await ledger.GetSummaryAsync("scope", since);
        Assert.Equal(5m, summary.TotalCost);
        Assert.Equal(2, summary.TotalCalls);
    }

    /// <summary>Uses AIOptions as the Vault configuration fallback for policy loading.</summary>
    [Fact]
    public async Task PolicyLoader_uses_options_default_policy_name()
    {
        var expected = new CostFirstRoutingPolicy();
        var loader = new PolicyLoader(new DefaultOnlyConfigProvider(), [expected], Options.Create(new AIOptions { DefaultPolicyName = CostFirstRoutingPolicy.Name }));

        var policy = await loader.LoadActiveAsync();
        Assert.Same(expected, policy);
    }

    private sealed class DefaultOnlyConfigProvider : IConfigProvider
    {
        public Task<string> GetValueAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);

        public Task<T> GetValueAsync<T>(string path, T defaultValue, CancellationToken cancellationToken = default) => Task.FromResult(defaultValue);

        public Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult<T>(default!);

        public Task<string?> TryGetValueAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }

    private sealed class TestProvider(string providerId, decimal cost) : IModelProvider
    {
        private readonly decimal cost = cost;

        public string ProviderId { get; } = providerId;

        public ModelCapabilityDeclaration[] DeclaredCapabilities => [new(this.ProviderId, $"{this.ProviderId}:model", 4096, false, false, false, ["local"], this.cost, 0m)];

        public IChatClient GetChatClient(string modelId) => throw new NotSupportedException();

        public IEmbeddingGenerator GetEmbeddingGenerator(string modelId) => throw new NotSupportedException();
    }
}
