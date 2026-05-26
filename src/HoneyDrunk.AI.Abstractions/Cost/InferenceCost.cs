namespace HoneyDrunk.AI.Abstractions.Cost;

/// <summary>Represents estimated inference cost for a single operation.</summary>
/// <param name="ProviderId">The provider identifier.</param>
/// <param name="ModelId">The model identifier.</param>
/// <param name="InputTokens">Input token count.</param>
/// <param name="OutputTokens">Output token count.</param>
/// <param name="EstimatedCost">Estimated operation cost.</param>
/// <param name="OperationCorrelationId">Correlation identifier for the operation.</param>
public sealed record InferenceCost(string ProviderId, string ModelId, int InputTokens, int OutputTokens, decimal EstimatedCost, string OperationCorrelationId);
