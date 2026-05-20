namespace HoneyDrunk.AI.Telemetry;

/// <summary>Telemetry constants for HoneyDrunk.AI inference calls.</summary>
public static class InferenceTelemetry
{
    /// <summary>The chat completion activity name.</summary>
    public const string ChatCompletionActivityName = "HoneyDrunk.AI.ChatCompletion";

    /// <summary>The embedding generation activity name.</summary>
    public const string EmbeddingGenerationActivityName = "HoneyDrunk.AI.EmbeddingGeneration";

    /// <summary>The provider id tag.</summary>
    public const string ProviderIdTag = "honeydrunk.ai.provider_id";

    /// <summary>The model id tag.</summary>
    public const string ModelIdTag = "honeydrunk.ai.model_id";

    /// <summary>The input token tag.</summary>
    public const string InputTokensTag = "honeydrunk.ai.input_tokens";

    /// <summary>The output token tag.</summary>
    public const string OutputTokensTag = "honeydrunk.ai.output_tokens";

    /// <summary>The estimated cost tag.</summary>
    public const string EstimatedCostTag = "honeydrunk.ai.estimated_cost";
}
