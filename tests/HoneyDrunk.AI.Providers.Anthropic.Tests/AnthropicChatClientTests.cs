using HoneyDrunk.AI.Abstractions.Chat;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Chat client behavior over a fake Messages API.</summary>
public sealed class AnthropicChatClientTests
{
    private static readonly AnthropicModelOptions Model = new() { ModelId = "test-model-a", MaxContextTokens = 200000, DefaultMaxOutputTokens = 1024 };

    /// <summary>A request goes out through the request mapper and comes back through the response mapper.</summary>
    [Fact]
    public async Task CompleteAsync_round_trips_through_mappers()
    {
        var api = new FakeMessagesApi { NextMessage = Fixtures.TextMessage("hello back") };
        var client = new AnthropicChatClient("test-model-a", Model, api, new ListLogger());

        var completion = await client.CompleteAsync([new ChatMessage(ChatRole.User, "hello")]);

        Assert.Equal("hello back", completion.Message.Content);
        var request = Assert.Single(api.Requests);
        Assert.Contains("\"max_tokens\": 1024", request.ToString(), StringComparison.Ordinal);
    }

    /// <summary>A truncated answer is returned and logged once as a warning.</summary>
    [Fact]
    public async Task CompleteAsync_logs_once_when_truncated()
    {
        var logger = new ListLogger();
        var api = new FakeMessagesApi { NextMessage = Fixtures.TextMessage("partial", stopReason: "max_tokens") };
        var client = new AnthropicChatClient("test-model-a", Model, api, logger);

        var completion = await client.CompleteAsync([new ChatMessage(ChatRole.User, "go")]);

        Assert.Equal("partial", completion.Message.Content);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.level);
        Assert.Contains("test-model-a", entry.message, StringComparison.Ordinal);
    }

    /// <summary>Text deltas stream in order, followed by exactly one completion marker.</summary>
    [Fact]
    public async Task Streaming_yields_deltas_then_one_complete_update()
    {
        var api = new FakeMessagesApi
        {
            NextEvents = [StreamFixtures.TextDelta("Hel"), StreamFixtures.TextDelta("lo"), StreamFixtures.MessageDelta("end_turn"), StreamFixtures.Stop()],
        };
        var client = new AnthropicChatClient("test-model-a", Model, api, new ListLogger());

        var updates = new List<ChatCompletionUpdate>();
        await foreach (var update in client.CompleteStreamingAsync([new ChatMessage(ChatRole.User, "hi")]))
        {
            updates.Add(update);
        }

        Assert.Equal(["Hel", "lo", string.Empty], updates.Select(u => u.Delta));
        Assert.Equal([false, false, true], updates.Select(u => u.IsComplete));
        Assert.All(updates, u => Assert.Equal("test-model-a", u.ModelId));
    }

    /// <summary>A stream cut off at the token limit completes and is logged once as a warning.</summary>
    [Fact]
    public async Task Streaming_logs_once_when_truncated()
    {
        var logger = new ListLogger();
        var api = new FakeMessagesApi { NextEvents = [StreamFixtures.TextDelta("part"), StreamFixtures.MessageDelta("max_tokens"), StreamFixtures.Stop()] };
        var client = new AnthropicChatClient("test-model-a", Model, api, logger);

        var updates = new List<ChatCompletionUpdate>();
        await foreach (var update in client.CompleteStreamingAsync([new ChatMessage(ChatRole.User, "go")]))
        {
            updates.Add(update);
        }

        Assert.True(updates[^1].IsComplete);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.level);
    }

    /// <summary>A stream that ends before the stop event is an error, not a finished answer.</summary>
    [Fact]
    public async Task Streaming_without_stop_event_throws()
    {
        var api = new FakeMessagesApi { NextEvents = [StreamFixtures.TextDelta("half an answ")] };
        var client = new AnthropicChatClient("test-model-a", Model, api, new ListLogger());
        var updates = new List<ChatCompletionUpdate>();

        await Assert.ThrowsAsync<IOException>(async () =>
        {
            await foreach (var update in client.CompleteStreamingAsync([new ChatMessage(ChatRole.User, "hi")]))
            {
                updates.Add(update);
            }
        });

        Assert.DoesNotContain(updates, u => u.IsComplete);
    }

    /// <summary>A refusal reported mid-stream raises the refusal exception.</summary>
    [Fact]
    public async Task Streaming_throws_on_refusal()
    {
        var api = new FakeMessagesApi { NextEvents = [StreamFixtures.MessageDelta("refusal", refusalCategory: "bio"), StreamFixtures.Stop()] };
        var client = new AnthropicChatClient("test-model-a", Model, api, new ListLogger());

        var error = await Assert.ThrowsAsync<AnthropicRefusalException>(async () =>
        {
            await foreach (var update in client.CompleteStreamingAsync([new ChatMessage(ChatRole.User, "hi")]))
            {
                Assert.False(update.IsComplete);
            }
        });

        Assert.Equal("bio", error.Category);
    }

    /// <summary>Cancelling mid-stream stops without a completion marker.</summary>
    [Fact]
    public async Task Cancellation_propagates_without_final_update()
    {
        var api = new FakeMessagesApi { NextEvents = [StreamFixtures.TextDelta("one"), StreamFixtures.TextDelta("two"), StreamFixtures.Stop()] };
        var client = new AnthropicChatClient("test-model-a", Model, api, new ListLogger());
        using var cts = new CancellationTokenSource();
        var updates = new List<ChatCompletionUpdate>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var update in client.CompleteStreamingAsync([new ChatMessage(ChatRole.User, "hi")], null, cts.Token))
            {
                updates.Add(update);
                await cts.CancelAsync();
            }
        });

        Assert.DoesNotContain(updates, u => u.IsComplete);
    }
}
