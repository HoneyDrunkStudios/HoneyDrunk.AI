using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Provider shell, capability declaration and options validation tests.</summary>
public sealed class AnthropicModelProviderTests
{
    /// <summary>Maps every configured model to a capability declaration.</summary>
    [Fact]
    public void DeclaredCapabilities_maps_each_configured_model()
    {
        var provider = CreateProvider(ValidOptions());

        var declarations = provider.DeclaredCapabilities;

        Assert.Equal(2, declarations.Length);
        var first = Assert.Single(declarations, d => d.ModelId == "test-model-a");
        Assert.Equal("anthropic", first.ProviderId);
        Assert.True(first.SupportsStreaming);
        Assert.Equal(200000, first.MaxContextTokens);
        Assert.Equal(0.001m, first.InputCostPerKToken);
        Assert.Equal(0.005m, first.OutputCostPerKToken);
        Assert.Contains(declarations, d => d.ModelId == "test-model-b");
    }

    /// <summary>Rejects a model ID that is not configured and lists the configured ones.</summary>
    [Fact]
    public void GetChatClient_rejects_unconfigured_model()
    {
        var provider = CreateProvider(ValidOptions());

        var error = Assert.Throws<ArgumentException>(() => provider.GetChatClient("not-configured"));

        Assert.Contains("test-model-a", error.Message, StringComparison.Ordinal);
        Assert.Contains("test-model-b", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A configured model gets a working chat client over the provider's API seam.</summary>
    [Fact]
    public async Task GetChatClient_returns_client_for_configured_model()
    {
        var api = new FakeMessagesApi { NextMessage = Fixtures.TextMessage("ok") };
        var provider = new AnthropicModelProvider(Options.Create(ValidOptions()), api, NullLoggerFactory.Instance);

        var completion = await provider.GetChatClient("test-model-b").CompleteAsync([new HoneyDrunk.AI.Abstractions.Chat.ChatMessage(HoneyDrunk.AI.Abstractions.Chat.ChatRole.User, "hi")]);

        Assert.Equal("ok", completion.Message.Content);
        Assert.Contains("\"model\": \"test-model-b\"", Assert.Single(api.Requests).ToString(), StringComparison.Ordinal);
    }

    /// <summary>Embeddings are not offered by this provider.</summary>
    [Fact]
    public void GetEmbeddingGenerator_is_not_supported()
    {
        var provider = CreateProvider(ValidOptions());

        Assert.Throws<NotSupportedException>(() => provider.GetEmbeddingGenerator("test-model-a"));
    }

    /// <summary>A blank key fails validation, and no failure message echoes the key.</summary>
    [Fact]
    public void Validate_rejects_blank_api_key_without_echoing_secrets()
    {
        var blank = ValidOptions();
        blank.ApiKey = "   ";
        Assert.Throws<InvalidOperationException>(() => AnthropicProviderOptionsValidator.Validate(blank));

        var noModels = new AnthropicProviderOptions { ApiKey = "sk-secret-value" };
        var error = Assert.Throws<InvalidOperationException>(() => AnthropicProviderOptionsValidator.Validate(noModels));
        Assert.DoesNotContain("sk-secret-value", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Duplicate model IDs fail validation.</summary>
    [Fact]
    public void Validate_rejects_duplicate_model_ids()
    {
        var options = ValidOptions();
        options.Models.Add(new AnthropicModelOptions { ModelId = "test-model-a", MaxContextTokens = 1000 });

        var error = Assert.Throws<InvalidOperationException>(() => AnthropicProviderOptionsValidator.Validate(options));

        Assert.Contains("test-model-a", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A default output budget under one fails validation.</summary>
    [Fact]
    public void Validate_rejects_non_positive_default_max_output_tokens()
    {
        var options = ValidOptions();
        options.Models[0].DefaultMaxOutputTokens = 0;

        Assert.Throws<InvalidOperationException>(() => AnthropicProviderOptionsValidator.Validate(options));
    }

    /// <summary>A negative retry count fails validation.</summary>
    [Fact]
    public void Validate_rejects_negative_max_retries()
    {
        var options = ValidOptions();
        options.MaxRetries = -1;

        Assert.Throws<InvalidOperationException>(() => AnthropicProviderOptionsValidator.Validate(options));
    }

    /// <summary>A zero or negative timeout fails validation.</summary>
    [Fact]
    public void Validate_rejects_non_positive_timeout()
    {
        var options = ValidOptions();
        options.Timeout = TimeSpan.Zero;

        Assert.Throws<InvalidOperationException>(() => AnthropicProviderOptionsValidator.Validate(options));
    }

    /// <summary>Configured retries and timeout reach the SDK client.</summary>
    [Fact]
    public void Client_factory_applies_configured_retries_and_timeout()
    {
        var options = ValidOptions();
        options.MaxRetries = 0;
        options.Timeout = TimeSpan.FromSeconds(90);

        using var client = AnthropicClientFactory.Create(options);

        Assert.Equal(0, client.MaxRetries);
        Assert.Equal(TimeSpan.FromSeconds(90), client.Timeout);
        Assert.Equal("sk-test-key", client.ApiKey);
    }

    /// <summary>Unset retries and timeout keep the SDK's own defaults.</summary>
    [Fact]
    public void Client_factory_keeps_sdk_defaults_when_unset()
    {
        using var sdkDefaults = new global::Anthropic.AnthropicClient { ApiKey = "x" };

        using var client = AnthropicClientFactory.Create(ValidOptions());

        Assert.Equal(sdkDefaults.MaxRetries, client.MaxRetries);
        Assert.Equal(sdkDefaults.Timeout, client.Timeout);
    }

    /// <summary>Valid options pass validation.</summary>
    [Fact]
    public void Validate_accepts_valid_options()
    {
        AnthropicProviderOptionsValidator.Validate(ValidOptions());
    }

    internal static AnthropicProviderOptions ValidOptions()
    {
        var options = new AnthropicProviderOptions { ApiKey = "sk-test-key" };
        options.Models.Add(new AnthropicModelOptions
        {
            ModelId = "test-model-a",
            MaxContextTokens = 200000,
            InputCostPerKToken = 0.001m,
            OutputCostPerKToken = 0.005m,
        });
        options.Models.Add(new AnthropicModelOptions { ModelId = "test-model-b", MaxContextTokens = 100000 });
        return options;
    }

    private static AnthropicModelProvider CreateProvider(AnthropicProviderOptions options) =>
        new(Options.Create(options), new FakeMessagesApi(), NullLoggerFactory.Instance);
}
