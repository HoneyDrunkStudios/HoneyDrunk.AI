using HoneyDrunk.AI.Abstractions.Cost;

namespace HoneyDrunk.AI.Cost;

/// <summary>In-process inference cost ledger.</summary>
public sealed class DefaultCostLedger : ICostLedger
{
    private const int DefaultMaxEntries = 10_000;
    private readonly object gate = new();
    private readonly List<Entry> entries = [];
    private readonly int maxEntries;

    /// <summary>Initializes a new instance of the <see cref="DefaultCostLedger"/> class.</summary>
    /// <param name="maxEntries">Maximum retained entries before oldest entries are pruned.</param>
    public DefaultCostLedger(int maxEntries = DefaultMaxEntries)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEntries);
        this.maxEntries = maxEntries;
    }

    /// <inheritdoc />
    public Task RecordAsync(InferenceCost cost, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(cost);
        lock (this.gate)
        {
            this.entries.Add(new Entry(DateTimeOffset.UtcNow, cost));
            if (this.entries.Count > this.maxEntries)
            {
                this.entries.RemoveRange(0, this.entries.Count - this.maxEntries);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<CostSummary> GetSummaryAsync(string scope, DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        var until = DateTimeOffset.UtcNow;
        Entry[] snapshot;
        lock (this.gate)
        {
            snapshot = [.. this.entries];
        }

        var scoped = snapshot.Where(entry => entry.RecordedAt >= since && IsInScope(entry.Cost.OperationCorrelationId, scope)).ToArray();
        var totalCost = scoped.Sum(entry => entry.Cost.EstimatedCost);
        return Task.FromResult(new CostSummary(scope, since, until, totalCost, scoped.Length));
    }

    private static bool IsInScope(string operationCorrelationId, string scope)
        => string.Equals(operationCorrelationId, scope, StringComparison.Ordinal)
        || operationCorrelationId.StartsWith(scope + ":", StringComparison.Ordinal);

    private sealed record Entry(DateTimeOffset RecordedAt, InferenceCost Cost);
}
