using Anthropic.Models.Messages;
using HoneyDrunk.AI.Abstractions.Chat;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>
/// Maps provider-neutral chat requests to Anthropic Messages requests. Sampling parameters
/// (temperature, top_p, top_k) are never sent: the current API rejects them.
/// </summary>
internal static class AnthropicRequestMapper
{
    private const string SystemSeparator = "\n\n";

    /// <summary>Builds the Messages request for one chat call.</summary>
    /// <param name="modelId">The configured model identifier.</param>
    /// <param name="messages">The conversation, oldest first.</param>
    /// <param name="options">The caller's options; temperature is ignored.</param>
    /// <param name="model">The configured model options.</param>
    /// <returns>The request to send.</returns>
    public static MessageCreateParams ToCreateParams(string modelId, IReadOnlyList<ChatMessage> messages, ChatOptions? options, AnthropicModelOptions model)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        ArgumentNullException.ThrowIfNull(model);
        if (messages is null || messages.Count == 0)
        {
            throw new ArgumentException("At least one message is required.", nameof(messages));
        }

        var system = new List<string>();
        var turns = new List<MessageParam>();
        ChatRole? lastTurnRole = null;
        foreach (var message in messages)
        {
            switch (message.Role)
            {
                case ChatRole.System:
                    system.Add(message.Content);
                    break;
                case ChatRole.User:
                    turns.Add(new UserMessageParam { Content = message.Content });
                    lastTurnRole = ChatRole.User;
                    break;
                case ChatRole.Assistant:
                    turns.Add(new AssistantMessageParam { Content = message.Content });
                    lastTurnRole = ChatRole.Assistant;
                    break;
                case ChatRole.Tool:
                    throw new NotSupportedException("Tool messages are not supported by the Anthropic provider yet: the chat contract has no tool-call identifiers.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(messages), message.Role, "Unknown chat role.");
            }
        }

        if (lastTurnRole != ChatRole.User)
        {
            throw new ArgumentException("The last turn must be from the user: assistant prefill is not supported by current Anthropic models.", nameof(messages));
        }

        var maxTokens = options?.MaxOutputTokens is >= 1 ? options.MaxOutputTokens.Value : model.DefaultMaxOutputTokens;
        return system.Count == 0
            ? new MessageCreateParams { Model = modelId, MaxTokens = maxTokens, Messages = turns }
            : new MessageCreateParams { Model = modelId, MaxTokens = maxTokens, Messages = turns, System = string.Join(SystemSeparator, system) };
    }
}
