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
  - `docs/guide-architecture.md`
  - `docs/guide-design-decisions.md`
  - `docs/process-ai-usage.md`
  - `docs/guide-presentation.md`
  - `docs/process-development-log.md`

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

- Rewrote and expanded `docs/process-ai-usage.md` with Step 1/Step 2 context, validation narrative, corrections (`TaskItemStatus`, `Result`), forbidden-deps checklist commands, and planned JWT/SignalR validation notes.
- Expanded `docs/guide-architecture.md` with **Current implementation status**, planned auth/data/notification flows (diagrams), and careful wording not to overstate implementation.
- Expanded `docs/guide-presentation.md` with overview, architecture/testing summaries, GenAI summary pointer, trade-offs, future improvements.
- Expanded `docs/guide-design-decisions.md` with JWT, FluentValidation, controllers, Result modeling, test tooling, Docker, SignalR stack rationale.
- Added historical clarification under Step 1 in `docs/process-development-log.md` regarding superseded TODOs.

### Validation performed

- Manual editorial review for consistency across docs (English-only, aligned with current code reality).

### AI involvement

Cursor drafted documentation revisions based on a structured reviewer checklist from the human collaborator.

## 2026-05-09 — Step 4: HTTP API (controllers, FluentValidation, middleware)

### What was requested

Expose REST endpoints for auth (register/login), health, and authenticated task CRUD; add FluentValidation for request models; add correlation ID and global exception middleware; enable CORS for the Angular dev origin.

### What was generated/changed

- **Controllers:** `AuthController` (`POST /api/auth/register`, `POST /api/auth/login`), `TasksController` (task CRUD under `/api/tasks` with `[Authorize]`), `HealthController` (`GET /api/health`).
- **Validation:** FluentValidation validators for `RegisterRequest`, `LoginRequest`, `CreateTaskRequest`, `UpdateTaskRequest`; automatic validation enabled.
- **Middleware:** `CorrelationIdMiddleware` (`X-Correlation-ID`), `ExceptionHandlingMiddleware` (JSON 500 body).
- **API wiring:** JSON enums as strings; database initializer runs immediately after `WebApplication` build; CORS policy `AngularDev` from `Cors:Origins` or default `http://localhost:4200`.
- **Docs:** `README.md` route table; `docs/guide-architecture.md` status and auth/data sections updated to reflect implemented HTTP surface.

### Technical decisions

- Map Application-layer `Result` failures to HTTP statuses (e.g. duplicate email → **409**, login failure → **401**, missing task → **404**, delete success → **204**).
- User id resolved from JWT `NameIdentifier` / `sub` claims via `ClaimsPrincipalExtensions.TryGetUserId`.
- Removed FluentValidation clientside adapters registration (API-only).

### Validation performed

- `dotnet build backend/TaskManager.sln` — 0 warnings, 0 errors.
- `dotnet test backend/TaskManager.sln` — 8 unit tests passed.

### Issues / TODO

- Integration tests (`WebApplicationFactory`), Angular SPA, SignalR/notifications remain future work.

### Amendment (Swagger UI)

- Added **Swashbuckle.AspNetCore** — Swagger UI at **`/swagger`** when `ASPNETCORE_ENVIRONMENT=Development`, including JWT Bearer scheme for Try-it-out on `/api/tasks`.

### AI involvement

Cursor implemented Step 4 API layer and documentation updates described above.

## 2026-05-09 — Step 3: Infrastructure (ADO.NET), database initializer, seed, JWT issuance

### What was requested

Implement the Infrastructure layer: parameterized ADO.NET repositories, optional master connection for database creation, schema initializer, demo seed user/tasks, BCrypt password hashing, JWT token signing (`JwtTokenService`), dependency injection (`AddInfrastructure`), API startup wiring with JWT bearer validation (controllers still deferred).

### What was generated/changed

- **Packages:** `Microsoft.Data.SqlClient`, BCrypt, JWT/token packages on Infrastructure; `Microsoft.AspNetCore.Authentication.JwtBearer` on Api.
- **Options:** `DatabaseOptions`, `JwtOptions`; connection strings and JWT settings in `appsettings*.json` (Development aligned with Docker Compose SA password).
- **Data:** `IDbConnectionFactory`, `SqlConnectionFactory`.
- **Persistence:** `UserRepository`, `TaskRepository`, `NotificationRepository`, `DatabaseInitializer` (tables `Users`, `TaskItems`, `Notifications`; indexes; FKs).
- **Seed:** `DemoSeed` constants; inserts demo user and two tasks when absent.
- **Security:** `PasswordHasher`, `JwtTokenService`.
- **Composition:** `DependencyInjection.AddInfrastructure` registers repositories, services, initializer.
- **Api:** `Program.cs` runs initializer on startup, enables authentication/authorization middleware; minimal `/` health-style JSON until controllers exist.
- **Docs/README:** `README.md` demo credentials + `dotnet run`; `docs/guide-architecture.md` status table updated for honesty.

### Technical decisions

