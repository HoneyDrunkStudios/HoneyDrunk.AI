namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>
/// One Anthropic model the host makes available, with its declared capabilities and cost rates.
/// Hosts fill these from App Configuration; no model name is built into this package.
/// </summary>
public sealed class AnthropicModelOptions
{
    /// <summary>Gets or sets the model identifier sent to the API.</summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the model's maximum context window in tokens.</summary>
    public int MaxContextTokens { get; set; }

    /// <summary>Gets or sets the output budget used when a caller does not set one.</summary>
    public int DefaultMaxOutputTokens { get; set; } = 16000;

    /// <summary>Gets or sets a value indicating whether the model accepts image input.</summary>
    public bool SupportsVision { get; set; }

    /// <summary>Gets or sets a value indicating whether the model supports tool calls.</summary>
    public bool SupportsFunctionCalling { get; set; }

    /// <summary>Gets or sets the regions the model may be served from.</summary>
    public string[] SupportedRegions { get; set; } = [];

    /// <summary>Gets or sets the input cost per thousand tokens, when known.</summary>
    public decimal? InputCostPerKToken { get; set; }

    /// <summary>Gets or sets the output cost per thousand tokens, when known.</summary>
    public decimal? OutputCostPerKToken { get; set; }
}
