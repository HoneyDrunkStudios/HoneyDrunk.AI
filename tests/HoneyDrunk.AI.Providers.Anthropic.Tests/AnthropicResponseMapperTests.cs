using HoneyDrunk.AI.Abstractions.Chat;
using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Anthropic response to chat completion mapping tests.</summary>
public sealed class AnthropicResponseMapperTests
{
    /// <summary>Text blocks are joined in order.</summary>
    [Fact]
    public void Concatenates_text_blocks_in_order()
    {
        var response = Fixtures.Message("""
            {"id":"msg_1","type":"message","role":"assistant","model":"test-model-a",
             "content":[{"type":"text","text":"Hel"},{"type":"text","text":"lo"}],
             "stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":2}}
            """);

        var (completion, _) = AnthropicResponseMapper.ToCompletion(response);

        Assert.Equal("Hello", completion.Message.Content);
        Assert.Equal(ChatRole.Assistant, completion.Message.Role);
    }

    /// <summary>Thinking and other non-text blocks are left out.</summary>
    [Fact]
    public void Skips_non_text_blocks()
    {
        var response = Fixtures.Message("""
            {"id":"msg_1","type":"message","role":"assistant","model":"test-model-a",
             "content":[{"type":"thinking","thinking":"hidden","signature":"sig"},{"type":"text","text":"shown"}],
             "stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":2}}
            """);

        var (completion, _) = AnthropicResponseMapper.ToCompletion(response);

        Assert.Equal("shown", completion.Message.Content);
    }

    /// <summary>Token usage and the served model are carried across.</summary>
    [Fact]
    public void Copies_usage_and_model()
    {
        var (completion, truncated) = AnthropicResponseMapper.ToCompletion(Fixtures.TextMessage("hi", inputTokens: 12, outputTokens: 5));

        Assert.Equal("test-model-a", completion.ModelId);
        Assert.Equal(12, completion.InputTokens);
        Assert.Equal(5, completion.OutputTokens);
        Assert.False(truncated);
    }

    /// <summary>A max-tokens stop returns the text and flags it as truncated.</summary>
    [Fact]
    public void Flags_max_tokens_as_truncated()
    {
        var (completion, truncated) = AnthropicResponseMapper.ToCompletion(Fixtures.TextMessage("partial", stopReason: "max_tokens"));

        Assert.True(truncated);
        Assert.Equal("partial", completion.Message.Content);
    }

    /// <summary>A refusal is an exception carrying the model and category, never an empty success.</summary>
    [Fact]
    public void Throws_on_refusal_with_category()
    {
        var error = Assert.Throws<AnthropicRefusalException>(() => AnthropicResponseMapper.ToCompletion(Fixtures.RefusalMessage("cyber")));

        Assert.Equal("test-model-a", error.ModelId);
        Assert.Equal("cyber", error.Category);
        Assert.Contains("refusal", error.Message, StringComparison.Ordinal);
    }
}
