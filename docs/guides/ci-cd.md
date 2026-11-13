[← Back to README](../../README.md)

# CI/CD

## Workflows

| Workflow | File | Trigger | What it does |
|---|---|---|---|
| CI | `ci.yml` | pull requests, push to `dev`/`main`, manual | backend restore/build/test, frontend `npm ci`/lint/build |
| Test Env Pipeline | `env-lower.yml` | push to `dev`, manual | build images, optionally deploy, clean old images |
| Main Env Pipeline | `env-main.yml` | release published, manual | build release images, optionally deploy, clean old images |
| Build App | `build.yml` | called, manual | build and push frontend/backend/manager images to GHCR |
| Deploy App | `deploy.yml` | called, manual | SSH to the target, pull images, restart compose |
| Reset App | `reset.yml` | manual | wipe volumes and recreate the stack |
| Cleanup Old Container Images | `ghcr-cleanup.yml` | called, manual | keep the 10 newest versions of each package |
| CodeQL | `codeql.yml` | manual (public repo) | static analysis for C# and TypeScript |

## Pull request checks

`CI` runs on every pull request and on pushes to `dev` and `main`:

- **Backend** — `dotnet restore`, `dotnet build`, `dotnet test` on
  `backend/ChorukoAPI.sln` (.NET 9). Tests use EF Core InMemory, so no database is
  needed.
- **Frontend** — `npm ci`, `npm run lint`, `npm run build` in `frontend/ChorukoWeb`
  (Node 22).

The workflow uses `permissions: contents: read` and no secrets, so it is safe for
pull requests from forks. Once the repository is public, make `CI` a required
status check for `dev` and `main`.

## Deploy is opt-in

Deploy jobs are guarded by the repository variable `DEPLOY_ENABLED`:

```yaml
if: ${{ vars.DEPLOY_ENABLED == 'true' }}
```

It is set to `false` while no deploy target exists. Runs stay green in that state:
images are built and pushed, deploy is skipped, and image cleanup still runs
(it is gated on a successful build, not on deploy).

To re-enable deployment:

1. Add the repository secrets `SERVER_IP_ADDRESS`, `SSH_PRIVATE_KEY` and `CR_PAT`.
2. Set the variable:

   ```bash
   gh variable set DEPLOY_ENABLED --body true
   ```

3. Push to `dev` or run the pipeline manually.

To disable deployment again:

```bash
gh variable set DEPLOY_ENABLED --body false
```

If the target is gone for good, also remove the three secrets.

## Image tags

- Non-release builds push `latest` and `<branch>-<timestamp>`.
- Release builds push `latest`, `release` and the release tag.

Because non-release builds also update `latest`, `latest` may point at a `dev`
build. Use the `release` tag (the default in `docker/compose/example.env`) for
stable deployments.

## Repository settings for a public repository

- **Branch protection** for `main` and `dev`: require a pull request and the `CI`
  check, disallow direct pushes. No required approvals is fine for a solo
  maintainer.
- **Secret scanning** and **push protection**.
- **Dependabot alerts**; version updates are configured in
  `.github/dependabot.yml` for NuGet, npm, GitHub Actions and Docker.
- **CodeQL** — enable the `push`/`pull_request` triggers in `.github/workflows/codeql.yml`
  after the repository is public; CodeQL is not available on private repositories
  without GitHub Advanced Security.
- **GHCR packages** — set `choruko-frontend`, `choruko-backend` and
  `choruko-manager` to public, otherwise `docker pull` from the README quick start
  will fail for anonymous users.
