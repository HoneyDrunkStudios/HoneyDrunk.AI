# Changelog

## [Unreleased]

### Changed

- Onboarded AI to SonarQube Cloud (ADR-0011 D11). Wired a `sonarcloud` job in `pr.yml` that calls `HoneyDrunkStudios/HoneyDrunk.Actions/.github/workflows/job-sonarcloud.yml` on both `pull_request` (after `pr-core` succeeds) and `push` to `main` (standalone), both gated by the existing `preflight` solution-detection. PR analysis gates the merge on new-code findings; main-branch analysis populates the SonarCloud Overview dashboard and the leak-period baseline. Per-project source/test classification is discovered automatically from MSBuild `IsTestProject` properties; per-repo Sonar overrides can be added later via `Directory.Build.props` `<SonarQubeSetting>` items or as new inputs to `job-sonarcloud.yml`. Branch-protection requirement added separately after the first successful run lands.
- Enabled ADR-0044 Grid Review request workflow and repo-local OpenClaw/Codex review configuration.

- Adopted HoneyDrunk.Standards.Tests 0.2.9 for AI test projects and refreshed HoneyDrunk.Standards to 0.2.9 across AI packages for ADR-0047 testing alignment.

## [0.1.0] - 2026-05-20

- Established the HoneyDrunk.AI solution, package layout, runtime, provider slots, deterministic InMemory provider, tests, documentation, and API compatibility workflow for ADR-0016.
