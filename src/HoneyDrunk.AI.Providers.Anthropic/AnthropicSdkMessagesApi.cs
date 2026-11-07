using Anthropic;
using Anthropic.Models.Messages;
using System.Runtime.CompilerServices;

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
    public async IAsyncEnumerable<RawMessageStreamEvent> CreateStreamingAsync(MessageCreateParams request, [EnumeratorCancellation] CancellationToken ct)
    {
        var enumerator = this.client.Messages.CreateStreaming(request, ct).GetAsyncEnumerator(ct);
        await using (enumerator.ConfigureAwait(false))
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception error) when (error is not OperationCanceledException && !ReferenceEquals(SecretRedaction.Redact(error, this.apiKey), error))
                {
                    throw SecretRedaction.Redact(error, this.apiKey);
                }

                if (!moved)
                {
                    yield break;
                }

                yield return enumerator.Current;
            }
        }
    }
}
