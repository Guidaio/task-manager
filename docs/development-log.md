# Development Log

## 2026-05-09 16:19 - Step 1: Repository skeleton

### What was requested

Create the initial repository structure, .NET solution and backend projects, test projects, initial documentation files, SQL Server Docker Compose file, README skeleton, and this development log entry. Do not implement Domain, Application, Infrastructure, API endpoints, Angular, or SignalR yet.

### What was generated/changed

- Confirmed `backend/TaskManager.sln` exists.
- Confirmed backend source projects exist:
  - `backend/src/TaskManager.Api`
  - `backend/src/TaskManager.Application`
  - `backend/src/TaskManager.Domain`
  - `backend/src/TaskManager.Infrastructure`
- Confirmed backend test projects exist:
  - `backend/tests/TaskManager.UnitTests`
  - `backend/tests/TaskManager.IntegrationTests`
- Added `docker-compose.yml` with SQL Server 2022 Developer.
- Added `README.md` with stack, restrictions, minimum commands, core green checklist, and final smoke test checklist.
- Added initial docs:
  - `docs/architecture.md`
  - `docs/design-decisions.md`
  - `docs/ai-usage.md`
  - `docs/presentation.md`
  - `docs/development-log.md`

### Technical decisions

The skeleton keeps the planned Clean Architecture boundaries visible before implementation begins. SQL Server is defined in Docker Compose early because database availability is part of the core green checklist. SignalR remains documented but deferred until the core backend, auth, task CRUD, SQL persistence, tests, and README instructions are green.

### Validation performed

- Listed projects in `backend/TaskManager.sln` and confirmed all planned backend and test projects are included.
- Inspected project references to confirm the intended dependency direction is already represented.
- Ran `dotnet build backend/TaskManager.sln`; the backend skeleton built successfully with 0 warnings and 0 errors.
- Tried to validate Docker Compose syntax with `docker compose config`, but Docker is not available in the current environment.

### Issues / TODO

- Frontend folder exists but the Angular app has not been created yet.
- Domain, Application, Infrastructure, API endpoints, database initializer, tests, Angular, and SignalR are intentionally not implemented in Step 1.
- Docker Compose should be validated once Docker is available locally.

### AI involvement

Cursor helped create the Step 1 skeleton documentation and repository support files based on the approved plan and the user's scope clarification.
