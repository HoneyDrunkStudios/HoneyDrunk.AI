namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Configuration for the Anthropic provider. The host supplies the key from Vault.</summary>
public sealed class AnthropicProviderOptions
{
    /// <summary>Gets or sets the API key. Never logged or included in error messages.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets the models this provider serves.</summary>
    public IList<AnthropicModelOptions> Models { get; } = [];
}
