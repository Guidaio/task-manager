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

The backend and frontend run commands will be completed when those layers are implemented.

## Demo Credentials

Seed/demo credentials will be added with the database initializer.

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
