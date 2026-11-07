using Anthropic.Models.Messages;
using System.Text.Json;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Builds SDK stream events from API-shaped JSON.</summary>
internal static class StreamFixtures
{
    public static RawMessageStreamEvent Event(string json) =>
        JsonSerializer.Deserialize<RawMessageStreamEvent>(json) ?? throw new InvalidOperationException("Null event.");

    public static RawMessageStreamEvent TextDelta(string text) =>
        Event($$$"""{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":{{{JsonSerializer.Serialize(text)}}}}}""");

    public static RawMessageStreamEvent MessageDelta(string stopReason, string? refusalCategory = null) =>
        Event(refusalCategory is null
            ? $$$"""{"type":"message_delta","delta":{"stop_reason":"{{{stopReason}}}","stop_sequence":null},"usage":{"output_tokens":5}}"""
            : $$$"""{"type":"message_delta","delta":{"stop_reason":"{{{stopReason}}}","stop_sequence":null,"stop_details":{"type":"refusal","category":"{{{refusalCategory}}}","explanation":"declined"}},"usage":{"output_tokens":0}}""");

    public static RawMessageStreamEvent Stop() => Event("""{"type":"message_stop"}""");
}
