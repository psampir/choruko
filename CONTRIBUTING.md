# Contributing to Choruko

Thanks for taking the time to contribute. This document covers the branch flow, the
checks we expect, and a few project-specific details.

By participating you agree to the [Code of Conduct](CODE_OF_CONDUCT.md).

## Branch flow

```
feature branch ──► dev ──► main
```

- `main` is the stable branch and only receives merges from `dev`.
- `dev` is the integration branch. Pull requests target `dev`.
- Work happens on a feature branch. **Never commit directly to `main`**, and avoid
  committing directly to `dev` as well.

Name the branch after the issue or the change:

- `224-non-admin-participant-tasks`
- `add-settlement-tests`
- `fix-login-cookie`

## Development setup

Follow [docs/guides/set-up-dev-env.md](docs/guides/set-up-dev-env.md). It covers
Docker, .NET 9, Node, Git LFS, the development database and migrations.

The repository stores PNG images with Git LFS. Install it before cloning, or run
`git lfs pull` afterwards.

## Checks

Run these before opening a pull request:

```bash
dotnet test backend/ChorukoAPI.sln

npm ci --prefix frontend/ChorukoWeb
npm run lint --prefix frontend/ChorukoWeb
npm run build --prefix frontend/ChorukoWeb
```

CI runs the same commands (`.github/workflows/ci.yml`) and must be green.

## Database changes

If you change an entity or `DataContext`:

```bash
dotnet ef migrations add DescriptiveName \
  --project backend/ChorukoAPI --startup-project backend/ChorukoAPI

python3 backend/update-db.py
```

- Never edit a migration that has already been applied; add a new one.
- Commit the generated migration and the updated model snapshot.
- Mention the migration in the pull request description.

## Commit messages

Use the imperative mood and a short prefix:

- `feat:` new behaviour
- `fix:` bug fix
- `chore:` maintenance
- `docs:` documentation
- `ci:` workflows and deployment
- `test:` tests only

Keep the subject under ~72 characters. Explain *why* in the body when it is not
obvious.

## Pull requests

- Target `dev`.
- Keep the change focused; split unrelated work into separate pull requests.
- Fill in the pull request template.
- Link the issue (`Closes #123`) when there is one.
- Update documentation when behaviour or configuration changes.
- Do not include secrets, `.env` files, database dumps or build artifacts.

## Security

Do not open a public issue for a vulnerability. See [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your contribution is licensed under the
AGPL-3.0-or-later, the same license as the project.
