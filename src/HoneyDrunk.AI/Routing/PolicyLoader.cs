using HoneyDrunk.AI.Abstractions;
using HoneyDrunk.Vault.Abstractions;
using Microsoft.Extensions.Options;

namespace HoneyDrunk.AI.Routing;

/// <summary>Loads the active routing policy from configuration and DI-registered policies.</summary>
public sealed class PolicyLoader
{
    private readonly IConfigProvider configProvider;
    private readonly IReadOnlyList<IRoutingPolicy> policies;
    private readonly IOptions<AIOptions> options;

    /// <summary>Initializes a new instance of the <see cref="PolicyLoader"/> class.</summary>
    /// <param name="configProvider">The Vault configuration provider.</param>
    /// <param name="policies">Registered policies.</param>
    /// <param name="options">Runtime options used as configuration defaults.</param>
    public PolicyLoader(IConfigProvider configProvider, IEnumerable<IRoutingPolicy> policies, IOptions<AIOptions> options)
    {
        this.configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        this.policies = (policies ?? throw new ArgumentNullException(nameof(policies))).ToArray();
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>Loads the active policy.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active routing policy.</returns>
    public async Task<IRoutingPolicy> LoadActiveAsync(CancellationToken cancellationToken = default)
    {
        var defaultPolicyName = this.options.Value.DefaultPolicyName;
        var configuredName = await this.configProvider.GetValueAsync("HoneyDrunk:AI:Routing:ActivePolicy", defaultPolicyName, cancellationToken).ConfigureAwait(false);
        return this.policies.FirstOrDefault(policy => policy.PolicyName.Equals(configuredName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No HoneyDrunk.AI routing policy named '{configuredName}' is registered.");
    }
}
