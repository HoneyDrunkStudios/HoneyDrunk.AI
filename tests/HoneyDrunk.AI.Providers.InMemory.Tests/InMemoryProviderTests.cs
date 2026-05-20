using HoneyDrunk.AI.Abstractions;
using HoneyDrunk.AI.Providers.InMemory;
using Xunit;

namespace HoneyDrunk.AI.Providers.InMemory.Tests;

/// <summary>InMemory provider behavior tests.</summary>
public sealed class InMemoryProviderTests
{
    /// <summary>Returns a scripted response when fingerprint matches.</summary>
    [Fact]
    public async Task ChatClient_returns_scripted_response()
    {
        var messages = new[] { new ChatMessage(ChatRole.User, "hello") };
        var fingerprint = InMemoryChatClient.Fingerprint(messages);
        var scripted = new ChatCompletion(new ChatMessage(ChatRole.Assistant, "scripted"), "inmemory:fast");
        var client = new InMemoryChatClient("inmemory:fast", new Dictionary<string, ChatCompletion> { [fingerprint] = scripted });

        var completion = await client.CompleteAsync(messages);
        Assert.Equal("scripted", completion.Message.Content);
    }

    /// <summary>Returns deterministic echo when no script matches.</summary>
    [Fact]
    public async Task ChatClient_returns_default_echo()
    {
        var client = new InMemoryChatClient("inmemory:fast");
        var completion = await client.CompleteAsync([new ChatMessage(ChatRole.User, "ping")]);
        Assert.Equal("[InMemory:inmemory:fast] ping", completion.Message.Content);
    }

    /// <summary>Returns stable embeddings for the same input.</summary>
    [Fact]
    public async Task EmbeddingGenerator_is_deterministic()
    {
        var generator = new InMemoryEmbeddingGenerator("inmemory:fast");
        var first = await generator.GenerateAsync(["same"], new EmbeddingOptions(Dimensions: 4));
        var second = await generator.GenerateAsync(["same"], new EmbeddingOptions(Dimensions: 4));
        Assert.Equal(first[0].Vector, second[0].Vector);
    }
}
