using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.AI.Abstractions.Routing;

namespace HoneyDrunk.AI.Routing;

/// <summary>Routes requests across declared model provider capabilities.</summary>
/// <remarks>Initializes a new instance of the <see cref="DefaultModelRouter"/> class.</remarks>
/// <param name="providers">Registered model providers.</param>
public sealed class DefaultModelRouter(IEnumerable<IModelProvider> providers) : IModelRouter
{
    private readonly IReadOnlyList<IModelProvider> providers = [.. providers ?? throw new ArgumentNullException(nameof(providers))];

    /// <inheritdoc />
    public Task<RoutedModel> RouteAsync(ChatRequestSummary request, IRoutingPolicy policy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(policy);
        cancellationToken.ThrowIfCancellationRequested();

        var candidates = this.providers
            .SelectMany(provider => provider.DeclaredCapabilities.Select(capability => new ModelCandidate(
                provider.ProviderId,
                capability.ModelId,
                capability,
                BlendedCost(capability))))
            .Where(candidate => candidate.Capability.MaxContextTokens >= request.EstimatedInputTokens + request.MaxOutputTokens)
            .Where(candidate => HasRequiredCapabilities(candidate.Capability, request.RequiredCapabilities))
            .ToArray();

        if (candidates.Length == 0)
        {
            throw new InvalidOperationException("No HoneyDrunk.AI model candidates satisfy the request.");
        }

        var decision = policy.Choose(candidates, request);
        return Task.FromResult(new RoutedModel(decision.Selected.ProviderId, decision.Selected.ModelId, decision.Selected.Capability));
    }

    private static decimal BlendedCost(ModelCapabilityDeclaration capability)
        => (capability.InputCostPerKToken ?? 0m) + (capability.OutputCostPerKToken ?? 0m);

    private static bool HasRequiredCapabilities(ModelCapabilityDeclaration capability, IReadOnlyList<string> requiredCapabilities)
    {
        foreach (var required in requiredCapabilities)
        {
            if (required.Equals("streaming", StringComparison.OrdinalIgnoreCase) && !capability.SupportsStreaming) return false;
            if (required.Equals("vision", StringComparison.OrdinalIgnoreCase) && !capability.SupportsVision) return false;
            if ((required.Equals("function-calling", StringComparison.OrdinalIgnoreCase) || required.Equals("tools", StringComparison.OrdinalIgnoreCase)) && !capability.SupportsFunctionCalling) return false;
        }

        return true;
    }
}
