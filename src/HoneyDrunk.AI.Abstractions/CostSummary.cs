namespace HoneyDrunk.AI.Abstractions;

/// <summary>Summarizes recorded cost for a scope.</summary>
/// <param name="Scope">Caller-defined scope.</param>
/// <param name="Since">Summary lower bound.</param>
/// <param name="Until">Summary upper bound.</param>
/// <param name="TotalCost">Total estimated cost.</param>
/// <param name="TotalCalls">Total recorded calls.</param>
public sealed record CostSummary(string Scope, DateTimeOffset Since, DateTimeOffset Until, decimal TotalCost, int TotalCalls);
