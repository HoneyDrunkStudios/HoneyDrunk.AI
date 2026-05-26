# Changelog

## [Unreleased]

### Changed (breaking)

- **`HoneyDrunk.AI.Abstractions` contracts reorganized into domain sub-namespaces** to mirror the runtime project's `Cost`/`Routing`/`Telemetry` layout. New namespaces: `HoneyDrunk.AI.Abstractions.Chat` (`ChatCompletion`, `ChatCompletionUpdate`, `ChatMessage`, `ChatOptions`, `ChatRequestSummary`, `ChatRole`, `IChatClient`); `HoneyDrunk.AI.Abstractions.Embeddings` (`Embedding`, `EmbeddingOptions`, `IEmbeddingGenerator`); `HoneyDrunk.AI.Abstractions.Cost` (`CostSummary`, `ICostLedger`, `InferenceCost`); `HoneyDrunk.AI.Abstractions.Routing` (`IModelRouter`, `IRoutingPolicy`, `ModelCandidate`, `RoutedModel`, `RoutingDecision`); `HoneyDrunk.AI.Abstractions.Providers` (`IModelProvider`, `ModelCapabilityDeclaration`). **Migration:** replace `using HoneyDrunk.AI.Abstractions;` with the specific sub-namespaces actually used. No external Grid consumers today, so the only callsites updated are the AI runtime + provider packages + tests in this repo.
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
