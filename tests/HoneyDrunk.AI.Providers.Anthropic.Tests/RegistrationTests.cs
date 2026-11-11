using HoneyDrunk.AI.Abstractions.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Service registration tests.</summary>
public sealed class RegistrationTests
{
    /// <summary>The provider resolves as an <see cref="IModelProvider"/> with its configured models.</summary>
    [Fact]
    public void Registers_provider_resolvable_as_IModelProvider()
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddAnthropicModelProvider(options =>
            {
                options.ApiKey = "sk-test-key";
                options.Models.Add(new AnthropicModelOptions { ModelId = "test-model-a", MaxContextTokens = 1000 });
            })
            .BuildServiceProvider();

        var provider = Assert.IsType<AnthropicModelProvider>(services.GetRequiredService<IModelProvider>());
        Assert.Equal("test-model-a", Assert.Single(provider.DeclaredCapabilities).ModelId);
    }

    /// <summary>Invalid options fail at startup validation, without echoing the key.</summary>
    [Fact]
    public async Task Invalid_options_fail_on_start()
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddAnthropicModelProvider(options => options.ApiKey = "sk-secret-value")
            .BuildServiceProvider();

        var error = await Assert.ThrowsAsync<OptionsValidationException>(() => services.GetRequiredService<IAsyncStartupValidator>().ValidateAsync(CancellationToken.None));
        Assert.DoesNotContain("sk-secret-value", error.Message, StringComparison.Ordinal);
    }
}
