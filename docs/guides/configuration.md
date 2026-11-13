[← Back to README](../../README.md)

# Configuration

Choruko is configured with environment variables. In the Docker Compose setup they
live in `.env`, next to `docker-compose.yml`. The backend reads the same variables
from the process environment, so the same values work outside Docker.

Copy [`docker/compose/example.env`](../../docker/compose/example.env) to `.env` and
adjust it. Do not commit `.env` — it contains secrets.

## Compose variables

| Variable | Default | Description |
|---|---|---|
| `CHORUKO_VERSION` | `release` | Image tag to deploy: `release`, `latest`, or a version such as `v1.0.2`. |
| `CHORUKO_PORT` | `2467` | Host port for the web interface. The container always listens on port 80. |
| `APP_DATA_LOCATION` | `./app_data` | Host directory for uploaded pictures and generated VAPID keys. |
| `DB_DATA_LOCATION` | `./db_data` | Host directory for PostgreSQL data. Network shares are not supported. |

`APP_DATA_LOCATION` and `DB_DATA_LOCATION` are bind mounts. Back them up; see
[backup and restore](backup-restore.md).

## Database

| Variable | Default | Description |
|---|---|---|
| `DB_HOST` | `database` | PostgreSQL host. The Compose service is called `database`. |
| `DB_PORT` | `5432` | PostgreSQL port. |
| `DB_USERNAME` | `postgres` | Database user. |
| `DB_PASSWORD` | `postgres` | Database password. **Change it.** Use only `A-Z`, `a-z`, `0-9`. |
| `DB_DATABASE_NAME` | `choruko` | Database name. |

The `manager` container applies EF Core migrations on startup and writes a health
signal when it is done; `backend` waits for it. Do not start the backend against an
empty database manually.

Changing `DB_PASSWORD` on an existing installation requires changing it in
PostgreSQL as well, not just in `.env`.

## Email

Password-reset emails are optional. With `ENABLE_EMAILS=false` the app runs a no-op
email service, and users can only get a new password through the `manager` container
(see [installation](install.md#reset-a-password)).

| Variable | Default | Description |
|---|---|---|
| `ENABLE_EMAILS` | `false` | `true` enables SMTP. |
| `SMTP_HOST` | — | SMTP server, for example `smtp.example.com`. Required when emails are enabled. |
| `SMTP_PORT` | `587` | `587` for STARTTLS, `465` for implicit TLS. |
| `SMTP_USERNAME` | — | SMTP user. Required when emails are enabled. |
| `SMTP_PASSWORD` | — | SMTP password or app password. Required when emails are enabled. |
| `SMTP_FROM` | — | Sender identity, for example `Choruko <no-reply@example.com>`. Required. |
| `SERVER_URL` | — | Public URL of your instance, used in reset links, for example `https://choruko.example.com`. Required. |

When `ENABLE_EMAILS=true`, the backend refuses to start if any of the required SMTP
values or `SERVER_URL` is missing.

## Storage

Uploaded pictures are stored under `APP_DATA_LOCATION` and served by the backend at
`/uploads/...`. The directory contains:

- `uploads/users/` — profile pictures
- `uploads/households/` — household pictures
- `uploads/vapid-keys.json` — generated Web Push keys

Keep the directory writable by the container. Back it up together with the database;
the VAPID keys must survive a redeploy, otherwise existing push subscriptions stop
working.

## Timezone

| Variable | Default | Description |
|---|---|---|
| `TZ` | host timezone | IANA timezone name, for example `Europe/Warsaw`. |

The Compose files also mount `/etc/localtime` read-only. `TZ` is used for date
formatting and for the deadline notification service.

## Image uploads (advanced)

Picture processing is configured in `backend/ChorukoAPI/appsettings.json` under
`ImageSettings`. Any setting can be overridden with an environment variable using a
double underscore:

| Setting | Default | Meaning |
|---|---|---|
| `ImageSettings__MaxFileSize` | `10485760` | Maximum upload size in bytes (10 MB). |
| `ImageSettings__MaxDimension` | `400` | Images larger than this are cropped and resized. |
| `ImageSettings__MinDimension` | `150` | Smaller images are rejected. |
| `ImageSettings__AllowedFormats__0` | `.png` | Allowed extension. Add more with `__1`, `__2`, … |

Supported formats by default: `.png`, `.jpg`, `.jpeg`, `.webp`, `.gif`.

## Reverse proxy

The container serves plain HTTP on port 80 and terminates nothing. Put a reverse
proxy in front of it if the instance is reachable from other machines:

- PWA installation and Web Push require a secure context: HTTPS with a trusted
  certificate, or `localhost`.
- The rest of the app works over plain HTTP on a trusted LAN.
- Forward the original `Host` header. The backend does not currently process
  `X-Forwarded-*` headers, so cookie security and generated links rely on
  `SERVER_URL` and on the proxy preserving the host.

See the [installation guide](install.md#reverse-proxy-and-https) for Caddy and Nginx
examples.

## Minimal `.env`

```dotenv
CHORUKO_VERSION=release
CHORUKO_PORT=2467
APP_DATA_LOCATION=./app_data
DB_DATA_LOCATION=./db_data

DB_PASSWORD=change-me-to-a-long-alphanumeric-password
DB_USERNAME=postgres
DB_DATABASE_NAME=choruko

ENABLE_EMAILS=false
TZ=Europe/Warsaw
```
