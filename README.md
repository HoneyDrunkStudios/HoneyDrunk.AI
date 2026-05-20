# HoneyDrunk.AI

HoneyDrunk.AI is the Grid AI Node for inference contracts, model routing, cost accounting, and provider slots established by ADR-0016.

## Packages

- `HoneyDrunk.AI.Abstractions` — D3 contracts for chat, embeddings, providers, routing, capabilities, and cost.
- `HoneyDrunk.AI` — runtime DI, default model router, cost-first policy, cost ledger, and telemetry constants.
- `HoneyDrunk.AI.Providers.InMemory` — deterministic local provider for tests, evals, and canaries.
- `HoneyDrunk.AI.Providers.OpenAI`, `HoneyDrunk.AI.Providers.Anthropic`, `HoneyDrunk.AI.Providers.AzureOpenAI` — ADR-0016 follow-up provider slots.

## For downstream consumers — canary projects

```csharp
using HoneyDrunk.AI;
using HoneyDrunk.AI.Abstractions;
using HoneyDrunk.AI.Providers.InMemory;
using Microsoft.Extensions.DependencyInjection;

var messages = new[] { new ChatMessage(ChatRole.User, "ping") };
var scripted = new Dictionary<string, ChatCompletion>
{
    [InMemoryChatClient.Fingerprint(messages)] = new(new ChatMessage(ChatRole.Assistant, "pong"), "inmemory:fast"),
};

var services = new ServiceCollection()
    .AddHoneyDrunkAI()
    .AddSingleton<IModelProvider>(_ => new InMemoryModelProvider(scripted))
    .BuildServiceProvider();
```

## Production host sketch

Register `HoneyDrunk.Vault`/App Configuration in the deployable host, then add `HoneyDrunk.AI` plus a real provider implementation when provider follow-up packets land.

```csharp
services.AddHoneyDrunkAI();
// services.AddVault().AddAppConfigurationProvider(...);
// services.AddModelProvider<OpenAIModelProvider>();
```
