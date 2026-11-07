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
