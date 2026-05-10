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

## Restrictions

This project must not use:

- Entity Framework or EF Core
- Dapper
- MediatR or mediator libraries

## Repository Structure

```text
backend/
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
docs/
docker-compose.yml
README.md
```

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

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/health` | No |
| POST | `/api/auth/register` | No |
| POST | `/api/auth/login` | No |
| GET | `/api/tasks` | Bearer JWT |
| GET | `/api/tasks/{id}` | Bearer JWT |
| POST | `/api/tasks` | Bearer JWT |
| PUT | `/api/tasks/{id}` | Bearer JWT |
| DELETE | `/api/tasks/{id}` | Bearer JWT |

Send `Authorization: Bearer <token>` from `/api/auth/login` or register for task routes. Enum `status` values serialize as JSON strings (`Pending`, `InProgress`, ...).

### Swagger / OpenAPI (Development)

When `ASPNETCORE_ENVIRONMENT=Development`, browse **`/swagger`** for interactive docs. Click **Authorize**, paste the **JWT only** (no `Bearer ` prefix), then call protected `/api/tasks` endpoints.

## Demo Credentials

After the database initializer runs (API startup with SQL Server available), a demo user is seeded if missing:

| Field | Value |
|--------|--------|
| Email | `demo@taskmanager.local` |
| Password | `Demo_user_12345` |

Fixed demo user id (for reference): `11111111-1111-1111-1111-111111111111`. Two sample tasks are seeded when that user has no tasks.

## Minimum Run Instructions

Start SQL Server:

```powershell
docker compose up -d sqlserver
```

Build the backend:

```powershell
dotnet build .\backend\TaskManager.sln
```

Run backend tests:

```powershell
dotnet test .\backend\TaskManager.sln
```

Run the API (Development profile uses `appsettings.Development.json`; requires SQL Server reachable):

```powershell
dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj
```

Run the Angular client (expects API at `http://localhost:5035`; adjust `src/environments/environment.ts` if your port differs):

```powershell
cd .\frontend\task-manager-web
npm start
```

Browse `http://localhost:4200`. CORS allows this origin against the API.

## Final Smoke Test Checklist

- Frontend login/register.
- Task list/create/edit/delete.
- Notification received.
- No relevant browser console errors.
- Forbidden dependencies check: no EF Core, no Dapper, no MediatR.

## Documentation

- `docs/architecture.md`
- `docs/design-decisions.md`
- `docs/ai-usage.md`
- `docs/development-log.md`
- `docs/presentation.md`
