[← Back to README](../../README.md)

# CI/CD

## Pipelines

| Workflow | File | Trigger | What it does |
|---|---|---|---|
| Test Env Pipeline | `env-lower.yml` | push to `dev`, manual | build images, optionally deploy to `dev`/`test1`/`test2` |
| Main Env Pipeline | `env-main.yml` | release published, manual | build release images, optionally deploy to `main` |
| Build App | `build.yml` | called, manual | build and push frontend/backend/manager images to GHCR |
| Deploy App | `deploy.yml` | called, manual | SSH to the target, pull images, restart compose |
| Reset App | `reset.yml` | manual | wipe volumes and recreate the stack |
| Cleanup Old Container Images | `ghcr-cleanup.yml` | called, manual | keep the 10 newest versions of each package |

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
