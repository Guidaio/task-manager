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

### Historical clarification (Step 1 Issues / TODO)

The **Issues / TODO** list under Step 1 is a **historical snapshot** of what was intentionally deferred **at the end of Step 1** only. It is **partially superseded** by later work: **Step 2** implemented Domain, Application, and unit tests (see the Step 2 entry below). As of Step 2, items such as **Infrastructure**, **API** (including JWT HTTP wiring), **database initializer**, **Angular**, **SignalR**, and **integration tests** were still outstanding unless a later log entry states otherwise.

## 2026-05-09 — Step 2: Domain and Application with unit tests

### What was requested

Implement Domain entities and enums, Application DTOs and abstractions, `AuthService` and `TaskService` business logic, and focused unit tests using Moq. No Infrastructure, API endpoints, Angular, or SignalR yet.

### What was generated/changed

- Domain: `User`, `TaskItem`, `Notification`, `TaskItemStatus`, `NotificationType`.
- Application: `Result`/`Result<T>`, auth and task DTOs, repository/token/password contracts, `IAuthService`/`ITaskService`, `INotificationRepository`, `AuthService`, `TaskService`.
- Unit tests: `AuthServiceTests`, `TaskServiceTests` with Moq; removed placeholder `UnitTest1.cs`.
- Added Moq package reference to `TaskManager.UnitTests`.

### Technical decisions

- Application services return `Result` types so controllers can translate failures consistently later without throwing for expected validation failures.
- Task writes validate title and guard against undefined enum values when deserialized from integers.
- Enum renamed to `TaskItemStatus` to avoid clashing with `System.Threading.Tasks.TaskStatus`.

### Validation performed

- Ran `dotnet build backend/TaskManager.sln` with 0 warnings and 0 errors.
- Ran `dotnet test backend/TaskManager.sln`; 8 unit tests passed (`TaskManager.UnitTests`). Integration test project has no tests yet.

### Issues / TODO

- Wire dependency injection, SQL repositories, JWT, and API controllers in subsequent steps.
- Integration tests and Angular remain pending.

### AI involvement

Cursor implemented the Domain/Application layers and unit tests described above.

## 2026-05-09 — Documentation revision (technical test alignment)

### What was requested

Expand and align project documentation with submission expectations before implementing further product features: AI usage audit depth, architecture honesty about planned vs implemented, presentation outline completeness, design rationale gaps, and a historical clarification on Step 1 TODOs.

### What was generated/changed

- Rewrote and expanded `docs/ai-usage.md` with Step 1/Step 2 context, validation narrative, corrections (`TaskItemStatus`, `Result`), forbidden-deps checklist commands, and planned JWT/SignalR validation notes.
- Expanded `docs/architecture.md` with **Current implementation status**, planned auth/data/notification flows (diagrams), and careful wording not to overstate implementation.
- Expanded `docs/presentation.md` with overview, architecture/testing summaries, GenAI summary pointer, trade-offs, future improvements.
- Expanded `docs/design-decisions.md` with JWT, FluentValidation, controllers, Result modeling, test tooling, Docker, SignalR stack rationale.
- Added historical clarification under Step 1 in `docs/development-log.md` regarding superseded TODOs.

### Validation performed

- Manual editorial review for consistency across docs (English-only, aligned with current code reality).

### AI involvement

Cursor drafted documentation revisions based on a structured reviewer checklist from the human collaborator.
