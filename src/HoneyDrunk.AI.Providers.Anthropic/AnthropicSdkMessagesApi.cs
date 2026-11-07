using Anthropic;
using Anthropic.Models.Messages;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>
/// The real Messages API, through the official SDK. Owns its client. Errors pass through
/// <see cref="SecretRedaction.Translate"/>: key-bearing errors are replaced and SDK timeouts are not reported as cancellation.
/// </summary>
/// <param name="client">The SDK client; disposed with this instance.</param>
/// <param name="apiKey">The key, used only to recognize it in error messages.</param>
internal sealed class AnthropicSdkMessagesApi(AnthropicClient client, string apiKey) : IAnthropicMessagesApi, IDisposable
{
    private readonly AnthropicClient client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly string apiKey = apiKey ?? string.Empty;

    /// <inheritdoc />
    public async Task<Message> CreateAsync(MessageCreateParams request, CancellationToken ct)
    {
        try
        {
            return await this.client.Messages.Create(request, ct).ConfigureAwait(false);
        }
        catch (Exception error) when (SecretRedaction.Translate(error, this.apiKey, ct) is { } replacement)
        {
            throw replacement;
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RawMessageStreamEvent> CreateStreamingAsync(MessageCreateParams request, CancellationToken ct) =>
        SecretRedaction.RedactStream(() => this.client.Messages.CreateStreaming(request, ct), this.apiKey, ct);

    /// <inheritdoc />
    public void Dispose() => this.client.Dispose();
}
