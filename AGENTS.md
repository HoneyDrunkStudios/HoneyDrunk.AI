# HoneyDrunk.AI agent instructions

Own inference contracts, routing, cost accounting and provider slots. The external provider projects are currently non-packable stubs; the InMemory provider is test evidence, not proof of a real external integration. Do not start paid inference to validate documentation.

Start with [README.md](README.md) and the relevant source/tests. Project files and lockfiles own SDK, framework and dependency versions.

Read the [shared engineering conventions](https://github.com/HoneyDrunkStudios/HoneyDrunk.Standards/blob/main/HoneyDrunk.Standards/docs/CONVENTIONS.md) and this repository's owning documentation before editing. Apply the parts relevant to this stack; preserve existing public contracts, dependency direction and repository-specific behavior. Verify shared capabilities in current code before reusing them; a catalog entry or scaffold is not an implemented integration.

Work within the selected request. Preserve unrelated changes and use a separate worktree when needed. Review the final diff, use Conventional Commits and ready-for-review PRs with exactly one accurate `Authorship:` line and a `Request:` line; include the authorship in commit trailers. Run meaningful checks for the affected behavior and report the reviewed/tested revision, failures and unrun checks. For documentation-only changes, check links, paths and instruction consistency. Preserve required checks and inspect actual latest-head Sonar new-code findings where analysis applies; do not suppress findings or weaken gates to obtain a pass. Legacy Grid Review is retired; do not restore its workers, queues or bypass labels. A configured replacement reviewer is not evidence of a completed review or enforcing merge check.

## Verification

From the repository root for code/build changes:

```sh
dotnet restore HoneyDrunk.AI.slnx
dotnet build HoneyDrunk.AI.slnx -c Release --no-restore
dotnet test HoneyDrunk.AI.slnx -c Release --no-build
```

Use the checked-in workflow and relevant test documentation for additional integration prerequisites, coverage and consumer checks. Do not use live resources or credentials merely to make a local check pass.

## Code Review Rules

Apply the [shared review criteria](https://github.com/HoneyDrunkStudios/HoneyDrunk.Standards/blob/main/HoneyDrunk.Standards/docs/CONVENTIONS.md#code-review) to changed behavior, using the repository boundaries above. Report actionable findings with the failing path, concrete impact and a small corrective action; disclose unavailable evidence. These rules grant no cross-repository access or merge authority.

- Keep inference contracts, routing and cost accounting provider-neutral; SDK details stay in provider slots. Verify implemented providers before recommending reuse: external provider stubs and InMemory tests do not establish a working external integration.
- Trace untrusted prompts/output, tool permissions, tenant context and budget decisions through retries, cancellation and provider failure. Flag prompt-driven authority escalation, context/secret leakage, unbounded token/tool loops or retries that defeat a budget stop.
- Require routing, cost/usage, isolation and provider-failure contract tests for changed behavior. Separate deterministic test evidence from model-quality claims; do not invoke paid inference or invent a generic agent framework to complete review.
