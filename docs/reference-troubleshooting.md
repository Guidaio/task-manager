# Troubleshooting and runbook notes

Quick fixes for problems reviewers and developers commonly hit. The primary commands still live in the root **[README.md](../README.md)**.

---

## SQL Server (Docker)

| Symptom | What to check |
|---------|----------------|
| API fails on startup; logs mention SQL / timeout | Run **`docker compose up -d sqlserver`**. Wait until the container is healthy (Compose `healthcheck`). |
| Login failed for SQL | SA password and port must match **`docker-compose.yml`** and **`appsettings.Development.json`** (`TaskManager_dev_12345`, port **1433**). |
| Port 1433 already in use | Stop another SQL instance or change the host port mapping in Compose and update connection strings accordingly. |

Start / check:

```powershell
docker compose up -d sqlserver
docker compose ps
```

---

## Database reset (development)

The API runs **`DatabaseInitializer`** on startup (creates schema, optional database from master connection, seed).

To **wipe application data** and let the initializer recreate schema + seed on next API start:

1. Stop the API.
2. Either drop the database in SQL Server (e.g. `TaskManager`, and `TaskManager_IntegrationTests` if you use integration tests), or remove the Docker volume used by the SQL container (destructive; see Compose `volumes`).

Simplest full reset when using Compose for SQL only: stop containers, remove the named volume for `sqlserver`, then `docker compose up -d sqlserver` again and restart the API.

**Warning:** removing volumes deletes all local SQL data for that Compose project.

---

## `dotnet test` / build: file lock (MSB3027)

**Symptom:** Build or test fails with **MSB3027** or “file is being used by another process” when copying outputs under **`TaskManager.Api/bin`**.

**Cause:** **`dotnet run`** (or another process) still has the API loaded and locks DLLs.

**Fix:** Stop the running API process, then run:

```powershell
dotnet test .\backend\TaskManager.sln
```

Same applies if an IDE debugger is attached to the API.

---

## Integration tests

- Require **SQL Server** at **`localhost,1433`** with credentials from **`appsettings.IntegrationTests.json`** (aligned with Docker Compose SA password).
- Use database **`TaskManager_IntegrationTests`**.
- Start SQL before **`dotnet test`** when running integration tests locally.

---

## Forbidden dependencies (assignment)

Verify no EF Core / Dapper / MediatR packages:

```powershell
cd .\backend
dotnet list TaskManager.sln package --include-transitive | Select-String -Pattern "EntityFramework|Dapper|MediatR"
```

Expect **no** matching lines. CI runs a similar check in **`.github/workflows/ci.yml`**.

---

## Angular: `ng serve` / builder package not found

**Symptom:** `npm start` fails with *Could not find the '@angular-devkit/build-angular:dev-server' builder's node package* or *Node packages may not be installed*.

**Cause:** **`node_modules`** is not in the repo; dependencies were never installed in this folder.

**Fix:** From **`frontend/task-manager-web`**:

```powershell
npm install
npm start
```

Use **`npm run build`** (not `npm build`) for a production build. On a fresh clone, always run **`npm install`** once before **`npm start`**.

---

## Angular cannot reach the API

- Confirm API URL in **`frontend/task-manager-web/src/environments/environment.ts`** matches the port printed by **`dotnet run`** (default example **5035**).
- CORS: API allows the Angular dev origin (`http://localhost:4200`); if you change ports, update API CORS configuration.

---

## SignalR / notifications in the browser

- Token is passed via **`access_token`** on the negotiate URL (JWT header not available on WebSocket in browsers).
- If notifications never appear: confirm login succeeded, SQL is up, and browser console for connection errors.

---

## Run the API in Docker (optional)

See **`backend/Dockerfile`**. Build from **repository root**:

```powershell
docker build -f backend/Dockerfile -t taskmanager-api .
```

Run requires a reachable SQL Server; set **`Database__ConnectionString`** (and related settings) via environment variables or mounted config for your environment. Local development is usually simpler with **`dotnet run`** + Compose SQL only.

---

## Where to look in code

| Concern | Location |
|---------|----------|
| Startup, JWT, SignalR | `backend/src/TaskManager.Api/Program.cs` |
| Global errors | `Middleware/ExceptionHandlingMiddleware.cs` (also returns `correlationId`) |
| Correlation id | `Middleware/CorrelationIdMiddleware.cs` |
| SQL access | `backend/src/TaskManager.Infrastructure/Persistence/` |
| Seed / demo user | `Infrastructure/Seed/DemoSeed.cs`, `DatabaseInitializer` |
