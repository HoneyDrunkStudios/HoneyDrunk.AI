using Anthropic.Models.Messages;
using HoneyDrunk.AI.Abstractions.Chat;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Maps Anthropic Messages responses to provider-neutral chat completions.</summary>
internal static class AnthropicResponseMapper
{
    private const string RefusalStopReason = "refusal";
    private const string MaxTokensStopReason = "max_tokens";

    /// <summary>Maps a response. Only text blocks become content.</summary>
    /// <param name="response">The API response.</param>
    /// <returns>The completion, and whether the output was cut off at the token limit.</returns>
    /// <exception cref="AnthropicRefusalException">The model declined the request.</exception>
    public static (ChatCompletion completion, bool truncated) ToCompletion(Message response)
    {
        ArgumentNullException.ThrowIfNull(response);
        var modelId = response.Model.Raw();
        var stopReason = StopReason(response);
        if (stopReason == RefusalStopReason)
        {
            throw new AnthropicRefusalException(modelId, RefusalCategory(response.StopDetails));
        }

        var text = string.Concat(response.Content
            .Select(block => block.TryPickText(out var textBlock) ? textBlock : null)
            .OfType<TextBlock>()
            .Select(textBlock => textBlock.Text));

        var completion = new ChatCompletion(
            new ChatMessage(ChatRole.Assistant, text),
            modelId,
            checked((int)response.Usage.InputTokens),
            checked((int)response.Usage.OutputTokens));
        return (completion, stopReason == MaxTokensStopReason);
    }

    /// <summary>Reads the refusal category from stop details, if the API sent one.</summary>
    /// <param name="details">The stop details.</param>
    /// <returns>The category's wire value, or <see langword="null"/>.</returns>
    internal static string? RefusalCategory(RefusalStopDetails? details) => details?.Category?.Raw();

    private static string? StopReason(Message response) => response.StopReason?.Raw();
}
