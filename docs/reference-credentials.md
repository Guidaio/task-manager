# Local development reference (credentials & URLs)

**Scope:** Development only. Do not use these values in production or expose real secrets in public repos.

---

## Web app (Angular)

| Item | Value |
|------|--------|
| URL | `http://localhost:4200` |
| API base (dev) | `http://localhost:5035` (see `frontend/task-manager-web/src/environments/environment.ts`) |

---

## Demo user (seeded when the API starts against an empty database)

Use these on the login screen or in Swagger (JWT from `/api/auth/login`).

| Field | Value |
|--------|--------|
| Email | `demo@taskmanager.local` |
| Password | `Demo_user_12345` |
| Display name | `Demo User` |
| User id (fixed, for docs/tests) | `11111111-1111-1111-1111-111111111111` |

Seeded task ids (when that user has no tasks yet): `22222222-2222-2222-2222-222222222222`, `33333333-3333-3333-3333-333333333333`.

---

## SQL Server (Docker Compose)

| Item | Value |
|------|--------|
| Host / port | `localhost`, `1433` |
| SA login | `sa` |
| SA password | `TaskManager_dev_12345` |
| Default app database | `TaskManager` |

Start:

```powershell
docker compose up -d sqlserver
```

Connection strings for the API live in `appsettings.Development.json` (same password as Compose).

**Integration tests** use database `TaskManager_IntegrationTests` (see `appsettings.IntegrationTests.json` and test helpers).

---

## TaskManager API

| Item | Value |
|------|--------|
| HTTP | `http://localhost:5035` |
| HTTPS (if you use the `https` profile) | `https://localhost:7212` |
| Health | `GET /api/health` |
| Swagger (Development) | `http://localhost:5035/swagger` |
| SignalR hub | `/hubs/notifications` (JWT via `access_token` query for WebSockets; the Angular client uses `accessTokenFactory`) |

Run:

```powershell
dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj
```

---

## JWT (Development `appsettings.Development.json`)

| Setting | Example / note |
|---------|----------------|
| Issuer | `TaskManager` |
| Audience | `TaskManagerClients` |
| SigningKey | Must be ≥ 32 UTF-8 bytes; current dev key is in appsettings (replace in production via config/secrets). |

For Swagger **Authorize**, paste the **raw JWT** only (no `Bearer ` prefix).

---

## Quick sanity checklist

1. SQL: `docker compose up -d sqlserver`
2. API: `dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj` → listen on `http://localhost:5035`
3. UI: `cd frontend\task-manager-web` → `npm start` → `http://localhost:4200`
4. Login: `demo@taskmanager.local` / `Demo_user_12345`

If build fails with DLL/exe “in use”, stop the running API (Ctrl+C) before `dotnet build` or `dotnet run`.

---

_Source of truth in code: `DemoSeed`, `docker-compose.yml`, `appsettings.Development.json`, `launchSettings.json`, `environment.ts`._
