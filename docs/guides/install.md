[← Back to README](../../README.md)

# Installing Choruko

Choruko runs as three Docker containers: a web frontend, the API backend, and
PostgreSQL. The backend applies database migrations itself on startup.

## Requirements

- Docker with the Compose plugin
- About 1 GB of free disk space for images and data

Plain HTTP is enough for a trusted LAN: notifications go out through ntfy and do not
need HTTPS here.

## Step 1: download the deployment files

```bash
mkdir choruko && cd choruko

curl -fsSLO https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/prod/docker-compose.yml
curl -fsSL -o .env https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/example.env
```

## Step 2: configure

Edit `.env`. At minimum, change the database password:

```dotenv
DB_PASSWORD=change-me-to-a-long-alphanumeric-password
```

Use only `A-Z`, `a-z`, `0-9`. The full list of variables is in
[configuration](configuration.md).

Set `CHORUKO_PORT` if port `2467` is taken. Set `SERVER_URL` only if you enable email;
it must be the public URL of the instance.

## Step 3: start

```bash
docker compose up -d
```

The backend applies any pending database migrations before it starts serving. Watch
the startup:

```bash
docker compose logs -f backend
```

Open `http://localhost:2467` (or `http://<server-ip>:2467`).

## Step 4: first account and household

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

When email is disabled, or a user is locked out, run the admin command in the
backend container:

```bash
docker compose exec backend choruko-admin reset-password
```

It asks for the user's email and prints a new password. Share it over a secure
channel.

## Upgrade

> **Upgrading from v1.x is a breaking change:** the `choruko-manager` image is no
> longer published. Update `docker-compose.yml` before pulling — the `manager` service
> and the `manager` entry under `backend.depends_on` are gone. Pulling with the old
> file fails with `manifest unknown` for `choruko-manager`. Keep your `.env`; if you
> customized the compose file, merge those two removals instead of overwriting it.

```bash
# 1. back up first — see backup-restore.md
docker exec choruko_db pg_dump -U postgres -d choruko -Fc > "pre-upgrade-$(date +%F).dump"

# 2. fetch the current compose file next to .env (or merge the manager removal by hand)
curl -fsSL -o docker-compose.yml.new \
  https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/prod/docker-compose.yml
diff -u docker-compose.yml docker-compose.yml.new   # review your local changes
mv docker-compose.yml.new docker-compose.yml

# 3. pull and recreate
docker compose pull
docker compose up -d --remove-orphans
docker compose logs -f backend
```

`--remove-orphans` removes the `manager` container left behind by older releases, and
the backend applies pending migrations on startup.

`CHORUKO_VERSION=release` tracks the newest stable release. Pin a version tag such as
`v2.0.0` if you prefer controlled upgrades.

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
