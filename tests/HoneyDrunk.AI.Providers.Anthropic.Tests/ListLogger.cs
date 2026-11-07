using Microsoft.Extensions.Logging;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Captures log entries for assertions.</summary>
internal sealed class ListLogger : ILogger
{
    public List<(LogLevel level, string message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        this.Entries.Add((logLevel, formatter(state, exception)));
}
