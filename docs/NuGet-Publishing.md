# Publishing to NuGet.org

The `deploy` workflow publishes `tomware.tapir` to nuget.org using trusted
publishing. It runs automatically for pushed tags matching `v*.*.*`; the
package version is the tag without its leading `v`. The workflow packs first,
then exchanges its GitHub Actions OIDC identity for a short-lived NuGet API key
with [`NuGet/login@v1`](../.github/workflows/deploy.yaml). It does not use the
former long-lived `NUGET` key.

## Access and account details

Before configuring trusted publishing, coordinate with maintainers who can:

- Update Actions secrets and workflow settings for the
  [`thomasduft/tapir` repository](https://github.com/thomasduft/tapir), and
  create an authorized release tag when live verification is scheduled.
- Manage the nuget.org account that owns `tomware.tapir` and create or update
  that account's trusted-publishing policy.
- Publish package versions for the selected NuGet account. If the policy is
  organization-owned, its creator must be an active organization member with
  the required package permissions.

The policy owner (the individual user or organization that owns the package)
and the NuGet profile username used by the workflow are separate values. The
repository does not establish which individual or organization owns the
package, who will create the policy, or who will maintain it. Confirm these
details with the package owners; do not assume the GitHub repository owner is
the NuGet owner.

## Configure trusted publishing

1. Sign in to nuget.org using the account that will own the trusted-publishing
   policy. Open **Trusted Publishing** from the account menu and create a
   policy. Select the individual or organization that owns `tomware.tapir`.
2. Set the GitHub policy fields to these repository-specific values:

   The owner and repository values come from the GitHub repository URL. The
   workflow value comes from the workflow file under `.github/workflows/`.

   | Policy field | Value |
   | --- | --- |
   | Repository owner | `thomasduft` |
   | Repository | `tapir` |
   | Workflow file | `deploy.yaml` |
   | Environment | Leave empty; the workflow does not use a GitHub environment. |

   The workflow field is the file name only, not `.github/workflows/deploy.yaml`.
   NuGet treats these values as case-insensitive.
3. Restrict package authorization to the exact `tomware.tapir` package pattern;
   do not use a wildcard or authorize unrelated packages. Grant only the scope
   needed to publish new versions of this existing package. Confirm the ID
   against the existing nuget.org package listing and release package; do not
   grant unrelated package-management scopes.
4. In the GitHub repository, add an Actions **repository secret** named
   `NUGET_USER`. Its value must be the NuGet profile username of the person
   who created the policy, not an email address, organization name, or API key.
   Add it under **Settings > Secrets and variables > Actions > Repository
   secrets**, and confirm the exact username with that person. The secret
   supplies the login action's `user` input; it is not a publishing credential.
5. Before relying on the policy, review it in the nuget.org account. Confirm the
   owner, repository, workflow filename, empty environment, exact package
   pattern, and narrow publishing scope. Check that the policy is active and
   that the owner still has the required access.

Some policies may be temporarily active for seven days while nuget.org waits
for a successful publish to bind the GitHub repository and owner IDs. If the
nuget.org UI reports this status, complete the first eligible publish within
the displayed window or restart the window as directed there. A successful
publish permanently activates the policy.

## Diagnose setup failures

- **Login fails:** Confirm `NUGET_USER` exists as a repository Actions secret
  and matches the policy creator's NuGet profile username. Check that the
  policy is active and that its owner, repository, workflow file, and empty
  environment match the table above. The deploy job must retain its OIDC
  permission.
- **The policy is inactive or mismatched:** Check its status and warnings in
  nuget.org. For an organization-owned policy, confirm the creator is still an
  active member and that the organization and package owner are active.
- **Login succeeds but push is rejected:** Confirm that the selected policy
  owner owns `tomware.tapir`, that its package pattern matches that exact ID,
  and that its scope allows publishing new versions.
- **A release fails:** The push follows the login step, so failed login prevents
  the push. Correct the trust configuration and retry through the existing
  eligible tag workflow. There is no long-lived-key fallback.

Never print, enable debug tracing for, or copy OIDC tokens or API keys into
logs, issues, or other persisted data. Share sanitized error details only.

## Verify a live release and retire the former key

Repository configuration and a successful build do not prove trusted
publication works. Coordinate an eligible release using an unused version
through the existing tag workflow; do not add a separate publishing trigger.
Verify that the workflow's login and push steps succeed and that the expected
`tomware.tapir` version appears on nuget.org. Retain the run and package
verification details without copying credentials. If this live verification
has not happened, keep credential retirement pending.

After a successful trusted-publishing release, review every known consumer of
the former `NUGET` secret and API key, including other workflows and
publishing automation. Migrate or remove those dependencies first. Then:

1. Remove the repository's `NUGET` Actions secret if no remaining workflow
   needs it.
2. Revoke the former API key in nuget.org only after all of its other
   dependencies have been resolved.

If either the successful release or dependency review is incomplete, do not
perform the corresponding retirement action. Removing a repository secret does
not revoke the key, and revoking a shared key can break other publishers.

## References

- [NuGet trusted publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- [NuGet login action](https://github.com/NuGet/login)
