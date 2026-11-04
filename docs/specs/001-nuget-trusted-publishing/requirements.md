# Requirements: NuGet Trusted Publishing

- Status: Approved
- Date: 2026-11-04

## Problem

The release workflow currently publishes `tomware.tapir` to nuget.org using the
long-lived `NUGET` repository secret. Repository maintainers must store and manage
this credential, including its rotation and revocation. Trusted publishing should
remove that dependency without changing how maintainers release the package or
how users obtain it.

The current release workflow runs on pushed tags matching `v*.*.*`, derives the
package version by removing the leading `v`, and publishes automatically without
a separate approval step. The feature serves repository maintainers; package
consumers should observe no change in package identity or distribution.

Reference: [NuGet trusted publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## Goals

- Publish releases using trusted publishing without a stored long-lived NuGet API key.
- Preserve existing release behavior and package compatibility for consumers.
- Give maintainers sufficient instructions to configure trust and retire the old credential.
- Allow repository-level acceptance with explicitly deferred live-publish verification.

## Non-goals

- Changing the release trigger, version derivation, or approval requirements.
- Changing package contents, package identity, or the nuget.org destination.
- Adding another package feed or publishing provider.
- Changing duplicate-version handling or introducing new release channels.
- Performing account administration or publishing a live release during requirements work.

## Functional requirements

- FR-001: Package publication must authenticate through nuget.org trusted publishing without depending on a stored long-lived NuGet API key.
- FR-002: Pushed tags matching the existing `v*.*.*` release trigger must continue to initiate publication automatically without an additional maintainer approval step.
- FR-003: Release package versions must continue to be derived by removing the leading `v` from the triggering tag.
- FR-004: Releases must continue to publish the existing `tomware.tapir` package to nuget.org without changing its contents except for normal release-dependent values.
- FR-005: A trusted-publishing authentication failure must fail the release without attempting publication using a long-lived credential.
- FR-006: Maintainer instructions must identify the account access, publishing identity, repository and release identity, package authorization, and trust configuration needed to enable publication.
- FR-007: Maintainer instructions must describe removal of the stored `NUGET` secret and revocation of the former long-lived key only after a successful trusted-publishing release has been verified.
- FR-008: The verification record must distinguish repository checks actually performed from live-publishing verification that remains deferred.

## Non-functional requirements

- NFR-001: Publishing credentials must not appear in plaintext in repository content or release logs.
- NFR-002: The documented trust authorization must be limited to the intended release workflow in `thomasduft/tapir` and the existing `tomware.tapir` package.

## Acceptance criteria

- AC-001 (FR-001): Given valid trust configuration and no stored long-lived NuGet API key, When an eligible release runs, Then it authenticates through trusted publishing and nuget.org accepts the package publication.
- AC-002 (FR-002): Given the release trigger configuration, When it is compared with the current configuration, Then it still selects pushed tags matching `v*.*.*` and introduces no additional maintainer approval requirement.
- AC-003 (FR-003): Given an eligible tag `vX.Y.Z`, When the release package version is determined, Then it is `X.Y.Z` with the leading `v` removed.
- AC-004 (FR-004): Given the same source revision and release version, When the package definition and publishing destination are compared before and after the authentication change, Then package identity, packaged content rules, and the nuget.org destination are unchanged.
- AC-005 (FR-005): Given trusted-publishing authentication is rejected, When the release executes, Then the release fails without attempting package publication or falling back to the former long-lived key.
- AC-006 (FR-006): Given a maintainer preparing to enable trusted publishing, When the setup instructions are reviewed against the referenced NuGet guidance, Then they identify each prerequisite in FR-006, explain where repository-specific values are obtained, and identify any setup that must be completed outside the repository.
- AC-007 (FR-007): Given the credential-retirement instructions, When their sequence is reviewed, Then verification of a successful live trusted-publishing release precedes removal of the `NUGET` secret and revocation of the old key, and both retirement actions are included.
- AC-008 (FR-008): Given repository-only verification is used, When its results are recorded, Then each performed check has a result, live publication is explicitly marked unverified, and credential retirement is recorded as pending until a successful live release is verified.
- AC-009 (NFR-001): Given the proposed repository changes and available release logs, When they are inspected for publishing credentials, Then no plaintext credential value is present; if no live logs are available, log verification is explicitly marked deferred.
- AC-010 (NFR-002): Given the documented trust authorization, When its permitted publishing identity and package scope are reviewed, Then it authorizes the intended release workflow in `thomasduft/tapir` for `tomware.tapir` and does not authorize unrelated workflows, repositories, or packages.

## Assumptions

- Repository maintainers can arrange the required GitHub and nuget.org account administration; actual access has not been verified.
- Authentication failure should stop publication without a legacy-key fallback; this security behavior remains subject to review of this draft.
- Repository-level checks may support acceptance without a live release, as requested. They do not prove successful publication. AC-001 and live execution evidence for AC-005 and AC-009 may remain explicitly deferred.
- Removing the repository secret and revoking the old key are operational follow-ups, not prerequisites for repository-level acceptance; neither should occur before a verified successful trusted-publishing release.

## Open questions

- Which nuget.org publishing account will be used, and will the trust policy be owned by an individual or an organization?
- Does the former long-lived key serve other publishing workflows or packages that must be migrated before it can be revoked?
- Who will perform the first live-release verification and the subsequent credential retirement?
