[← Back to README](../../README.md)

# Backup and restore

Choruko keeps state in two places:

1. **PostgreSQL** — users, households, tasks, activities, settlements, invitations,
   points and subscriptions.
2. **App data directory** (`APP_DATA_LOCATION`) — profile pictures, household
   pictures and the generated VAPID keys.

Keep `.env` with them; it contains the database password and SMTP settings.

## What to back up

| Item | Where | Why |
|---|---|---|
| Database dump | PostgreSQL | all relational data |
| `APP_DATA_LOCATION` | host directory | uploaded pictures and push keys |
| `.env` | next to `docker-compose.yml` | secrets and configuration |
| `docker-compose.yml` | next to `.env` | exact deployment definition |

Losing the VAPID keys does not break the app, but every existing push subscription
stops working and users have to subscribe again.

## Database dump

The container is named `choruko_db` in the provided Compose file. Adjust the names if
you changed them.

```bash
# custom format, compressed, restorable with pg_restore
docker exec choruko_db pg_dump -U postgres -d choruko -Fc > "choruko-$(date +%F).dump"
```

Check that the file is not empty:

```bash
ls -lh choruko-*.dump
```

A plain SQL dump also works, but `pg_restore` with the custom format is more flexible.

## App data

```bash
tar czf "app-data-$(date +%F).tar.gz" -C "${APP_DATA_LOCATION:-./app_data}" .
```

Stop the stack first if you want a perfectly consistent copy of the pictures:

```bash
docker compose down
tar czf "app-data-$(date +%F).tar.gz" -C "${APP_DATA_LOCATION:-./app_data}" .
docker compose up -d
```

The database dump is consistent on its own; stopping is only about the files.

## Restore

### 1. Restore configuration

Put `docker-compose.yml` and `.env` back in the deployment directory. If the database
password changed, update `.env` and PostgreSQL accordingly.

### 2. Restore the database

Start only the database, then restore the dump:

```bash
docker compose up -d database

docker exec -i choruko_db pg_restore \
  -U postgres -d choruko --clean --if-exists < choruko-YYYY-MM-DD.dump
```

`--clean --if-exists` drops existing objects first, so this replaces the current
database. If the dump was made with a plain SQL dump, use `psql` instead:

```bash
docker exec -i choruko_db psql -U postgres -d choruko < choruko-YYYY-MM-DD.sql
```

### 3. Restore app data

```bash
tar xzf app-data-YYYY-MM-DD.tar.gz -C "${APP_DATA_LOCATION:-./app_data}"
```

### 4. Start everything

```bash
docker compose up -d
```

The `manager` container applies any pending migrations, then the backend starts.
Watch the logs:

```bash
docker compose logs -f manager backend
```

## Upgrade

```bash
# 1. back up first
docker exec choruko_db pg_dump -U postgres -d choruko -Fc > "pre-upgrade-$(date +%F).dump"

# 2. pull the new images
docker compose pull

# 3. recreate containers; the manager runs migrations
docker compose up -d

# 4. verify
docker compose ps
docker compose logs -f manager backend
```

Pin `CHORUKO_VERSION` to a version tag (for example `v1.0.2`) if you want to control
upgrades instead of tracking `release`.

## Test the restore

A backup is only useful if it restores. Once in a while, restore into a separate
directory and port, for example:

```bash
mkdir /tmp/choruko-restore && cd /tmp/choruko-restore
# copy docker-compose.yml, .env and the dump, change CHORUKO_PORT and the volume paths
docker compose up -d
```

Check that you can log in, see the household and open a task.
