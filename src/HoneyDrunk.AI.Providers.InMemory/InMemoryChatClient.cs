using HoneyDrunk.AI.Abstractions.Chat;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace HoneyDrunk.AI.Providers.InMemory;

/// <summary>Deterministic chat client with scripted responses and echo fallback.</summary>
public sealed class InMemoryChatClient : IChatClient
{
    private readonly string modelId;
    private readonly IReadOnlyDictionary<string, ChatCompletion> scriptedResponses;

    /// <summary>Initializes a new instance of the <see cref="InMemoryChatClient"/> class.</summary>
    /// <param name="modelId">The synthetic model id.</param>
    /// <param name="scriptedResponses">Scripted completions keyed by request fingerprint.</param>
    public InMemoryChatClient(string modelId, IReadOnlyDictionary<string, ChatCompletion>? scriptedResponses = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        this.modelId = modelId;
        this.scriptedResponses = scriptedResponses ?? new Dictionary<string, ChatCompletion>();
    }

    /// <inheritdoc />
    public Task<ChatCompletion> CompleteAsync(IReadOnlyList<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(messages);
        var fingerprint = Fingerprint(messages);
        if (this.scriptedResponses.TryGetValue(fingerprint, out var scripted))
        {
            return Task.FromResult(scripted);
        }

        var firstUserMessage = messages.FirstOrDefault(message => message.Role == ChatRole.User)?.Content ?? string.Empty;
        var content = $"[InMemory:{this.modelId}] {firstUserMessage}";
        return Task.FromResult(new ChatCompletion(new ChatMessage(ChatRole.Assistant, content), this.modelId, EstimateTokens(messages.Select(message => message.Content)), EstimateTokens([content])));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionUpdate> CompleteStreamingAsync(IReadOnlyList<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var completion = await this.CompleteAsync(messages, options, cancellationToken).ConfigureAwait(false);
        yield return new ChatCompletionUpdate(completion.Message.Content, true, completion.ModelId);
    }

    /// <summary>Computes the deterministic request fingerprint used for scripts.</summary>
    /// <param name="messages">The request messages.</param>
    /// <returns>A SHA-256 fingerprint.</returns>
    public static string Fingerprint(IReadOnlyList<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var canonical = new StringBuilder();
        foreach (var message in messages)
        {
            AppendLengthPrefixed(canonical, message.Role.ToString());
            AppendLengthPrefixed(canonical, message.Name);
            AppendLengthPrefixed(canonical, message.Content);
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()))).ToLowerInvariant();
    }

    private static void AppendLengthPrefixed(StringBuilder builder, string? value)
    {
        if (value is null)
        {
            builder.Append("-1:");
            return;
        }

        builder.Append(value.Length).Append(':').Append(value).Append(';');
    }

    private static int EstimateTokens(IEnumerable<string> values) => Math.Max(1, values.Sum(value => value.Length) / 4);
}
