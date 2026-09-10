# Backend

The backend is an ASP.NET Core API on .NET 10, organised as vertical slices with MediatR. `src/`
holds four projects: `HalcyonRecords.Api`, the Aspire host `HalcyonRecords.AppHost`,
`HalcyonRecords.ServiceDefaults` for telemetry and health checks, and `HalcyonRecords.Shared` for
code the API shares with the seed tool. `tests/` holds the unit and integration suites, and
`tools/` holds the seed data generator.

Commands run from `backend/` unless a section says otherwise. The [root README](../README.md)
covers starting the app and running every test.

## Filtering test runs

`--project` runs a single suite:

```bash
dotnet test --project tests/HalcyonRecords.Api.IntegrationTests
```

The integration suite needs Docker Desktop running.

Tests run on Microsoft Testing Platform, so filtering uses xUnit's own options. `--filter-class`
takes a fully qualified class name and `--filter-method` a fully qualified method name, and both
accept `*` at the start or end:

```bash
dotnet test --project tests/HalcyonRecords.Api.UnitTests --filter-class "*SlugifierTests"
dotnet test --project tests/HalcyonRecords.Api.UnitTests --filter-method "*Slugify_ProducesExpectedSlug"
```

Keep `--project` alongside a filter. Without it, a filter that matches nothing in one suite marks
the whole run as failed, even when every test that ran has passed.

## Adding a migration

The EF Core CLI and CSharpier are local tools pinned in `.config/dotnet-tools.json`. Restore them
once:

```bash
dotnet tool restore
```

Migrations are added from the API project. The generated files need formatting before CI accepts
them:

```bash
cd src/HalcyonRecords.Api
dotnet ef migrations add YourMigrationName --output-dir Infrastructure/Migrations
dotnet csharpier format Infrastructure/Migrations
```

In development, the API applies pending migrations when it starts. In production, the `migrate`
job applies them on every deploy.

To remove the most recent migration, from the same folder:

```bash
dotnet ef migrations remove --force
```

The design-time connection string points at no real database. `--force` lets the removal go ahead
when EF cannot reach it to check whether the migration was applied.

## Formatting

CI runs `dotnet csharpier check .` and fails if CSharpier would change any file. To format the
whole backend:

```bash
dotnet csharpier format .
```

## Lock files

Every project except the AppHost has a `packages.lock.json` recording the exact version of every
package it depends on. Adding, removing or updating a package rewrites the file on the next
restore. Commit it with the change, since CI restores in locked mode and fails when a lock file is
out of date.

## Background jobs

In development, Coravel runs account maintenance and the restock nightly inside the API. Two
development-only endpoints run them on demand:

```bash
curl -X POST https://localhost:7000/api/dev/albums/restock
curl -X POST https://localhost:7000/api/dev/demo-accounts/run-maintenance
```

Neither endpoint appears in the OpenAPI document or in Scalar.

In production, each job is an Azure Container Apps job that starts the API image with `--job` and
a job name. The API runs that one job and exits.

- `migrate`: applies pending migrations.
- `seed`: loads the sample catalogue into an empty database.
- `reindex`: rebuilds the Meilisearch index from the database.
- `reseed`: deletes all data including accounts and orders, then runs `seed` and `reindex`.
- `account-maintenance`: deletes stale accounts and resets the showcase account.
- `restock`: restores every album to its restock level.

`deploy-api.yml` runs `migrate`, `seed` and `reindex` in that order after every deploy.
`account-maintenance` runs daily at 03:15 UTC and `restock` at 03:20 UTC. `reseed` runs only when
started by hand.
