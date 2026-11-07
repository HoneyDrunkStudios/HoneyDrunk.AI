using Anthropic;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Builds the SDK client from provider options. The key is set explicitly, so no ambient credential is read.</summary>
internal static class AnthropicClientFactory
{
    /// <summary>Creates the client, applying retries and timeout only when configured.</summary>
    /// <param name="options">Validated provider options.</param>
    /// <returns>The SDK client.</returns>
    public static AnthropicClient Create(AnthropicProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new AnthropicClient { ApiKey = options.ApiKey, MaxRetries = options.MaxRetries, Timeout = options.Timeout };
    }
}
