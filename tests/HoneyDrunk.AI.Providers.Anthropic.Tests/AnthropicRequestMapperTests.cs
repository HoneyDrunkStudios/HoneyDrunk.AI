using Anthropic.Models.Messages;
using HoneyDrunk.AI.Abstractions.Chat;
using System.Text.Json;
using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Chat request to Anthropic Messages request mapping tests.</summary>
public sealed class AnthropicRequestMapperTests
{
    private static readonly AnthropicModelOptions Model = new() { ModelId = "test-model-a", MaxContextTokens = 200000, DefaultMaxOutputTokens = 4096 };

    /// <summary>An empty conversation is rejected.</summary>
    [Fact]
    public void Rejects_empty_messages()
    {
        Assert.Throws<ArgumentException>(() => AnthropicRequestMapper.ToCreateParams("test-model-a", [], null, Model));
    }

    /// <summary>System messages are joined into the system parameter and left out of the turn list.</summary>
    [Fact]
    public void Joins_system_messages_into_system_parameter()
    {
        var request = AnthropicRequestMapper.ToCreateParams(
            "test-model-a",
            [new(ChatRole.System, "You are a bee."), new(ChatRole.System, "Be brief."), new(ChatRole.User, "hello")],
            null,
            Model);

        var json = Json(request);
        Assert.Equal("You are a bee.\n\nBe brief.", json.GetProperty("system").GetString());
        Assert.Equal(1, json.GetProperty("messages").GetArrayLength());
    }

    /// <summary>No system message means no system parameter.</summary>
    [Fact]
    public void Omits_system_parameter_without_system_messages()
    {
        var request = AnthropicRequestMapper.ToCreateParams("test-model-a", [new(ChatRole.User, "hello")], null, Model);

        Assert.False(Json(request).TryGetProperty("system", out _));
    }

    /// <summary>User and assistant turns keep their order and roles.</summary>
    [Fact]
    public void Maps_user_and_assistant_in_order()
    {
        var request = AnthropicRequestMapper.ToCreateParams(
            "test-model-a",
            [new(ChatRole.User, "one"), new(ChatRole.Assistant, "two"), new(ChatRole.User, "three")],
            null,
            Model);

        var messages = Json(request).GetProperty("messages").EnumerateArray().ToArray();
        Assert.Equal(["user", "assistant", "user"], messages.Select(m => m.GetProperty("role").GetString()));
        Assert.Equal(["one", "two", "three"], messages.Select(m => m.GetProperty("content").GetString()));
        Assert.Equal("test-model-a", Json(request).GetProperty("model").GetString());
    }

    /// <summary>The tool role cannot be expressed through the current contract.</summary>
    [Fact]
    public void Rejects_tool_role()
    {
        Assert.Throws<NotSupportedException>(() => AnthropicRequestMapper.ToCreateParams(
            "test-model-a",
            [new(ChatRole.User, "hi"), new(ChatRole.Tool, "result")],
            null,
            Model));
    }

    /// <summary>A conversation ending with an assistant turn is rejected as prefill before reaching the API.</summary>
    [Fact]
    public void Rejects_assistant_last_as_prefill()
    {
        var error = Assert.Throws<ArgumentException>(() => AnthropicRequestMapper.ToCreateParams(
            "test-model-a",
            [new(ChatRole.User, "hi"), new(ChatRole.Assistant, "Sure, ")],
            null,
            Model));

        Assert.Contains("prefill", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The caller's output budget wins when set.</summary>
    [Fact]
    public void Uses_caller_max_tokens_when_set()
    {
        var request = AnthropicRequestMapper.ToCreateParams("test-model-a", [new(ChatRole.User, "hi")], new ChatOptions(MaxOutputTokens: 256), Model);

        Assert.Equal(256, Json(request).GetProperty("max_tokens").GetInt32());
    }

    /// <summary>The model's default output budget applies when the caller sets none.</summary>
    [Fact]
    public void Falls_back_to_model_default_max_tokens()
    {
        var request = AnthropicRequestMapper.ToCreateParams("test-model-a", [new(ChatRole.User, "hi")], new ChatOptions(MaxOutputTokens: 0), Model);

        Assert.Equal(4096, Json(request).GetProperty("max_tokens").GetInt32());
    }

    /// <summary>Temperature is never sent; current models reject sampling parameters.</summary>
    [Fact]
    public void Never_sends_temperature()
    {
        var request = AnthropicRequestMapper.ToCreateParams("test-model-a", [new(ChatRole.User, "hi")], new ChatOptions(Temperature: 0.7), Model);

        var json = Json(request);
        Assert.False(json.TryGetProperty("temperature", out _));
        Assert.False(json.TryGetProperty("top_p", out _));
        Assert.False(json.TryGetProperty("top_k", out _));
    }

    /// <summary>Message names are not sent.</summary>
    [Fact]
    public void Does_not_send_message_names()
    {
        var request = AnthropicRequestMapper.ToCreateParams("test-model-a", [new(ChatRole.User, "hi", Name: "visitor-7")], null, Model);

        Assert.DoesNotContain("visitor-7", request.ToString(), StringComparison.Ordinal);
    }

    /// <summary>The request body as JSON. The SDK's <c>ToString()</c> wraps it in <c>BodyData</c>.</summary>
    private static JsonElement Json(MessageCreateParams request) => JsonDocument.Parse(request.ToString()).RootElement.GetProperty("BodyData");
}
