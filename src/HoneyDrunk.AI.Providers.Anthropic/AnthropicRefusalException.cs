namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>The model declined the request. Callers decide whether to try another candidate.</summary>
public sealed class AnthropicRefusalException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="AnthropicRefusalException"/> class.</summary>
    /// <param name="modelId">The model that declined.</param>
    /// <param name="category">The refusal category the API reported, when present.</param>
    public AnthropicRefusalException(string modelId, string? category)
        : base(category is null ? "The model declined the request (refusal)." : $"The model declined the request (refusal, category '{category}').")
    {
        this.ModelId = modelId;
        this.Category = category;
    }

    /// <summary>Gets the model that declined.</summary>
    public string ModelId { get; }

    /// <summary>Gets the refusal category the API reported, when present.</summary>
    public string? Category { get; }
}
