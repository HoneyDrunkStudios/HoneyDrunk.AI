namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Configuration for the Anthropic provider. The host supplies the key from Vault.</summary>
public sealed class AnthropicProviderOptions
{
    /// <summary>Gets or sets the API key. Never logged or included in error messages.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets the models this provider serves.</summary>
    public IList<AnthropicModelOptions> Models { get; } = [];

    /// <summary>
    /// Gets or sets how many times the SDK retries a failed request (connection errors, timeouts, 408, 409, 429, 5xx).
    /// <see langword="null"/> keeps the SDK default. A retried request that the server already completed is billed again.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>Gets or sets the time allowed for one HTTP attempt. <see langword="null"/> keeps the SDK default.</summary>
    public TimeSpan? Timeout { get; set; }
}
