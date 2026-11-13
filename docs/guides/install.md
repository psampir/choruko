[← Back to README](../../README.md)

# Installing Choruko

Choruko runs as four Docker containers: a web frontend, the API backend, a short-lived
manager that applies database migrations and provides admin tools, and PostgreSQL.

## Requirements

- Docker with the Compose plugin
- About 1 GB of free disk space for images and data
- Optional: a reverse proxy and a TLS certificate if the instance is reachable from
  other machines

Plain HTTP is enough for a trusted LAN. Notifications go out through ntfy and do
not need HTTPS on this instance. Add
[Reverse proxy and HTTPS](#reverse-proxy-and-https) when the instance is reachable
from other machines, so passwords and session cookies are not sent in clear text.

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

## Reverse proxy and HTTPS

The containers speak plain HTTP on `${CHORUKO_PORT}`. Terminate TLS in front of them
and forward everything to that port. Do not expose the backend directly.

### Caddy

Caddy obtains and renews certificates automatically:

```caddyfile
choruko.example.com {
    reverse_proxy 127.0.0.1:2467
}
```

### Nginx

```nginx
server {
    listen 443 ssl;
    server_name choruko.example.com;

    ssl_certificate     /etc/letsencrypt/live/choruko.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/choruko.example.com/privkey.pem;

    # picture uploads
    client_max_body_size 10m;

    location / {
        proxy_pass http://127.0.0.1:2467;
        proxy_http_version 1.1;

        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Add a port-80 server that redirects to HTTPS, and obtain certificates with your
preferred ACME client.

### What HTTPS buys you

| Feature | HTTP on a LAN | HTTPS with a trusted certificate |
|---|---|---|
| Tasks, settlements, invitations | ✅ | ✅ |
| Picture uploads | ✅ | ✅ |
| Add to home screen / web clip | ✅ | ✅ |
| ntfy notifications | ✅ | ✅ |
| Password and session protection | ❌ | ✅ |

Notifications travel from the backend to your ntfy server, so this instance does not
need to be reachable from the phone. A certificate the browser only accepts after a
manual warning bypass gives no real protection, but everything still works.

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
`v1.0.2` if you prefer controlled upgrades.

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
