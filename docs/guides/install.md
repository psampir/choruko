[← Back to README](../../README.md)

# Installing Choruko

Choruko runs as four Docker containers: a web frontend, the API backend, a short-lived
manager that applies database migrations and provides admin tools, and PostgreSQL.

## Requirements

- Docker with the Compose plugin
- About 1 GB of free disk space for images and data

Plain HTTP is enough for a trusted LAN: notifications go out through ntfy and do not
need HTTPS here.

## Step 1 — download the deployment files

```bash
mkdir choruko && cd choruko

curl -fsSLO https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/prod/docker-compose.yml
curl -fsSL -o .env https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/example.env
```

## Step 2 — configure

Edit `.env`. At minimum, change the database password:

```dotenv
DB_PASSWORD=change-me-to-a-long-alphanumeric-password
```

Use only `A-Z`, `a-z`, `0-9`. The full list of variables is in
[configuration](configuration.md).

Set `CHORUKO_PORT` if port `2467` is taken. Set `SERVER_URL` only if you enable email;
it must be the public URL of the instance.

## Step 3 — start

```bash
docker compose up -d
```

The manager applies migrations first; the backend waits for it. Watch the startup:

```bash
docker compose logs -f manager backend
```

Open `http://localhost:2467` (or `http://<server-ip>:2467`).

## Step 4 — first account and household

1. Register the first account.
2. Create a household. The creator becomes the household admin and five default
   activities are created.
3. Invite other people by email. They must already have an account; the invitation
   appears in their `/invitations` page.
4. Optionally configure SMTP later so users can reset their own passwords.

## Mobile

Users can open the instance in a phone browser and add it to the home screen on iOS.
On Android there is also an APK on
[GitHub Releases](https://github.com/psampir/choruko/releases): it asks for the
instance address on first launch, so the same APK works with every deployment. See
[the Android app guide](android-app.md).

Notifications do not need a mobile app: they arrive through the ntfy app, which each
user subscribes to with the topic shown in **Manage Account → Notifications**.

## Optional: HTTPS

The containers speak plain HTTP on `${CHORUKO_PORT}`. On a trusted LAN that is
enough. If the instance is reachable from other machines, put any reverse proxy with
a trusted certificate in front of it, forward everything to that port and keep the
original `Host` header; never expose the backend directly. Otherwise passwords and
the session cookie travel in clear text, and the clipboard API behind **Copy Topic**
is unavailable.

## Reset a password

When email is disabled, or a user is locked out, run the interactive tool in the
manager container:

```bash
docker exec -it choruko_mgr reset-password
```

It asks for the user's email and prints a new password. Share it over a secure
channel.

## Upgrade

```bash
# back up first — see backup-restore.md
docker exec choruko_db pg_dump -U postgres -d choruko -Fc > "pre-upgrade-$(date +%F).dump"

docker compose pull
docker compose up -d
docker compose logs -f manager backend
```

`CHORUKO_VERSION=release` tracks the newest stable release. Pin a version tag such as
`v1.1.1` if you prefer controlled upgrades.

## Backup and restore

See [backup and restore](backup-restore.md). Back up the database and
`APP_DATA_LOCATION`; the pictures and push keys live there.

## Logs and troubleshooting

```bash
docker compose ps
docker compose logs -f backend
```

Common problems and their fixes are in [troubleshooting](troubleshooting.md).

## Security checklist

- [ ] `DB_PASSWORD` changed from the default.
- [ ] `.env` not world-readable and not committed to git.
- [ ] HTTPS with a trusted certificate if reachable from other machines.
- [ ] `NTFY_BASE_URL` reachable from the backend, and users subscribed to their topic.
- [ ] Backups scheduled and a restore tested.
- [ ] The host firewall exposes only the proxy ports (80/443), not PostgreSQL.
