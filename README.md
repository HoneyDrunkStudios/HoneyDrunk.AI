# HoneyDrunk.AI

HoneyDrunk.AI is the Grid AI Node for inference contracts, model routing, cost accounting, and provider slots established by ADR-0016.

## Packages

- `HoneyDrunk.AI.Abstractions` — D3 contracts for chat, embeddings, providers, routing, capabilities, and cost.
- `HoneyDrunk.AI` — runtime DI, default model router, cost-first policy, cost ledger, and telemetry constants.
- `HoneyDrunk.AI.Providers.InMemory` — deterministic local provider for tests, evals, and canaries.
- `HoneyDrunk.AI.Providers.Anthropic` — chat provider over the official Anthropic SDK (see below).
- `HoneyDrunk.AI.Providers.OpenAI`, `HoneyDrunk.AI.Providers.AzureOpenAI` — ADR-0016 follow-up provider slots.

The OpenAI and Azure OpenAI slots still throw `NotImplementedException` and are deliberately non-packable. Releases include Abstractions, the core runtime, the InMemory provider and the Anthropic provider.

## Anthropic provider

```csharp
services.AddLogging();
services.AddAnthropicModelProvider(options =>
{
    options.ApiKey = apiKeyFromVault;            // never hardcode; the host reads it from Vault
    foreach (var model in modelsFromAppConfig)   // ids, context sizes and cost rates from App Configuration
    {
        options.Models.Add(model);
    }
});
```

- Only configured models are served; `GetChatClient` rejects any other ID and lists the configured ones.
- Chat only: `GetEmbeddingGenerator` throws `NotSupportedException`.
- `ChatOptions.Temperature` is ignored, because current Anthropic models reject sampling parameters. `MaxOutputTokens` falls back to each model's `DefaultMaxOutputTokens` (16000 unless configured).
- System messages are joined into the request's system prompt. Tool messages, and conversations ending with an assistant turn (prefill), are rejected before any request is sent.
- A refusal raises `AnthropicRefusalException` with the model and category. A response cut off at the token limit is returned and logged as a warning.
- `MaxRetries` and `Timeout` are optional. Left unset, the SDK's defaults apply (currently 2 retries and a 10-minute timeout per attempt). Retries cover connection errors, timeouts, 408, 409, 429 and 5xx. A retried request that the server already finished is billed again, so budget-sensitive hosts should set `MaxRetries` deliberately.
- A stream that ends before `message_stop` throws `IOException` instead of reporting a finished answer. An SDK timeout surfaces as `TimeoutException`, never as the caller's cancellation.
- The live smoke test runs only when `HONEYDRUNK_AI_ANTHROPIC_LIVE=1`, `ANTHROPIC_API_KEY` and `HONEYDRUNK_AI_ANTHROPIC_LIVE_MODEL` are set. It spends real tokens, and CI skips it.

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

Register `HoneyDrunk.Vault`/App Configuration in the deployable host, then add `HoneyDrunk.AI` plus a provider such as the Anthropic provider.

```csharp
services.AddHoneyDrunkAI();
// services.AddVault().AddAppConfigurationProvider(...);
// services.AddAnthropicModelProvider(options => { /* key from Vault, models from App Configuration */ });
```
