# Spec: NuGet Trusted Publishing

- Status: Approved
- Requirements: [requirements.md](requirements.md)

## Overview

Replace only the authentication boundary in the existing deployment workflow.
The deploy job obtains a GitHub OIDC identity after packaging, exchanges it for
a short-lived nuget.org API key through `NuGet/login@v1`, and uses that output
for the existing push. Trust is configured separately on nuget.org.

Keep the current tag trigger, version derivation, packaging command, package
configuration, and feed. Do not introduce a GitHub environment, approval gate,
manual trigger, legacy-key fallback, or changes to duplicate-version handling.

The design follows the [NuGet trusted publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
and the [NuGet login action interface](https://github.com/NuGet/login).

## Affected areas

Paths below are relative to the repository root.

| Path | Evidence and intended impact |
| --- | --- |
| [.github/workflows/deploy.yaml](../../../.github/workflows/deploy.yaml) | Verified: owns the tag trigger, version extraction, pack command, and push using `secrets.NUGET`. Only job permissions, login, and credential consumption change. |
| [build/Targets.cs](../../../build/Targets.cs) | Verified: `Pack` builds the solution and packs matching non-test projects into `./artifacts` with the supplied release version. No change planned. |
| [src/Directory.Build.props](../../../src/Directory.Build.props) | Verified: shared package metadata and .NET 10 target. No change planned. |
| [src/tapir.Cli/tapir.Cli.csproj](../../../src/tapir.Cli/tapir.Cli.csproj) | Verified: tool packaging and explicit inclusion of the root README, license, and icon. No change planned. |
| [src/tapir.DevHost/tapir.DevHost.csproj](../../../src/tapir.DevHost/tapir.DevHost.csproj) | Verified: Web SDK project without an explicit `IsPackable` setting. Actual artifact production depends on SDK behavior and remains unverified. No change planned. |
| [docs/specs/001-nuget-trusted-publishing/requirements.md](requirements.md) | Verified: approved input. No change planned. |
| new: docs/NuGet-Publishing.md | Maintainer setup, policy values, failure diagnosis, live verification, and credential-retirement guidance; separate from the packaged README. |
| new: docs/specs/001-nuget-trusted-publishing/validation.md | Subsequent verification evidence, including explicit deferrals; not produced as part of this design. |

External configuration includes the repository's publishing-identity setting,
the nuget.org trust policy, and eventual retirement of the legacy credential.
These are not repository files and have not been inspected or configured.

## Design decisions

| Requirement | Decision | Reason |
| --- | --- | --- |
| FR-001 | Add `NuGet/login@v1` with step ID `login` immediately after successful packaging and before push. Supply the NuGet profile username through `secrets.NUGET_USER`. Consume only `steps.login.outputs.NUGET_API_KEY` for publication. | Uses the documented token exchange and avoids issuing the one-hour key before the build. The username setting is identity configuration, not a long-lived API key. |
| FR-002 | Leave `on.push.tags` as `v*.*.*` and retain the existing automatic job without a GitHub environment or approval gate. | Preserves current release eligibility and interaction. |
| FR-003 | Leave the current `RELEASE_VERSION` extraction and its use by `./build.sh pack -rv $RELEASE_VERSION` unchanged. | Keeps the established tag-to-version contract. |
| FR-004 | Keep the pack command, project metadata, `./artifacts/*.nupkg` selection, and `https://api.nuget.org/v3/index.json` push destination unchanged. Place maintainer guidance outside the packaged README. | Restricts the change to authentication without altering package contents or feed semantics. |
| FR-005 | Use ordinary successful-step sequencing: login must succeed before push runs. Do not use `continue-on-error`, an unconditional push condition, or any reference to the old key. | An authentication error terminates the publishing path instead of trying an alternate credential. |
| FR-006 | Document GitHub configuration access, NuGet profile identity, package-owner access, exact policy fields, allowed package scope, and how to verify the configured values. | Makes external setup explicit without guessing account-specific details. |
| FR-007 | Document the operational order: verify a live trusted release, inspect old-key dependencies, delete the repository `NUGET` secret, then revoke the former key on nuget.org. If dependencies exist, keep revocation pending until they are resolved. | Avoids premature retirement or disruption of other consumers. The new workflow never consumes the retained old secret. |
| FR-008 | Record each check with its result and evidence, and separately track deferred live publication, authentication-rejection execution, log inspection, and credential retirement. | Repository-only acceptance must not imply operational proof. |
| NFR-001 | Pass the temporary key to the push step through a step-local environment variable. Retain the action's masking before key output, prohibit debug tracing and credential printing, and do not persist tokens or keys as artifacts or repository content. | Limits credential lifetime and exposure. The inspected action source calls `core.setSecret` for OIDC tokens and the returned key before `core.setOutput`. Live log verification remains separate. |
| NFR-002 | Give only the deploy job `contents: read` and `id-token: write`. Configure the NuGet policy for owner `thomasduft`, repository `tapir`, workflow filename `deploy.yaml`, no environment, and the exact `tomware.tapir` package pattern with only the publishing scope needed for new versions. | Preserves checkout access, enables OIDC, and limits trust to the existing publishing identity and package. |

## Interfaces and data

### Workflow boundary

| Value | Source and handling |
| --- | --- |
| `RELEASE_VERSION` | Existing derivation from the tag, with its first character removed; unchanged. |
| `NUGET_USER` | Repository Actions secret containing the trust policy creator's nuget.org profile username, not an email address, API key, or presumed organization name. Maintainers confirm it with the policy creator. |
| OIDC token | Requested by the login action using job-scoped permission; never stored as a repository secret. |
| `NUGET_API_KEY` | Login output; mapped to a push-step environment variable of the same name and expanded as a quoted shell argument for `dotnet nuget push`. Not placed in global job environment or persisted. |
| `NUGET` | Legacy secret; no references in the new publishing path. Retained externally only until the retirement conditions are met. |
| Login defaults | Use the action defaults: audience `https://www.nuget.org` and token service `https://www.nuget.org/api/v2/token`. These are distinct from the unchanged package-feed URL. |

The login action metadata at the inspected `v1` reference declares the required
`user` input, the `NUGET_API_KEY` output, and a Node 24 runtime. Retain the current
`ubuntu-latest` runner and existing checkout and .NET setup action versions.
No application interface, package schema, or test-data format changes.

### External trust and operations

The maintainer guide must distinguish the login username from the selected
NuGet policy owner. The inspected action's authentication error guidance specifies
the policy creator's username, not the policy owner's name. An organization-owned policy requires appropriate membership
and package publishing rights. The actual account and policy owner remain
unresolved; neither is inferred from the repository owner.

The policy uses the filename `deploy.yaml`, not its repository-relative path.
The environment field is empty because this design adds no environment. Package
authorization uses `tomware.tapir` without a wildcard; access to unrelated
packages or non-publishing operations is not needed. Maintainers verify these
values in the policy UI before enabling a release.

The guide must explain that an inactive or mismatched policy, missing username,
or insufficient package-owner access stops login or publication. Troubleshooting
must not introduce a legacy-key fallback or ask maintainers to print tokens.
If the policy is pending full activation, maintainers must follow the activation
window shown by nuget.org before relying on it for a live release.

A first live verification uses a legitimate, unused release version through the
existing tag path, not a new dry-run publication trigger. Evidence records the
workflow run, release tag, observed authentication and push results, and package
identity/version visible on nuget.org. It also confirms the publishing path did
not consume the old secret; that secret may remain stored until verification.
Only then may its removal and key revocation proceed. Shared-key dependencies
keep revocation pending rather than changing the scope of this feature.

### Acceptance evidence

This table defines verification methods, not checks already executed.

| Criteria | Repository-level evidence | Live evidence or limitation |
| --- | --- | --- |
| AC-001 | Inspect login inputs, output consumption, and absence of legacy-key references. | Successful authorized tagged publication with no long-lived-key dependency; may be deferred. Static inspection alone does not pass the live criterion. |
| AC-002 | Compare trigger and approval configuration with the existing workflow. | No separate live release needed for configuration comparison. |
| AC-003 | Exercise the preserved version-extraction expression with representative matching tags, including a prerelease tag, and inspect its pack input. | Does not establish NuGet acceptance of every tag matched by the pre-existing glob. |
| AC-004 | Compare pack command, package definition, artifact selection, and destination before and after the change. | Package metadata/content inspection can supplement comparison; a live publish is not required by this criterion. |
| AC-005 | Inspect success-only sequencing and the absence of fallback references. | Rejected-authentication execution must show failed login and skipped push; may be deferred rather than simulated by publishing a package. |
| AC-006 | Review the maintainer guide against NuGet guidance and the specified configuration contract. | Actual account access and policy configuration require maintainer confirmation. |
| AC-007 | Review that both retirement actions follow live verification and dependency assessment in the guide. | Actual retirement is pending until its operational conditions are satisfied. |
| AC-008 | Review the verification record for check results, evidence, and explicit deferred/pending entries. | No unperformed check is reported as successful. |
| AC-009 | Inspect changed content for credential values and unsafe logging; review the action's masking behavior. | Inspect available release logs for exposure without copying secrets into evidence; mark unavailable log evidence deferred. |
| AC-010 | Compare documented policy fields and exact package scope against this design. | Confirm actual nuget.org policy values separately; documented values do not prove external configuration. |

## Risks

- Trust grants identity to the named workflow, not an independent tag-only
  authorization rule. Preserving its tag-only trigger and protecting repository
  and workflow write access remain important security boundaries.
- Missing identity configuration, incorrect ownership, inactive policies, or
  mismatched policy fields can block a legitimate release. There is intentionally
  no automatic fallback.
- The existing pack selector matches non-test project paths containing `tapir`.
  The resulting artifact IDs have not been verified. Additional package IDs
  would conflict with an exact single-package policy; do not broaden trust to
  conceal that mismatch.
- `NuGet/login@v1` is a mutable major-version reference, consistent with the
  existing workflow's action-reference style. Upstream changes can affect
  runtime behavior and masking; review the selected action when implementing.
- A shared old key cannot be revoked safely until its other consumers have been
  resolved. Local secret removal alone does not revoke that key.
- A repository-only verification record cannot establish that external trust,
  live publishing, or log masking works in the deployed workflow.

## Assumptions

- Maintainers can obtain the required repository and package-owner access;
  this has not been verified.
- The runner can execute the inspected login action's runtime and reach GitHub
  OIDC and nuget.org services.
- The action's executed distribution matches its inspected source. The source
  masks known tokens and keys before output; live logs and the bundled runtime
  distribution have not been inspected. Token-service error response text can
  appear in failure logs, so live failure-log verification remains necessary.
- The approved allowance for deferred live evidence also permits operational
  account configuration and credential retirement to remain pending when clearly
  recorded.

## Open questions

- Which NuGet profile username and individual or organization policy owner will
  maintainers configure?
- Does the former API key serve other packages or workflows, and who owns any
  prerequisite migrations?
- Who will verify the first live release and perform credential retirement?

## Review findings

- RF-001: The approved account-ownership and operational-responsibility questions
  remain unanswered. The design isolates them as external configuration, but
  they must be resolved before live rollout.
- RF-002 (resolved at source-review level): The inspected
  [login action source](https://raw.githubusercontent.com/NuGet/login/v1/src/index.ts)
  masks OIDC tokens and the returned API key before output and reports failures
  with `core.setFailed`. This supports the proposed masking design but does not
  replace live-log inspection for AC-009; error-response content is logged and
  remains an external-service dependency.
- RF-003: Confirm the existing artifact set contains only package IDs authorized
  by NFR-002 before rollout. If additional publishable IDs are intentional, ask
  for clarification of the approved scope rather than changing packaging or
  widening the trust policy implicitly.
- RF-004: AC-001 requires successful live publication while repository-only
  acceptance is explicitly permitted. Treat it as deferred operational evidence,
  not a passed check; apply the same distinction to live portions of AC-005 and
  AC-009. No requirements changes were made.
- Review coverage: every FR/NFR has a decision row and every AC has a verification
  method above. Affected existing paths were read; proposed paths are marked
  new. No workflow execution, package build, account configuration, credential
  inspection, or live publication was performed for this specification.
