# Validation: NuGet Trusted Publishing

- Date: 2026-11-04
- Branch: `feat/nuget-trusted-publishing`
- Scope: Repository-level checks only. These results do not establish that
  nuget.org trust is configured or that a live release can publish.

## Checks performed

| Check | Result | Evidence |
| --- | --- | --- |
| `./build.sh test` | Passed | Build succeeded with 0 warnings and 0 errors; 257 tests passed, 0 failed, 0 skipped. |
| `./build.sh pack -rv 0.0.0` | Passed with warning | Created `artifacts/tomware.Tapir.0.0.0.nupkg`. The DevHost project emitted its existing warning that packing is disabled; the command succeeded. |
| Workflow review | Passed at repository level | `.github/workflows/deploy.yaml` retains the `v*.*.*` tag trigger, version derivation, pack command and nuget.org push destination. Deploy-job permissions are `contents: read` and `id-token: write`; login is before push; push consumes the login output. No reference to the legacy `secrets.NUGET` key, explicit token-printing, or debug-tracing path was found. |
| Maintainer guide review | Passed at repository level | `docs/NuGet-Publishing.md` documents the repository/workflow policy values, exact `tomware.tapir` scope, `NUGET_USER`, policy verification, troubleshooting, and retirement conditions. NuGet account ownership and configured values remain unverified. |
| `git diff --check` | Passed | No whitespace errors in the task changes. |

## Acceptance evidence and operational deferrals

| Criterion | Status | Evidence or limitation |
| --- | --- | --- |
| AC-001: successful trusted publication | **Unverified / deferred** | No live tagged release or nuget.org package publication was performed. |
| AC-005: rejected authentication stops publication | **Repository review passed; live execution deferred** | Workflow sequencing has login before push and contains no legacy-key fallback. No intentionally rejected authentication run was performed. |
| AC-008: accurate repository-only record | **Recorded** | Commands actually run and their outcomes are listed above; unperformed operational checks are explicitly deferred below. |
| AC-009: credential exposure | **Repository review passed; live-log inspection deferred** | The reviewed workflow contains secret/output references but no plaintext credential value or explicit credential-printing/debugging path. No live release logs were available to inspect. |

- GitHub Actions and nuget.org account configuration: **unverified**.
- Policy owner and NuGet profile username: **not confirmed**.
- Live authentication, package publication, and live release-log inspection:
  **unverified / deferred**.
- Former-key dependency review: **not performed**.
- Removal of the `NUGET` repository secret and revocation of the former key:
  **pending** until a successful trusted-publishing release and dependency
  assessment are verified.
