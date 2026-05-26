namespace HoneyDrunk.AI.Abstractions.Chat;

/// <summary>Completes chat requests against a model.</summary>
public interface IChatClient
{
    /// <summary>Produces a complete chat response.</summary>
    /// <param name="messages">The ordered conversation messages.</param>
    /// <param name="options">Optional chat options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat completion.</returns>
    Task<ChatCompletion> CompleteAsync(IReadOnlyList<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Produces streaming chat response updates.</summary>
    /// <param name="messages">The ordered conversation messages.</param>
    /// <param name="options">Optional chat options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An asynchronous stream of completion updates.</returns>
    IAsyncEnumerable<ChatCompletionUpdate> CompleteStreamingAsync(IReadOnlyList<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default);
}
