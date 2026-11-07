using Anthropic.Models.Messages;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>The two Messages API calls the provider makes. A seam so the client can be tested without the network.</summary>
public interface IAnthropicMessagesApi
{
    /// <summary>Creates a message.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The model's response.</returns>
    Task<Message> CreateAsync(MessageCreateParams request, CancellationToken ct);

    /// <summary>Creates a message and streams its events.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The stream events, in order.</returns>
    IAsyncEnumerable<RawMessageStreamEvent> CreateStreamingAsync(MessageCreateParams request, CancellationToken ct);
}
