# Session checkpoint (handoff)

Use this file when picking up after a break or the next day.

---

## Evening wrap — 2026-05-09

**Git:** `master` matches **`origin/master`** — latest pushed commit **`34d2aba`** (`feat(api): Swagger UI with JWT bearer (Development)`). Working tree should be clean unless you have uncommitted local edits.

**Done today:**

- **Step 3 on `master`:** ADO.NET (`Microsoft.Data.SqlClient` only — **no EF, no Dapper, no MediatR**), schema + demo seed, BCrypt, JWT issuance + bearer middleware. Notifications FK uses **`ON DELETE NO ACTION`** on `TaskItems` to satisfy SQL Server cascade rules.
- **Step 4 on `master`:** REST auth + tasks + health, FluentValidation, correlation + exception middleware, CORS, **Swagger at `/swagger`** (Development).
- **Smoke verified locally:** health, JWT login, **`GET /api/tasks`** returning seeded demo tasks.

**Operational tip:** Stop the API (**Ctrl+C** or kill `dotnet` hosting `TaskManager.Api`) before **`dotnet build`** to avoid DLL locks.

---

## Completed on `master` (summary)

- **Step 3:** ADO.NET repositories, `DatabaseInitializer`, BCrypt, `JwtTokenService`, DI via `AddInfrastructure`.
- **Step 4:** `POST /api/auth/register`, `POST /api/auth/login`, task CRUD **`/api/tasks`** with JWT, **`GET /api/health`**, FluentValidation, middleware, CORS, **`/swagger`** in Development.

See **`README.md`**, **`docs/development-log.md`**, **`docs/architecture.md`**.

---

## Tomorrow — start here

1. **`git pull`** if you work from another machine.
2. **SQL:** `docker compose up -d sqlserver`
3. **API:** `dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj`
4. **`/swagger`** — login as demo user, exercise tasks CRUD.
5. **Next implementation milestones** (in order from project todos):
   - **Integration tests** (`WebApplicationFactory`, DB + JWT + isolation).
   - **Angular** (`frontend/task-manager-web`).
   - **SignalR + notifications** only after **core green** checklist is satisfied.

---

## Suggested local smoke (after pull)

```powershell
docker compose up -d sqlserver
dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj
```

Then `GET /api/health`, `POST /api/auth/login`, `GET /api/tasks` with Bearer token.

---

## Configuration reminder (for interviews)

- **Database:** `Database:ConnectionString` / `MasterConnectionString` in **`appsettings.Development.json`** (not hardcoded in repository SQL — only **demo seed ids** are constants in `DemoSeed`).
- **JWT:** `Jwt` section in appsettings; production would use env/secrets, not committed keys.

You may delete this file once you rely solely on `docs/development-log.md`.