- **Parameterized SQL only** in repositories; no string concatenation of user input.
- **BCrypt** for password hashing (assignment forbids EF/Dapper/MediatR, not BCrypt).
- **Symmetric JWT (HS256)** with configurable signing key length guard (minimum 32 UTF-8 bytes).
- **Database creation** optional via `MasterConnectionString`; catalog name taken from application connection string builder.

### Validation performed

- `dotnet build backend/TaskManager.sln` — 0 warnings, 0 errors.
- `dotnet test backend/TaskManager.sln` — 8 unit tests passed; integration tests still absent.
- Manual review: no EF Core / Dapper / MediatR package references added.

### Issues / TODO

- HTTP controllers (auth/register/login, tasks CRUD, health), FluentValidation, exception/correlation middleware, integration tests, Angular, SignalR remain future steps.

### AI involvement

Cursor implemented the Infrastructure and startup wiring described above.

## 2026-05-09 — Session checkpoint note (before PC restart)

Added `docs/process-session-checkpoint.md`: Step 3 remains **uncommitted** until local Docker + `dotnet run` verification; lists resume steps, suggested commit message, and Step 4 pointer. Solution builds clean (`dotnet build`) at time of note.

## 2026-05-09 — End-of-day handoff

### What was requested

Refresh **`docs/process-session-checkpoint.md`** so work can resume tomorrow with a clear summary, git pointer, and next milestones.

### What was generated/changed

- **`docs/process-session-checkpoint.md`:** evening wrap (Steps 3–4 + Swagger on `master`, commit **`34d2aba`**), tomorrow startup steps, reminder that persistence is **ADO.NET only** (no EF/Dapper/MediatR), config vs hardcoded demo seed note.

### Validation performed

- `git status` — clean working tree before commit (aside from this documentation edit).

### AI involvement

Cursor updated the checkpoint document per user request to wrap the session for the day.

## 2026-05-10 — Requirements traceability, forbidden-deps audit, doc refresh

### What was requested

Close out submission readiness: verify **forbidden dependencies** (no EF Core, Dapper, MediatR), add **test documentation** mapping exercise requirements to automated tests, align **architecture** and **presentation** docs with the implemented Angular + SignalR + integration test reality, and note sensible optional follow-ups without over-scoping.

### What was generated/changed

- **`docs/reference-testing-requirements.md`:** consolidated requirement table (from README/presentation), full inventory of **8** unit + **12** integration tests with file references, manual/smoke gaps, repeatable forbidden-deps commands with **2026-05-10** grep/`dotnet list` results, optional CI / server-side read-note.
- **`docs/guide-architecture.md`:** status table updated (Angular, SignalR, integration tests **implemented**); notification section marked **implemented**; ADO.NET constraint wording fixed (“once implemented” removed).
- **`docs/guide-presentation.md`:** overview, API line, demo step 7, testing table, trade-offs row for SignalR, smoke checklist, and talking points updated to match current codebase.
- **`README.md`:** Realtime paragraph mentions notification center; Documentation list links the new testing doc.

### Validation performed

- Searched all `backend/**/*.csproj` for `EntityFramework`, `Dapper`, `MediatR`: **no matches**.
- `dotnet list TaskManager.sln package --include-transitive` filtered for EF/Dapper/MediatR: **no matches**.
- `dotnet test backend/TaskManager.sln` — **20** tests passed (8 unit, 12 integration).

### Issues / TODO

- **Human:** run README final browser smoke (user-owned).
- **Optional:** CI forbidden-package step; API to persist notification “read” if reviewers expect cross-session unread state.

### AI involvement

Cursor drafted the traceability document and updated related docs per the collaborator’s checklist; no product code changes in this entry.

## 2026-05-10 — Documentation layout: `brief/` + naming convention

### What was requested

Centralize exercise prompts with project docs; replace the misspelled `stater-documents` folder; adopt consistent names (`guide-*`, `reference-*`, `process-*`).

### What was generated/changed

- **`docs/brief/`** — English and Portuguese exercise specs (`ballast-lane-task-manager.en.txt`, `.pt-BR.txt`); removed **`docs/stater-documents/`**.
- **Renamed** project markdown files: `guide-architecture.md`, `guide-design-decisions.md`, `guide-presentation.md`, `reference-credentials.md`, `reference-testing-requirements.md`, `process-ai-usage.md`, `process-development-log.md`, `process-session-checkpoint.md`.
- **`docs/README.md`** — table of contents; **`README.md`** Documentation section updated with links.

### Validation performed

- Grep for legacy `docs/architecture.md` paths in active docs (should find only `guide-*` targets); spot-check links.

### AI involvement

Cursor performed moves/renames and link updates.

## 2026-05-10 — Move root `starter-documents/` into `docs/brief/source/`

### What was requested

Consolidate the remaining **`starter-documents/`** folder (PDFs + DOCX) with the rest of documentation.

### What was generated/changed

- **`docs/brief/source/`** — `ballast-lane-technical-exercise-v5.pdf`, `ballast-lane-job-description-senior-net.pdf`, `ballast-lane-design-thought-process.docx` (renamed from long filenames for clarity).
- Removed empty **`starter-documents/`** at repo root.
- **`docs/README.md`** and **`reference-testing-requirements.md`** updated to point at official brief materials.

