using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Embeddings;
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

    /// <summary>Rejects invalid embedding dimensions.</summary>
    [Fact]
    public async Task EmbeddingGenerator_rejects_invalid_dimensions()
    {
        var generator = new InMemoryEmbeddingGenerator("inmemory:fast");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => generator.GenerateAsync(["same"], new EmbeddingOptions(Dimensions: 0)));
    }

    /// <summary>Uses length-prefixed fingerprints so delimiter characters are unambiguous.</summary>
    [Fact]
    public void Fingerprint_handles_delimiter_characters()
    {
        var first = InMemoryChatClient.Fingerprint([new ChatMessage(ChatRole.User, "a:b\nc")]);
        var second = InMemoryChatClient.Fingerprint([new ChatMessage(ChatRole.User, "a:b"), new ChatMessage(ChatRole.User, "c")]);
        Assert.NotEqual(first, second);
    }

    /// <summary>Rejects a null scripted-response dictionary at construction time.</summary>
    [Fact]
    public void Provider_rejects_null_scripted_responses()
    {
        Assert.Throws<ArgumentNullException>(() => new InMemoryModelProvider(null!));
    }

    /// <summary>Exposes matching clients and streaming-capable capabilities.</summary>
    [Fact]
    public async Task Provider_exposes_deterministic_clients()
    {
        var provider = new InMemoryModelProvider();
        Assert.All(provider.DeclaredCapabilities, capability => Assert.True(capability.SupportsStreaming));

        var chat = provider.GetChatClient("inmemory:fast");
        var completion = await chat.CompleteAsync([new ChatMessage(ChatRole.User, "provider")]);
        Assert.Equal("[InMemory:inmemory:fast] provider", completion.Message.Content);

        var generator = provider.GetEmbeddingGenerator("inmemory:large");
        var embeddings = await generator.GenerateAsync(["provider"]);
        Assert.Equal("inmemory:large", embeddings[0].ModelId);
    }
}
