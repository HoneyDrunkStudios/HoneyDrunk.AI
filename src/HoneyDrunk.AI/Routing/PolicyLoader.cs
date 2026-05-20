using HoneyDrunk.AI.Abstractions;
using HoneyDrunk.Vault.Abstractions;

namespace HoneyDrunk.AI.Routing;

/// <summary>Loads the active routing policy from configuration and DI-registered policies.</summary>
public sealed class PolicyLoader
{
    private readonly IConfigProvider configProvider;
    private readonly IReadOnlyList<IRoutingPolicy> policies;

    /// <summary>Initializes a new instance of the <see cref="PolicyLoader"/> class.</summary>
    /// <param name="configProvider">The Vault configuration provider.</param>
    /// <param name="policies">Registered policies.</param>
    public PolicyLoader(IConfigProvider configProvider, IEnumerable<IRoutingPolicy> policies)
    {
        this.configProvider = configProvider;
        this.policies = policies.ToArray();
    }

    /// <summary>Loads the active policy.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active routing policy.</returns>
    public async Task<IRoutingPolicy> LoadActiveAsync(CancellationToken cancellationToken = default)
    {
        var configuredName = await this.configProvider.GetValueAsync("HoneyDrunk:AI:Routing:ActivePolicy", CostFirstRoutingPolicy.Name, cancellationToken).ConfigureAwait(false);
        return this.policies.FirstOrDefault(policy => policy.PolicyName.Equals(configuredName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No HoneyDrunk.AI routing policy named '{configuredName}' is registered.");
    }
}