### Validation performed

- `git status` — expected renames only.

### AI involvement

Cursor moved tracked binaries with `git mv` and updated doc links.

## 2026-05-10 — Pre-submission documentation alignment + developer validation log

### What was requested

Finalize submission-facing documentation: remove misleading **“planned”** wording for JWT, FluentValidation, **`WebApplicationFactory`**, SignalR, and related features that are already shipped; **align published test counts** with the repository; add **`GET /api/notifications`** to the README route table and an explicit **prerequisites** list; align **`docs/process-ai-usage.md`** with **historical vs current** validation narrative; record **developer-performed** final manual smoke and **`dotnet test`** validation (not assistant-executed).

### What was generated/changed

- **`docs/guide-design-decisions.md`**, **`docs/guide-architecture.md`**, **`docs/guide-presentation.md`**, **`docs/process-ai-usage.md`**, **`README.md`**, and this log — wording and checklists updated to match the codebase.

### Validation performed (developer-owned)

**Manual smoke (developer):** Docker SQL Server started successfully; backend API and database initialization OK; Swagger OK; user register/login OK; JWT-protected endpoints rejected unauthenticated calls; authenticated task CRUD OK; **user isolation** validated; frontend start OK; login/register and task CRUD from UI OK; **SignalR notifications** OK; **notification mark-read** persisted correctly; browser console had no relevant errors; **`dotnet test .\backend\TaskManager.sln`** run/checked by the developer as part of final validation.

### AI involvement

Cursor applied the documentation edits above per collaborator request; **no application code changes** were required for this pass.

## 2026-05-10 — Docs pass: runbook/security, API error shape, Docker, test counts in guides

### What was requested

Continue the project plan after task list filtering + pagination: add **troubleshooting** + **security** reference docs, align **500** exception JSON with controller **`error`** field, optional **backend Dockerfile**, README/doc index links, and refresh **test totals** everywhere (**9** unit + **16** integration = **25**).

### What was generated/changed

- **`docs/reference-troubleshooting.md`**, **`docs/reference-security.md`** (new); linked from **`README.md`**, **`docs/README.md`**, **`reference-credentials.md`**.
- **`ExceptionHandlingMiddleware`** — JSON property **`message`** renamed to **`error`** for consistency with other API errors.
- **`backend/Dockerfile`** (multi-stage .NET 8, port **8080**); README **“Optional: API Docker image”** subsection.
- **`docs/reference-testing-requirements.md`** — table rows for paging + list tests; **25** total.
- **`docs/guide-architecture.md`**, **`docs/guide-presentation.md`**, **`docs/process-ai-usage.md`**, **`docs/guide-design-decisions.md`** — counts and coverage wording aligned to **9 / 16 / 25** (incl. list filter + pagination).

### Validation performed

- `dotnet test .\backend\TaskManager.sln` after build (assistant run; SQL Server available; API not holding file locks).

### AI involvement

Cursor authored the docs/middleware/Dockerfile edits and grep-driven consistency pass on guide and review tables.

## 2026-05-10 — Submission documentation trim

### What was requested

Commit and open a PR; reduce documentation to what the exercise needs; confirm planning/decision/process docs remain accurate; keep a clear manual validation list for the author.

### What was generated/changed

- **Removed** **`docs/final-project-review.md`** (long internal audit; requirements coverage already in **`reference-testing-requirements.md`** and guides).
- **Removed** **`docs/process-session-checkpoint.md`** (stale handoff; history remains in this log).
- **`docs/README.md`** — single index aligned to exercise deliverables + operational references.
- **`README.md`** — smoke section points at **`process-development-log.md`** and **`reference-testing-requirements.md`** instead of the removed review doc.
- **`docs/guide-presentation.md`** — **Future improvements** CI bullet updated (workflow already exists).

### Validation performed

- Link sweep for removed filenames in active docs and root README.

### AI involvement

Cursor applied removals/edits and prepared git commit + PR instructions.

## 2026-05-10 — Auth redirect and task 404 UX

### What was requested

Polish before a new commit: avoid blocking login/register on SignalR; improve missing-task UX after manual isolation testing.

### What was generated/changed

- **Login / register:** Navigate immediately after auth succeeds; **`primeConnection()`** runs without gating navigation (fixes stuck sign-in when **`hub.start()`** hangs).
- **Task edit load:** On **404**, redirect to **`/tasks`** with a flash message styled as an error banner.
- **Task list:** Flash banners support **success** vs **error** via **`sessionStorage`** key **`taskManager.flashBanner`**.
- **`angular.json`:** Production **`anyComponentStyle`** budgets relaxed slightly so `ng build` stays clean with current shell SCSS size.
- **README / troubleshooting:** Earlier pass added **`npm install`** before **`npm start`** and Angular troubleshooting section.

### Validation performed

- **`npm run build`** (frontend). **`dotnet test`** requires stopping the API when DLL copy locks occur (MSB3027).

### AI involvement

Cursor implemented and smoke-checked the Angular changes.
