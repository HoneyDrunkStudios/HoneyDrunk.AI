namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Validates <see cref="AnthropicProviderOptions"/>. Messages name fields, never secret values.</summary>
public static class AnthropicProviderOptionsValidator
{
    /// <summary>Throws when the options cannot serve requests.</summary>
    /// <param name="options">The options to check.</param>
    /// <exception cref="InvalidOperationException">The options are incomplete or inconsistent.</exception>
    public static void Validate(AnthropicProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException("Anthropic provider: ApiKey is required.");
        }

        if (options.Models.Count == 0)
        {
            throw new InvalidOperationException("Anthropic provider: at least one model must be configured in Models.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var model in options.Models)
        {
            if (string.IsNullOrWhiteSpace(model.ModelId))
            {
                throw new InvalidOperationException("Anthropic provider: every model needs a ModelId.");
            }

            if (!seen.Add(model.ModelId))
            {
                throw new InvalidOperationException($"Anthropic provider: model '{model.ModelId}' is configured more than once.");
            }

            if (model.DefaultMaxOutputTokens < 1)
            {
                throw new InvalidOperationException($"Anthropic provider: model '{model.ModelId}' needs a DefaultMaxOutputTokens of at least 1.");
            }
        }
    }
}
