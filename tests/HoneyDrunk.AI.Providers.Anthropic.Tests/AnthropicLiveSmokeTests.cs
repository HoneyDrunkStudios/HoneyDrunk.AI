using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Providers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>
/// One real round trip, run only when an operator opts in with HONEYDRUNK_AI_ANTHROPIC_LIVE=1,
/// ANTHROPIC_API_KEY and HONEYDRUNK_AI_ANTHROPIC_LIVE_MODEL. Skipped in CI. It spends real tokens.
/// </summary>
public sealed class AnthropicLiveSmokeTests
{
    /// <summary>A short prompt returns non-empty text and reports usage.</summary>
    [LiveFact]
    public async Task Live_completion_round_trip()
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddAnthropicModelProvider(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")!;
                options.Models.Add(new AnthropicModelOptions
                {
                    ModelId = Environment.GetEnvironmentVariable("HONEYDRUNK_AI_ANTHROPIC_LIVE_MODEL")!,
                    MaxContextTokens = 200000,
                    DefaultMaxOutputTokens = 64,
                });
            })
            .BuildServiceProvider();
        var provider = services.GetRequiredService<IModelProvider>();
        var modelId = Assert.Single(provider.DeclaredCapabilities).ModelId;

        var completion = await provider.GetChatClient(modelId).CompleteAsync([new ChatMessage(ChatRole.User, "Reply with the single word: pong")]);

        Assert.False(string.IsNullOrWhiteSpace(completion.Message.Content));
        Assert.True(completion.OutputTokens > 0);
    }

    /// <summary>Runs only when the live-test environment variables are all set.</summary>
    private sealed class LiveFactAttribute : FactAttribute
    {
        public LiveFactAttribute()
        {
            var optedIn = Environment.GetEnvironmentVariable("HONEYDRUNK_AI_ANTHROPIC_LIVE") == "1"
                && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"))
                && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HONEYDRUNK_AI_ANTHROPIC_LIVE_MODEL"));
            if (!optedIn)
            {
                this.Skip = "Live Anthropic test: set HONEYDRUNK_AI_ANTHROPIC_LIVE=1, ANTHROPIC_API_KEY and HONEYDRUNK_AI_ANTHROPIC_LIVE_MODEL to run.";
            }
        }
    }
}
