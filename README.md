# Task Manager

Full stack Task Manager built for the Ballast Lane Senior .NET technical exercise.

## Goal

The application will let authenticated users register, log in, and manage their own tasks. The backend will use Clean Architecture, ASP.NET Core Web API, SQL Server, and plain ADO.NET. The frontend will use Angular.

## Stack

- .NET 8
- ASP.NET Core Web API with Controllers
- SQL Server
- Plain ADO.NET with parameterized SQL
- Angular
- xUnit
- Docker Compose

## Prerequisites

- **.NET 8 SDK** (matches `net8.0` target in backend projects)
- **Node.js** + **npm** (for the Angular app under `frontend/task-manager-web`)
- **Docker Desktop** (or compatible engine) for **SQL Server** via Compose
- **SQL Server** via **`docker compose`** (see below)—no standalone local instance required for the default dev path

## Quick start (from scratch)

**Working directory:** open a terminal in the **repository root**—the folder that contains `README.md`, `docker-compose.yml`, `backend\`, and `frontend\` (not inside `backend` or `frontend`).

1. **Prerequisites** — Install **.NET 8 SDK**, **Node.js (LTS) + npm**, and **Docker Desktop** (or another engine that provides `docker compose`).

2. **Start SQL Server** with Docker Compose:

   ```powershell
   docker compose up -d sqlserver
   ```

   First startup can take **30–60 seconds**. Ensure the container is healthy (`docker compose ps`, or check Docker Desktop) before starting the API. The app expects SQL at **`localhost:1433`** (see `backend/src/TaskManager.Api/appsettings.Development.json`).

3. **Run the API** (Development; uses `http://localhost:5035` from the default **http** profile):

   ```powershell
   dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj
   ```

   Wait for a log line like **Now listening on: `http://localhost:5035`**. If the DB was not ready, restart the API after SQL is healthy.

4. **Run the Angular app** (second terminal, still from repo root for `cd`):

   ```powershell
   cd .\frontend\task-manager-web
   npm install
   npm start
   ```

   Browse **`http://localhost:4200`**. The dev config points at the API on port **5035** (`frontend/task-manager-web/src/environments/environment.ts`).

