using HoneyDrunk.AI.Abstractions;
using HoneyDrunk.AI.Cost;
using HoneyDrunk.AI.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HoneyDrunk.AI;

/// <summary>Registers HoneyDrunk.AI runtime services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds HoneyDrunk.AI runtime services.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional runtime options configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddHoneyDrunkAI(this IServiceCollection services, Action<AIOptions>? configure = null)
    {
        services.AddOptions<AIOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<IModelRouter, DefaultModelRouter>();
        services.AddSingleton<ICostLedger, DefaultCostLedger>();
        services.AddSingleton<IRoutingPolicy, CostFirstRoutingPolicy>();
        services.AddSingleton<PolicyLoader>();
        return services;
    }

    /// <summary>Adds a model provider implementation.</summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddModelProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IModelProvider
    {
        services.AddSingleton<IModelProvider, TProvider>();
        return services;
    }
}
