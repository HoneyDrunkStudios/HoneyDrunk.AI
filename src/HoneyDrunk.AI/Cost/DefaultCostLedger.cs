using HoneyDrunk.AI.Abstractions;

namespace HoneyDrunk.AI.Cost;

/// <summary>In-process inference cost ledger.</summary>
public sealed class DefaultCostLedger : ICostLedger
{
    private readonly object gate = new();
    private readonly List<Entry> entries = new();

    /// <inheritdoc />
    public Task RecordAsync(InferenceCost cost, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (this.gate)
        {
            this.entries.Add(new Entry(DateTimeOffset.UtcNow, cost));
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<CostSummary> GetSummaryAsync(string scope, DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var until = DateTimeOffset.UtcNow;
        Entry[] snapshot;
        lock (this.gate)
        {
            snapshot = this.entries.ToArray();
        }

        var scoped = snapshot.Where(entry => entry.RecordedAt >= since && entry.Cost.OperationCorrelationId.StartsWith(scope, StringComparison.Ordinal)).ToArray();
        var totalCost = scoped.Sum(entry => entry.Cost.EstimatedCost);
        return Task.FromResult(new CostSummary(scope, since, until, totalCost, scoped.Length));
    }

    private sealed record Entry(DateTimeOffset RecordedAt, InferenceCost Cost);
}
