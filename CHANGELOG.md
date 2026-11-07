# Changelog

## [0.3.0] - 2026-11-07

### Added

- `HoneyDrunk.AI.Providers.Anthropic` is now a working, packable chat provider over the official Anthropic SDK (`Anthropic` 13.3.0), replacing the ADR-0016 stub. `AddAnthropicModelProvider` registers it as an `IModelProvider`.
- Models, capability declarations and cost rates come from `AnthropicProviderOptions`, which the host fills from App Configuration; the API key comes from Vault. No model name is built into the package. Options are validated at startup, and validation messages never contain the key.
- Streaming yields text deltas, then one completion update.
- A refusal raises `AnthropicRefusalException` (model and category) instead of returning empty text. A response cut off at the token limit is returned and logged as a warning.
- Errors whose text contains the API key are replaced before they leave the provider, including during stream creation and disposal.

### Notes

- Temperature, `top_p` and `top_k` are never sent; current Anthropic models reject them. `ChatOptions.Temperature` is ignored by this provider.
- Assistant prefill (a conversation ending with an assistant turn) and tool messages are rejected before any request is made.
- Embeddings are not offered; `GetEmbeddingGenerator` throws `NotSupportedException`.
- `HoneyDrunk.AI.Abstractions` is unchanged.

## [0.2.2] - 2026-11-05

### Changed

- Refresh NuGet dependencies and adopt upstream package releases in dependency order. Preserve target frameworks and public contracts.

| Dependency | Previous | Updated |
| --- | --- | --- |
| HoneyDrunk.Kernel | 0.8.1 | 0.8.2 |
| HoneyDrunk.Standards | 0.3.0 | 0.4.3 |
| HoneyDrunk.Standards.Tests | 0.3.0 | 0.4.3 |
| HoneyDrunk.Vault | 0.8.1 | 0.8.2 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.12 | 10.0.14 |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.12 | 10.0.14 |
| Microsoft.Extensions.Logging.Abstractions | 10.0.12 | 10.0.14 |
| Microsoft.Extensions.Options.ConfigurationExtensions | 10.0.12 | 10.0.14 |

## [0.2.1] - 2026-09-26

### Changed

- Refresh stable NuGet dependencies; preserve target frameworks and HoneyDrunk public contracts.
- Exclude the unimplemented OpenAI, Anthropic, and AzureOpenAI provider slots from package publication. Their methods still throw `NotImplementedException`; implementing them is separate work.

| Dependency | Previous | Updated |
| --- | --- | --- |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.8 | 10.0.12 |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.8 | 10.0.12 |
| Microsoft.Extensions.Logging.Abstractions | 10.0.8 | 10.0.12 |
| Microsoft.Extensions.Options.ConfigurationExtensions | 10.0.8 | 10.0.12 |




### Verified HoneyDrunk dependencies

- HoneyDrunk.Kernel: 0.8.0 -> 0.8.1 (verified on NuGet.org).
- HoneyDrunk.Standards: 0.2.9 -> 0.3.0 (verified on NuGet.org).
- HoneyDrunk.Standards.Tests: 0.2.9 -> 0.3.0 (verified on NuGet.org).
- HoneyDrunk.Vault: 0.5.0 -> 0.8.1 (verified on NuGet.org).

## [Unreleased]

### Changed (breaking)

- **`HoneyDrunk.AI.Abstractions` contracts reorganized into domain sub-namespaces** (`Chat`/`Embeddings`/`Cost`/`Routing`/`Providers`) to match the runtime project's pattern of grouping types by domain folder. New namespaces: `HoneyDrunk.AI.Abstractions.Chat` (`ChatCompletion`, `ChatCompletionUpdate`, `ChatMessage`, `ChatOptions`, `ChatRequestSummary`, `ChatRole`, `IChatClient`); `HoneyDrunk.AI.Abstractions.Embeddings` (`Embedding`, `EmbeddingOptions`, `IEmbeddingGenerator`); `HoneyDrunk.AI.Abstractions.Cost` (`CostSummary`, `ICostLedger`, `InferenceCost`); `HoneyDrunk.AI.Abstractions.Routing` (`IModelRouter`, `IRoutingPolicy`, `ModelCandidate`, `RoutedModel`, `RoutingDecision`); `HoneyDrunk.AI.Abstractions.Providers` (`IModelProvider`, `ModelCapabilityDeclaration`). **Migration:** replace `using HoneyDrunk.AI.Abstractions;` with the specific sub-namespaces actually used. No external Grid consumers today, so the only callsites updated are the AI runtime + provider packages + tests in this repo.
- **Package versions bumped** to `HoneyDrunk.AI* 0.2.0` to reflect the breaking namespace shift per semver pre-1.0 (`0.x.0 → 0.(x+1).0` for breaks).

### Changed

- Triaged the initial SonarQube Cloud findings against AI (ADR-0011 D11 gate-cleanup). Moved workflow-level write permissions to job-level scopes on `nightly-security.yml`, `publish.yml`, and `weekly-deps.yml` so the `preflight` job runs with read-only access. Converted `InMemoryModelProvider`, `DefaultModelRouter`, `PolicyLoader`, and the `RuntimeTests.TestProvider` test double to primary constructors. Switched `DefaultCostLedger` list initializer and snapshot copy to C#12 collection expressions. Changed `InMemoryEmbeddingGenerator.BuildVector` return type from `IReadOnlyList<float>` to `float[]` for hot-path allocation/perf consistency with the surrounding `Embedding(value, float[], modelId)` constructor.
- Removed the `IDE0005` suppression from `Directory.Build.props` `NoWarn`. Unused `using` directives are now build errors (the project keeps `TreatWarningsAsErrors`), so contract-namespace imports stay precise instead of getting silently broad.
- Bumped `HoneyDrunk.Kernel` `0.7.0` → `0.8.0`. AI does not consume the affected static-class / `GridContextSnapshot` ctor surface, so no source changes required.
- Onboarded AI to SonarQube Cloud (ADR-0011 D11). Wired a `sonarcloud` job in `pr.yml` that calls `HoneyDrunkStudios/HoneyDrunk.Actions/.github/workflows/job-sonarcloud.yml` on both `pull_request` (after `pr-core` succeeds) and `push` to `main` (standalone), both gated by the existing `preflight` solution-detection. PR analysis gates the merge on new-code findings; main-branch analysis populates the SonarCloud Overview dashboard and the leak-period baseline. Per-project source/test classification is discovered automatically from MSBuild `IsTestProject` properties; per-repo Sonar overrides can be added later via `Directory.Build.props` `<SonarQubeSetting>` items or as new inputs to `job-sonarcloud.yml`. Branch-protection requirement added separately after the first successful run lands.
- Enabled ADR-0044 Grid Review request workflow and repo-local OpenClaw/Codex review configuration.

- Adopted HoneyDrunk.Standards.Tests 0.2.9 for AI test projects and refreshed HoneyDrunk.Standards to 0.2.9 across AI packages for ADR-0047 testing alignment.

## [0.1.0] - 2026-05-20

- Established the HoneyDrunk.AI solution, package layout, runtime, provider slots, deterministic InMemory provider, tests, documentation, and API compatibility workflow for ADR-0016.