5. **Demo credentials** — Sign in with the seeded user (see **[Demo credentials](#demo-credentials)** below), or use **Register** in the UI.

6. **Run backend tests** — **Integration tests need SQL running** (step 2). **Stop the API** process first if `dotnet test` fails with **MSB3027** / “file is being used by another process” (locked DLLs under `TaskManager.Api`).

   ```powershell
   dotnet test .\backend\TaskManager.sln
   ```

   Optional: from repo root, **`.\scripts\verify-backend.ps1`** runs **`dotnet build`** then **`dotnet test`** on the solution (same SQL / stop-API notes apply). Use **`.\scripts\verify-backend.ps1 -SkipTest`** to build only.

7. **Troubleshooting** — Common issues: SQL not up or wrong port, API started before SQL was ready, CORS/API URL mismatch, locked files during tests. See **[`docs/reference-troubleshooting.md`](docs/reference-troubleshooting.md)** (Docker/SQL, MSB3027, integration DB, forbidden dependencies).

On **macOS/Linux**, use `/` in paths (e.g. `docker compose up -d sqlserver`, `dotnet run --project backend/src/TaskManager.Api/TaskManager.Api.csproj`). **`docker-compose` (v1)** may work instead of **`docker compose`** if that is what your machine provides.

## Restrictions

This project must not use:

- Entity Framework or EF Core
- Dapper
- MediatR or mediator libraries

## Repository Structure

```text
.github/
  workflows/
    ci.yml
backend/
  Dockerfile
  TaskManager.sln
  src/
    TaskManager.Api/
    TaskManager.Application/
    TaskManager.Domain/
    TaskManager.Infrastructure/
  tests/
    TaskManager.UnitTests/
    TaskManager.IntegrationTests/
frontend/
  task-manager-web/
    src/                    # application code (components, services, …)
    public/
    angular.json
    package.json
    tsconfig.json
    tsconfig.app.json
    tsconfig.spec.json      # Karma / unit-test compilation
docs/
  README.md                 # documentation index
  brief/                    # exercise prompt (en/pt) + source/ (PDFs, DOCX)
  guide-*.md                # architecture, design, presentation
  reference-*.md            # credentials, testing, troubleshooting, security
  process-*.md              # AI usage, development log
scripts/
  verify-backend.ps1        # optional: dotnet build + test from repo root (PowerShell)
docker-compose.yml
README.md
```

**Frontend tests:** The Angular app is wired for **Karma + Jasmine** (`npm run test` in `frontend/task-manager-web`, `angular.json` `test` target). Convention is **`src/**/*.spec.ts`** next to features. This repo does not include spec files yet; coverage today is **backend xUnit** (see `docs/reference-testing-requirements.md`).

## Core Green Checklist

SignalR and notification work starts only after this checklist is complete:

- Backend solution builds successfully.
- SQL Server runs through Docker Compose.
- Database initializer creates schema and seed/demo data.
- Register and login work.
- JWT protects task endpoints.
- Task CRUD works.
- Task operations are always scoped by authenticated `UserId`.
- Main unit tests pass.
- Main integration tests pass.
- README has minimum run instructions.

## HTTP API (backend)

Base URL: whatever port `dotnet run` prints (e.g. `http://localhost:5035`).

| Method | Route | Auth | Notes |
|--------|-------|------|------|
| GET | `/api/health` | No | |
| POST | `/api/auth/register` | No | |
| POST | `/api/auth/login` | No | |
| GET | `/api/tasks` | Bearer JWT | See **Task list query** below |
| GET | `/api/tasks/{id}` | Bearer JWT | |
| POST | `/api/tasks` | Bearer JWT | |
| PUT | `/api/tasks/{id}` | Bearer JWT | |
| DELETE | `/api/tasks/{id}` | Bearer JWT | |
| GET | `/api/notifications` | Bearer JWT | Returns persisted notifications for the current user (newest first). Before the query, rows older than **30 days** for that user are deleted (retention). |
| POST | `/api/notifications/mark-read` | Bearer JWT | Body: `{ "ids": [ "guid", ... ] }` (max 50). Marks those notifications read for the user. |
| DELETE | `/api/notifications` | Bearer JWT | Deletes **all** notification rows for the current user. **`204 No Content`** on success. |

Send `Authorization: Bearer <token>` from `/api/auth/login` or register for **protected** task and notification routes. Enum `status` values serialize as JSON strings (`Pending`, `InProgress`, ...).

### Task list query (`GET /api/tasks`)

Response body: `{ "items": [ ... TaskDto ... ], "totalCount", "page", "pageSize" }`.

| Query | Default | Notes |
|-------|---------|--------|
| `status` | *(none)* | Optional. `Pending`, `InProgress`, `Completed`, or `Cancelled` (case-insensitive). Omit to list all statuses. |
| `page` | `1` | 1-based; values less than 1 are treated as 1. |
| `pageSize` | `25` | Clamped to **1–100** (large values capped at 100). |
| `sort` | `created` | `created` (default), `title`, `status`, or `due`. |
| `order` | `desc` when `sort=created`; `asc` otherwise | `asc` or `desc`. Omit to use that default for the selected sort. |
| `search` | *(none)* | Optional. Case-insensitive substring match on **title** or **description** (max **200** characters). |

Example: `/api/tasks?status=Completed&page=1&pageSize=10&sort=title&order=asc&search=invoice`

**Task create/update/delete** enqueue notifications that are **persisted in SQL** and pushed to the signed-in user over **SignalR** (see below). The notification drawer can show **mark-read** state (`POST /api/notifications/mark-read`); the UI also offers **Clear all**, which calls **`DELETE /api/notifications`**.

### Realtime (SignalR)

Connect to **`/hubs/notifications`** with the same JWT used for the API. Browsers cannot attach `Authorization` headers to the WebSocket upgrade, so the client sends the token via the **`access_token`** query parameter (the ASP.NET JWT bearer handler is configured to read it for that path). The Angular app uses `@microsoft/signalr` with `accessTokenFactory`, shows **in-app toasts** (auto-dismiss after a few seconds), and keeps a **notification center** (bell + slide-out list merged with **`GET /api/notifications`** and realtime payloads). **History** is stored in the database; **retention:** listing notifications triggers deletion of rows **older than 30 days** for that user (see **`GET /api/notifications`** above).

### Swagger / OpenAPI (Development)

When `ASPNETCORE_ENVIRONMENT=Development`, browse **`/swagger`** for interactive docs. Click **Authorize**, paste the **JWT only** (no `Bearer ` prefix), then call protected `/api/tasks` endpoints.

## Demo Credentials

After the database initializer runs (API startup with SQL Server available), a demo user is seeded if missing:

| Field | Value |
|--------|--------|
| Email | `demo@taskmanager.local` |
| Password | `Demo_user_12345` |

Fixed demo user id (for reference): `11111111-1111-1111-1111-111111111111`. Two sample tasks are seeded when that user has no tasks.

**Full dev reference** (SQL `sa`, ports, JWT, SignalR): see [`docs/reference-credentials.md`](docs/reference-credentials.md).

Commands for Docker, API, frontend, and tests are in **[Quick start (from scratch)](#quick-start-from-scratch)**. To compile without running:

```powershell
dotnet build .\backend\TaskManager.sln
```

## Optional: API Docker image

Multi-stage build from **repository root** (requires Docker). SQL must still be reachable from the container (configure connection string via environment for your network).

```powershell
docker build -f backend/Dockerfile -t taskmanager-api .
docker run --rm -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development -e Database__ConnectionString="YOUR_SQL_CONNECTION" taskmanager-api
```

Defaults: **`ASPNETCORE_URLS=http://+:8080`**. See [`docs/reference-troubleshooting.md`](docs/reference-troubleshooting.md).

## Final Smoke Test Checklist

For milestone-level validation notes, see **`docs/process-development-log.md`** (latest entry) and **`docs/reference-testing-requirements.md`**. Quick local checks:

- Frontend login/register.
- Task list/create/edit/delete.
- Notification received (toast / center); mark-read after refresh; optional **Clear all** in the notification drawer.
- No relevant browser console errors.
- Forbidden dependencies check: no EF Core, no Dapper, no MediatR.

## Documentation

- **CI** — on push/PR to `main` or `master`, [`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs backend build/tests (with SQL Server service), frontend build, and forbidden-dependency checks.
- [`docs/reference-troubleshooting.md`](docs/reference-troubleshooting.md) — Docker/SQL, DB reset, MSB3027 file locks, integration tests, forbidden deps, optional API Docker image
- [`docs/reference-security.md`](docs/reference-security.md) — passwords, JWT, isolation, SQL parameters, secrets (exercise scope)
- [`docs/reference-testing-requirements.md`](docs/reference-testing-requirements.md) — requirements traceability, test inventory, forbidden-deps checks, known gaps
- [`docs/reference-credentials.md`](docs/reference-credentials.md) — local URLs, demo user, SQL, JWT (dev)
- [`docs/guide-architecture.md`](docs/guide-architecture.md)
- [`docs/guide-design-decisions.md`](docs/guide-design-decisions.md)
- [`docs/process-ai-usage.md`](docs/process-ai-usage.md)
- [`docs/process-development-log.md`](docs/process-development-log.md)
- [`docs/guide-presentation.md`](docs/guide-presentation.md)
- [`docs/final-project-review.md`](docs/final-project-review.md) — pre-submission readiness summary (aligned with shipped code at last doc pass)
- **[`scripts/verify-backend.ps1`](scripts/verify-backend.ps1)** — optional PowerShell helper: `dotnet build` + `dotnet test` from repo root (see Quick start; stop API if MSB3027)
