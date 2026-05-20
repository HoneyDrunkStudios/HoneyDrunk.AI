namespace HoneyDrunk.AI.Abstractions;

/// <summary>Chooses one model candidate for a request.</summary>
public interface IRoutingPolicy
{
    /// <summary>Gets the policy name.</summary>
    string PolicyName { get; }

    /// <summary>Chooses one model from candidates.</summary>
    /// <param name="candidates">Candidate models.</param>
    /// <param name="request">The request summary.</param>
    /// <returns>The routing decision.</returns>
    RoutingDecision Choose(IReadOnlyList<ModelCandidate> candidates, ChatRequestSummary request);
}
