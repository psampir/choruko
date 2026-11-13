[← Back to README](../../README.md)

# Troubleshooting

Start with the container state and logs:

```bash
docker compose ps
docker compose logs -f backend
docker compose logs -f manager
docker compose logs -f frontend
docker compose logs -f database
```

The backend waits for the `manager` container, which applies migrations. If `manager`
is unhealthy, the backend never starts.

## The web interface does not load

- `docker compose ps` — all four containers should be `running`, `database` and
  `manager` `healthy`.
- Check the host port: `CHORUKO_PORT` defaults to `2467`.
- If a reverse proxy returns `502` or `504`, it cannot reach the frontend container.
  Verify the proxy target (`127.0.0.1:2467` in the examples) and that the proxy is on
  the same Docker network or host.
- `docker compose logs frontend` shows Nginx errors.

## Login does not stick / redirects to the login page

Choruko uses a cookie named `ChorukoCookie`, `HttpOnly`, `SameSite=Lax`, 30-day
sliding expiration.

- The browser and the API must be on the same origin. Use one hostname for the SPA
  and `/api`; do not call the API on a different port or hostname from the browser.
- A reverse proxy must forward the `Cookie` header and preserve the `Host` header.
  The Nginx example in the installation guide does both.
- Behind HTTPS, the cookie is only marked `Secure` if the backend sees HTTPS. The
  backend does not currently process `X-Forwarded-Proto`, so the cookie may lack the
  `Secure` flag; it still works, but keep the proxy trusted.
- If `ENABLE_EMAILS=true`, `SERVER_URL` must be the public URL of the instance;
  otherwise password-reset links point to the wrong place.

## "Copy topic" does nothing

The clipboard API needs a **secure context**: HTTPS with a certificate the device
trusts, or `http://localhost` / `http://127.0.0.1`. On `http://192.168.x.x` the app
falls back to a hidden text area, which some mobile browsers still refuse. Select the
topic by hand, or serve the instance over HTTPS.

Everything else, including notifications, works over plain HTTP: delivery happens
between the backend and the ntfy server, not in the browser.

## Notifications (ntfy) do not arrive

1. `NOTIFIER=ntfy` and `NTFY_BASE_URL` must point to an ntfy server reachable from
   the backend.
2. The user needs the topic and server address shown in **Manage Account →
   Notifications** (`GET /api/User/notification-settings`) and must be subscribed
   to that topic in the ntfy app.
3. A self-hosted ntfy server must be reachable from the phone. For background
   delivery on iOS, set `NTFY_UPSTREAM_BASE_URL=https://ntfy.sh` on the ntfy server
   (or use `https://ntfy.sh` directly).
4. If the ntfy server uses access control, set `NTFY_TOKEN`.
5. Check the backend log for `NTFY_BASE_URL is not configured` or
   `ntfy delivery failed`.
6. `PUBLIC_APP_URL` must be the URL users actually open, otherwise notification
   links point to the wrong place.

Web Push, VAPID and the `PushSubscriptions` table were removed; notifications go
through `INotifier` only. `/dev-push-test` and `/api/PushSubscription/*` no longer
exist.

## Picture upload fails

- Maximum file size is 10 MB by default. A reverse proxy must allow the same body
  size (`client_max_body_size 10M` in the Nginx example).
- Accepted formats: `.png`, `.jpg`, `.jpeg`, `.webp`, `.gif`.
- The image must be at least 150×150 px; larger than 400×400 is cropped and resized.
- Errors are returned as field validation errors; the UI shows the message from the
  API.

## Database errors

- `password authentication failed` — `.env` and the PostgreSQL volume disagree.
  Changing `DB_PASSWORD` on an existing installation requires changing the role in
  PostgreSQL too.
- `relation "..." does not exist` — migrations did not run. Check
  `docker compose logs manager`.
- To open a shell:

  ```bash
  docker exec -it choruko_db psql -U postgres -d choruko
  ```

- To inspect the schema version:

  ```sql
  SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC;
  ```

## Emails are not sent

- `ENABLE_EMAILS` must be `true`.
- `SMTP_HOST`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM` and `SERVER_URL` are all
  required; the backend refuses to start if one is missing.
- Port `587` is STARTTLS, `465` is implicit TLS.
- The forgot-password page asks `GET /api/config/email-enabled` and shows a notice
  when email is disabled.

## Reset a password

When email is disabled, use the `manager` container. It calls the admin endpoint
internally with the database superuser credentials from `.env` and prints a new
password:

```bash
docker exec -it choruko_mgr reset-password
```

The command is interactive and asks for the user's email. Share the generated
password with the user over a secure channel.

## Deadlines or points look wrong

- Containers use the host timezone through `/etc/localtime`; set `TZ` explicitly if
  the host timezone is ambiguous.
- Points are recalculated on task completion and un-completion. Monthly counters are
  reset on the 1st, yearly counters in January, by `PointsResetService`.
- Completed tasks and settlements older than three months are removed by
  `DataCleanupService`. Point counters live on the user and are not recalculated, so
  they are unaffected.
