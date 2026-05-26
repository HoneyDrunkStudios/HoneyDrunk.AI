using HoneyDrunk.AI.Abstractions.Providers;

namespace HoneyDrunk.AI.Providers.InMemory;

/// <summary>Capability declarations for the deterministic InMemory provider.</summary>
public static class InMemoryCapabilities
{
    /// <summary>Gets all synthetic InMemory model capabilities.</summary>
    public static ModelCapabilityDeclaration[] All { get; } =
    [
        new("inmemory", "inmemory:fast", 8_192, true, false, false, ["local"], 0m, 0m),
        new("inmemory", "inmemory:large", 32_768, true, false, false, ["local"], 0m, 0m),
    ];
}
