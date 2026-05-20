namespace HoneyDrunk.AI.Abstractions;

/// <summary>Routes requests to declared models using a supplied policy.</summary>
public interface IModelRouter
{
    /// <summary>Routes a request to a model.</summary>
    /// <param name="request">The request summary.</param>
    /// <param name="policy">The routing policy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The routed model.</returns>
    Task<RoutedModel> RouteAsync(ChatRequestSummary request, IRoutingPolicy policy, CancellationToken cancellationToken = default);
}
