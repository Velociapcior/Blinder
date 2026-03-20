# Database and Migrations

PostgreSQL 16 + PostGIS, managed via EF Core. Connection is configured through `ConnectionStrings__DefaultConnection` (see [Configuration](../README.md#configuration)).

## Dev environment (automatic)

In `Development`, the API automatically creates the database if it does not exist and applies any pending EF migrations on startup via `HostExtensions.MigrateDatabaseAsync()`. No manual steps required.

## Other environments (manual)

Migrations are **not** auto-applied outside `Development`. Use `migrations/latest.sql` to update the database schema explicitly.

Apply the script before starting the API:

```bash
docker compose exec -T db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" < migrations/latest.sql
```

## Add a migration

Run EF tooling inside the dev container (SDK stage has design-time packages):

```bash
docker compose run --rm api dotnet ef migrations add <MigrationName> \
    --project /src/Blinder.Api \
    --startup-project /src/Blinder.Api
```

## Regenerate the SQL artifact

After adding a migration, regenerate `migrations/latest.sql`:

```bash
docker compose run --rm api dotnet ef migrations script \
    --idempotent \
    --output /src/migrations/latest.sql \
    --project /src/Blinder.Api \
    --startup-project /src/Blinder.Api
```

Commit `migrations/latest.sql` together with the EF migration files.

## Connect directly to the database (dev)

PostgreSQL port `5432` is forwarded to the host in `docker-compose.override.yml`:

```bash
docker compose exec db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"
```

Or connect from the host with any PostgreSQL client on `localhost:5432`.

## Notes

- EF design-time tooling (`dotnet ef`) is only available inside the dev container (SDK stage). It is not available in the production runtime image.
- `docker compose down -v` is **prohibited** — it would destroy the bind-mounted DB data at `./postgres/data/pgdata`.
- To fully wipe the dev database and start fresh: delete `./postgres/data/pgdata` then `docker compose up -d --build`.
