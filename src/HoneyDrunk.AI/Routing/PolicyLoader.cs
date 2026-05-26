using HoneyDrunk.AI.Abstractions.Routing;
using HoneyDrunk.Vault.Abstractions;
using Microsoft.Extensions.Options;

namespace HoneyDrunk.AI.Routing;

/// <summary>Loads the active routing policy from configuration and DI-registered policies.</summary>
/// <remarks>Initializes a new instance of the <see cref="PolicyLoader"/> class.</remarks>
/// <param name="configProvider">The Vault configuration provider.</param>
/// <param name="policies">Registered policies.</param>
/// <param name="options">Runtime options used as configuration defaults.</param>
public sealed class PolicyLoader(IConfigProvider configProvider, IEnumerable<IRoutingPolicy> policies, IOptions<AIOptions> options)
{
    private readonly IConfigProvider configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
    private readonly IReadOnlyList<IRoutingPolicy> policies = [.. policies ?? throw new ArgumentNullException(nameof(policies))];
    private readonly IOptions<AIOptions> options = options ?? throw new ArgumentNullException(nameof(options));

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
