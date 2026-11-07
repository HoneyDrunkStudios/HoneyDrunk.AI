using Anthropic;
using Anthropic.Models.Messages;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>The real Messages API, through the official SDK. Errors that mention the key are redacted.</summary>
/// <param name="client">The SDK client.</param>
/// <param name="apiKey">The key, used only to recognize it in error messages.</param>
internal sealed class AnthropicSdkMessagesApi(AnthropicClient client, string apiKey) : IAnthropicMessagesApi
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
        catch (Exception error) when (error is not OperationCanceledException && !ReferenceEquals(SecretRedaction.Redact(error, this.apiKey), error))
        {
            throw SecretRedaction.Redact(error, this.apiKey);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RawMessageStreamEvent> CreateStreamingAsync(MessageCreateParams request, CancellationToken ct) =>
        SecretRedaction.RedactStream(() => this.client.Messages.CreateStreaming(request, ct), this.apiKey, ct);
}
