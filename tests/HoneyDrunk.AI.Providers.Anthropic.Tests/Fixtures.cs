using Anthropic.Models.Messages;
using System.Text.Json;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Builds SDK response objects from API-shaped JSON, the way the SDK itself reads them.</summary>
internal static class Fixtures
{
    public static Message Message(string json) => global::Anthropic.Models.Messages.Message.FromRawUnchecked(Raw(json));

    public static Message TextMessage(string text, string stopReason = "end_turn", int inputTokens = 12, int outputTokens = 5) =>
        Message($$$"""
        {"id":"msg_1","type":"message","role":"assistant","model":"test-model-a",
         "content":[{"type":"text","text":{{{JsonSerializer.Serialize(text)}}}}],
         "stop_reason":"{{{stopReason}}}","stop_sequence":null,
         "usage":{"input_tokens":{{{inputTokens}}},"output_tokens":{{{outputTokens}}}}}
        """);

    public static Message RefusalMessage(string category) =>
        Message($$$"""
        {"id":"msg_2","type":"message","role":"assistant","model":"test-model-a","content":[],
         "stop_reason":"refusal","stop_sequence":null,
         "stop_details":{"type":"refusal","category":"{{{category}}}","explanation":"declined"},
         "usage":{"input_tokens":3,"output_tokens":0}}
        """);

    public static IReadOnlyDictionary<string, JsonElement> Raw(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }
}
