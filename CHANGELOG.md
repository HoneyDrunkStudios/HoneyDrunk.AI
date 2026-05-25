# Changelog

## [Unreleased]

### Changed

- Onboarded AI to SonarQube Cloud (ADR-0011 D11). Added `sonar-project.properties` at the repo root and wired a `sonarcloud` job in `pr.yml` that calls `HoneyDrunk.Actions/.github/workflows/job-sonarcloud.yml` after `pr-core`, gated by the existing `preflight` solution-detection. Sources cover `src/` (runtime + Abstractions + four providers: Anthropic, AzureOpenAI, InMemory, OpenAI); tests cover `tests/` (Abstractions.Tests, InMemory.Tests, main Tests). Abstractions excluded from coverage. Branch-protection requirement added separately after the first successful run lands.
- Enabled ADR-0044 Grid Review request workflow and repo-local OpenClaw/Codex review configuration.

- Adopted HoneyDrunk.Standards.Tests 0.2.9 for AI test projects and refreshed HoneyDrunk.Standards to 0.2.9 across AI packages for ADR-0047 testing alignment.

## [0.1.0] - 2026-05-20

- Established the HoneyDrunk.AI solution, package layout, runtime, provider slots, deterministic InMemory provider, tests, documentation, and API compatibility workflow for ADR-0016.
