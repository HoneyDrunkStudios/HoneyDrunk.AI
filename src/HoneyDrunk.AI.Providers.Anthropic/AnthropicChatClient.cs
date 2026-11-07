using HoneyDrunk.AI.Abstractions.Chat;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Chat client for one configured Anthropic model.</summary>
/// <param name="modelId">The configured model identifier.</param>
/// <param name="model">The configured model options.</param>
/// <param name="api">The Messages API seam.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class AnthropicChatClient(string modelId, AnthropicModelOptions model, IAnthropicMessagesApi api, ILogger logger) : IChatClient
{
    private const string RefusalStopReason = "refusal";

    private readonly string modelId = string.IsNullOrWhiteSpace(modelId) ? throw new ArgumentException("A model ID is required.", nameof(modelId)) : modelId;
    private readonly AnthropicModelOptions model = model ?? throw new ArgumentNullException(nameof(model));
    private readonly IAnthropicMessagesApi api = api ?? throw new ArgumentNullException(nameof(api));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<ChatCompletion> CompleteAsync(IReadOnlyList<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var request = AnthropicRequestMapper.ToCreateParams(this.modelId, messages, options, this.model);
        var response = await this.api.CreateAsync(request, cancellationToken).ConfigureAwait(false);
        var (completion, truncated) = AnthropicResponseMapper.ToCompletion(response);
        if (truncated)
        {
            LogTruncated(this.logger, completion.ModelId);
        }

        return completion;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionUpdate> CompleteStreamingAsync(IReadOnlyList<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = AnthropicRequestMapper.ToCreateParams(this.modelId, messages, options, this.model);
        await foreach (var streamEvent in this.api.CreateStreamingAsync(request, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (streamEvent.TryPickContentBlockDelta(out var blockDelta) && blockDelta.Delta.TryPickText(out var text))
            {
                yield return new ChatCompletionUpdate(text.Text, false, this.modelId);
            }
            else if (streamEvent.TryPickDelta(out var messageDelta) && messageDelta.Delta.StopReason?.Raw() == RefusalStopReason)
            {
                throw new AnthropicRefusalException(this.modelId, AnthropicResponseMapper.RefusalCategory(messageDelta.Delta.StopDetails));
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        yield return new ChatCompletionUpdate(string.Empty, true, this.modelId);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Anthropic response truncated at max tokens for model {ModelId}")]
    private static partial void LogTruncated(ILogger logger, string modelId);
}
