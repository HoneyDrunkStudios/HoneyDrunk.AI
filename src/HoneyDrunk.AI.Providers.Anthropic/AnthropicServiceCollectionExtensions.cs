using Anthropic;
using HoneyDrunk.AI.Abstractions.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Registers the Anthropic provider.</summary>
public static class AnthropicServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Anthropic provider as an <see cref="IModelProvider"/>. The host binds the options
    /// (models and cost rates from App Configuration, the key from Vault); options are validated at startup.
    /// Logging must be registered by the host.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Fills the provider options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddAnthropicModelProvider(this IServiceCollection services, Action<AnthropicProviderOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<AnthropicProviderOptions>().Configure(configure).ValidateOnStart();
        services.AddSingleton<IValidateOptions<AnthropicProviderOptions>, AnthropicProviderOptionsValidation>();
        services.AddSingleton<IAnthropicMessagesApi>(provider =>
        {
            var apiKey = provider.GetRequiredService<IOptions<AnthropicProviderOptions>>().Value.ApiKey;
            return new AnthropicSdkMessagesApi(new AnthropicClient { ApiKey = apiKey }, apiKey);
        });
        services.AddSingleton<IModelProvider, AnthropicModelProvider>();
        return services;
    }

    private sealed class AnthropicProviderOptionsValidation : IValidateOptions<AnthropicProviderOptions>
    {
        public ValidateOptionsResult Validate(string? name, AnthropicProviderOptions options)
        {
            try
            {
                AnthropicProviderOptionsValidator.Validate(options);
                return ValidateOptionsResult.Success;
            }
            catch (InvalidOperationException error)
            {
                return ValidateOptionsResult.Fail(error.Message);
            }
        }
    }
}
