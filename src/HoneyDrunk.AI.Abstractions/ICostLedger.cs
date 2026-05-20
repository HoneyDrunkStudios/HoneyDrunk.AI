namespace HoneyDrunk.AI.Abstractions;

/// <summary>Records and summarizes inference cost.</summary>
public interface ICostLedger
{
    /// <summary>Records an inference cost entry.</summary>
    /// <param name="cost">The cost entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing completion.</returns>
    Task RecordAsync(InferenceCost cost, CancellationToken cancellationToken = default);

    /// <summary>Gets a summary for a caller-defined scope.</summary>
    /// <param name="scope">The scope to summarize.</param>
    /// <param name="since">The lower time bound.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cost summary.</returns>
    Task<CostSummary> GetSummaryAsync(string scope, DateTimeOffset since, CancellationToken cancellationToken = default);
}
