# Session checkpoint (handoff)

This file was introduced during Step 3 verification; content below reflects **current** repo intent after Steps **3–4**.

## Completed on `master`

- **Step 3:** ADO.NET repositories, `DatabaseInitializer` (schema + demo seed), BCrypt hashing, JWT signing + bearer middleware. Notifications FK `FK_Notifications_TaskItems` uses **`ON DELETE NO ACTION`** to avoid SQL Server multiple cascade paths (instead of `SET NULL`).
- **Step 4:** `POST /api/auth/register`, `POST /api/auth/login`, task CRUD under **`/api/tasks`** with JWT, **`GET /api/health`**, FluentValidation, **`X-Correlation-ID`** middleware, JSON exception middleware, CORS for Angular dev (`http://localhost:4200` by default).

See **`README.md`** (routes + demo user), **`docs/development-log.md`**, and **`docs/architecture.md`** for detail.

## Suggested local smoke (after pull)

```powershell
docker compose up -d sqlserver
dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj
```

Try `GET /api/health`, then register/login, then `Authorization: Bearer …` on `/api/tasks`.

## Next milestones

1. Integration tests (`WebApplicationFactory`).
2. Angular client.
3. SignalR + notifications **after core green**.

You may delete this file if you prefer everything to live only in `docs/development-log.md`.
