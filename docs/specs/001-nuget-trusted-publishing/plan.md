# Plan: NuGet Trusted Publishing

- Status: Approved
- Branch: feat/nuget-trusted-publishing
- Verification commands: `./build.sh test`; `./build.sh pack -rv <version>` (release workflow command)

## Tasks

### T-001: Switching the release workflow to trusted publishing
- [x] Done
- Covers: FR-001, FR-002, FR-003, FR-004, FR-005, NFR-001, NFR-002, AC-001, AC-002, AC-003, AC-004, AC-005, AC-009, AC-010
- Spec: Overview; Design decisions; Workflow boundary; Acceptance evidence
- Depends on: None
- Change: Add deploy-job-scoped OIDC permissions and authenticate with `NuGet/login@v1` after packaging, passing only its short-lived output to the existing push. Preserve the tag trigger, version derivation, packaging, package selection, feed, and automatic release behavior; do not add a legacy-key fallback or expose credentials in logs or persisted data.
- Files: [deploy.yaml](../../../.github/workflows/deploy.yaml)
- Done when: Workflow review confirms the existing `v*.*.*` trigger, version expression, pack command and NuGet destination are unchanged; login succeeds before push; push consumes only the login output; no `secrets.NUGET` or credential-printing/debugging path remains; permissions are limited to the deploy job and its stated needs.
- Notes: Workflow review passed; `./build.sh test` passed (257 tests) and `./build.sh pack -rv 0.0.0` succeeded. Live trusted publication, rejected-authentication execution, and release-log inspection remain deferred.

### T-002: Documenting maintainer setup and credential retirement
- [ ] Done
- Covers: FR-006, FR-007, NFR-002, AC-006, AC-007, AC-010
- Spec: External trust and operations; Design decisions
- Depends on: T-001
- Change: Add maintainer instructions for required GitHub/NuGet access, the login username, repository/workflow/environment policy fields, exact package scope, policy verification, and failure diagnosis. Document live-release verification and dependency assessment before deleting the `NUGET` secret and revoking the former key; make unresolved external account details explicit rather than assuming them.
- Files: new: docs/NuGet-Publishing.md
- Done when: Guide review confirms every FR-006 prerequisite and repository-specific value is explained; the documented trust scope is limited to the intended `thomasduft/tapir` `deploy.yaml` workflow and `tomware.tapir`; credential retirement follows successful live verification and dependency resolution.
- Notes: Keep the guide separate from the packaged README. Actual account configuration, live release, and credential retirement are external maintainer actions.

### T-003: Recording verification evidence and deferrals
- [ ] Done
- Covers: FR-008, AC-001, AC-005, AC-008, AC-009
- Spec: Acceptance evidence; Risks; Assumptions
- Depends on: T-001, T-002
- Change: Add a verification record with the result and evidence for each repository-level check actually performed. Mark successful live publication, rejected-authentication execution, and live-log inspection as unverified/deferred when unavailable, and keep credential retirement pending until its documented conditions are met.
- Files: new: docs/specs/001-nuget-trusted-publishing/validation.md
- Done when: The record distinguishes performed checks from unperformed operational checks, includes evidence for each performed check, explicitly marks live publication and any unavailable log/rejection evidence as deferred, and records credential retirement as pending unless a successful live release and dependency review have been verified.
- Notes: Run the documented test and pack commands where appropriate; repository-only checks must not be presented as proof of live publication.

## Coverage
| Requirement | Tasks |
| --- | --- |
| FR-001 | T-001 |
| FR-002 | T-001 |
| FR-003 | T-001 |
| FR-004 | T-001 |
| FR-005 | T-001 |
| FR-006 | T-002 |
| FR-007 | T-002 |
| FR-008 | T-003 |
| NFR-001 | T-001 |
| NFR-002 | T-001, T-002 |
| AC-001 | T-001, T-003 |
| AC-002 | T-001 |
| AC-003 | T-001 |
| AC-004 | T-001 |
| AC-005 | T-001, T-003 |
| AC-006 | T-002 |
| AC-007 | T-002 |
| AC-008 | T-003 |
| AC-009 | T-001, T-003 |
| AC-010 | T-001, T-002 |

## Open questions
- Which NuGet profile username and individual or organization policy owner will maintain the trust policy?
- Does the former API key serve other packages or workflows, and who owns any prerequisite migrations?
- Who will verify the first live release and perform credential retirement?
