# Final Project Review

This document summarizes the **current state** of the TaskManager monorepo for pre-submission review (requirements coverage, risks, inconsistencies, and improvements). It was first produced from **repository inspection** and **amended** after documentation alignment and **developer-reported** manual validation (see opening notes and §7, §10).

**Note:** This review was first produced from **repository inspection only**; some automated checks (e.g. full `dotnet test` in the review environment, GitHub visibility) were **not** executed by the reviewer.

**Post-review validation (developer-owned, manual):** After this document was generated, the **developer** performed final manual smoke testing and test validation locally (see §7, §10, and `docs/process-development-log.md`). Those results are recorded here as **reported by the developer**, not as assistant-verified automation.

---

## 1. Project Overview

### What the project does

A **full-stack task management** application: users **register and log in**, then **create, list, view, update, and delete** tasks scoped to their account. Task mutations generate **persisted notifications** in SQL Server and **real-time pushes** via **SignalR** to the Angular client (toasts + notification center).

### Main user story

Authenticated users manage **their own** tasks (CRUD) with JWT-protected APIs; notifications inform them of task activity (aligned with `docs/guide-presentation.md`).

### Main features implemented

- **Auth:** Register, login, JWT issuance, protected task and notification routes (`backend/src/TaskManager.Api/Controllers/AuthController.cs`, `TasksController.cs`, `NotificationsController.cs`).
- **Tasks:** Full CRUD with user isolation enforced in application/repository layers (`backend/src/TaskManager.Application/Services/TaskService.cs`, persistence under `TaskManager.Infrastructure/Persistence/`).
- **Notifications:** SQL persistence, background dispatch via `Channel<T>` + hosted worker, SignalR hub (`backend/src/TaskManager.Infrastructure/Notifications/NotificationDispatchWorker.cs`, `TaskManager.Infrastructure/SignalR/NotificationsHub.cs`, `Program.cs`).
- **Mark read:** `POST /api/notifications/mark-read` updates `IsRead` in the database (`NotificationsController.cs`, `NotificationRepository.cs`); Angular syncs UI to server `isRead` (`frontend/task-manager-web/src/app/core/notifications/notification-center.service.ts`).
- **Frontend:** Angular SPA — login/register, task list/form, guards, JWT interceptor, SignalR + notification drawer (`frontend/task-manager-web/src/app/`).

### Backend stack

- **.NET 8**, ASP.NET Core **Web API** with **controllers**
- **FluentValidation** for request validation (`TaskManager.Api`, validators under `backend/src/TaskManager.Api/Validation/`)
- **JWT Bearer** authentication (`Program.cs`, `JwtTokenService` in Infrastructure)
- **SQL Server** + **ADO.NET** (`Microsoft.Data.SqlClient` in `TaskManager.Infrastructure.csproj`)
- **SignalR** with camelCase JSON payloads for hub messages (`Program.cs`)
- **Swagger** in Development (`Program.cs`)
- **xUnit** + **Moq** (unit) + **Microsoft.AspNetCore.Mvc.Testing** (integration)

### Frontend stack

- **Angular** (application under `frontend/task-manager-web/`)
- **@microsoft/signalr** for realtime (see `package.json` and `core/realtime/signalr-notifications.service.ts`)
- **`HttpClient`** with **JWT interceptor** (`core/auth/auth.interceptor.ts`)
- **Reactive forms** on auth and task pages

### Database / data storage approach

- **SQL Server** (Docker Compose at repo root `docker-compose.yml`)
- **Schema + seed** applied at API startup via **`DatabaseInitializer`** (`backend/src/TaskManager.Infrastructure/Persistence/DatabaseInitializer.cs`)
- Tables include at least **`Users`**, **`TaskItems`**, **`Notifications`** (evident from initializer/repositories)

### Testing approach

- **Unit tests:** Application services (`AuthService`, `TaskService`) with **Moq** — `backend/tests/TaskManager.UnitTests/`
- **Integration tests:** **WebApplicationFactory**, real HTTP pipeline + SQL — `backend/tests/TaskManager.IntegrationTests/`
- **Manual:** README smoke checklist; no Playwright/Cypress in repo (`docs/reference-testing-requirements.md`)

### Documentation files included

| Path | Role |
|------|------|
| `README.md` | Runbook, routes, stack, restrictions |
| `docs/README.md` | Index of docs + naming convention |
| `docs/guide-architecture.md` | Architecture + status tables, diagrams |
| `docs/guide-design-decisions.md` | Rationale |
| `docs/guide-presentation.md` | Demo / interview outline |
| `docs/process-ai-usage.md` | GenAI usage audit |
| `docs/process-development-log.md` | Milestone log |
| `docs/final-project-review.md` | Pre-submission review (requirements, risks, validation notes) |
| `docs/process-session-checkpoint.md` | Optional handoff (may be stale) |
| `docs/reference-credentials.md` | URLs, demo user, SQL/JWT dev notes |
| `docs/reference-testing-requirements.md` | Requirements ↔ tests traceability |
| `docs/brief/` | Exercise prompts (txt) + PDFs/DOCX in `docs/brief/source/` |

---

## 2. Assignment Requirements Coverage

Legend: **Completed** | **Partially completed** | **Not completed** | **Evidence**

| Requirement | Status | Evidence |
|-------------|--------|----------|
| .NET / C# / ASP.NET Web API | **Completed** | `backend/TaskManager.sln`, `TaskManager.Api` net8.0 Web SDK (`TaskManager.Api.csproj`), controllers under `Controllers/` |
| Clean Architecture | **Completed** | Projects `Domain`, `Application`, `Infrastructure`, `Api`; Infrastructure → Application → Domain (`docs/guide-architecture.md`, project references in `.csproj` files) |
| CRUD operations (tasks) | **Completed** | `TasksController.cs`, `TaskService.cs`, integration test `Task_crud_round_trip_for_new_user` |
| User creation | **Completed** | `POST /api/auth/register` (`AuthController.cs`, `AuthService.cs`) |
| User login | **Completed** | `POST /api/auth/login`, JWT in response |
| Authorized endpoints | **Completed** | `[Authorize]` on tasks/notifications; JWT in `Program.cs` |
| Non-authorized endpoints | **Completed** | `[AllowAnonymous]` on register/login; `GET /api/health` (and `/` root JSON in `Program.cs`) |
| DB with user storage | **Completed** | `User` entity, `UserRepository.cs`, `DatabaseInitializer.cs` |
| DB with application data | **Completed** | `TaskItem`, `Notification` entities + repositories |
| Custom data access layer (no EF/Dapper) | **Completed** | `Microsoft.Data.SqlClient` only in Infrastructure; parameterized SQL in repositories |
| Business logic independent of API/data details | **Completed** | `TaskService` / `AuthService` in Application; interfaces in `Abstractions/`; unit tests mock repositories |
| Frontend integration | **Completed** | `frontend/task-manager-web` consumes REST + SignalR |
| Responsive / user-friendly frontend | **Partially completed** | Global styles, layout, table wrap (`styles.scss`, `app.component.scss`, task list); no dedicated mobile/breakpoint audit documented — reasonable dev UX, not a design system |
| Structured frontend code | **Completed** | `core/` (auth, tasks, notifications, realtime), `pages/`, `app.routes.ts`, `app.config.ts` |
| Unit tests | **Completed** | 8 tests in `TaskManager.UnitTests` (`AuthServiceTests.cs`, `TaskServiceTests.cs`) |
| Integration tests | **Completed** | 13 tests in `TaskManager.IntegrationTests` (see §7); `WebApplicationFactory` setup in `IntegrationTestWebApplicationFactory.cs` |
| README with setup instructions | **Completed** | `README.md` — Docker, build, run API, run frontend, run tests |
| Seed data / demo credentials | **Completed** | `DemoSeed` / initializer + README demo table; `docs/reference-credentials.md` |
| GenAI usage documentation | **Completed** | `docs/process-ai-usage.md` (path renamed from generic `ai-usage.md`) |
| Presentation / thought process | **Completed** | `docs/guide-presentation.md`, `docs/guide-design-decisions.md` |
| One repository only | **Completed** | Single monorepo: `backend/`, `frontend/`, `docs/` |

**Documentation alignment (post-review):** README route table, “planned” wording in design/AI/architecture/presentation docs, and integration test counts were updated to match the shipped implementation (see `docs/process-development-log.md` latest entry).

---

## 3. Forbidden Dependencies Check

**Assignment rule:** no **Entity Framework / EF Core**, **Dapper**, **MediatR** / generic **mediator** libraries.

### Verification performed (inspection-based)

1. **Glob search** on all `backend/**/*.csproj` for `EntityFramework`, `Dapper`, `MediatR`: **no matches**.
2. **Grep** on all `backend/**/*.cs` for `EntityFrameworkCore`, `Dapper`, `MediatR`, `Microsoft.EntityFramework`: **no matches**.
3. **`dotnet list TaskManager.sln package --include-transitive`** piped to filter for EF/Dapper/MediatR: **no output** (review run during document prep: exit code 0, empty filter result).

### Representative package references (not forbidden)

- **Infrastructure:** `Microsoft.Data.SqlClient`, `BCrypt.Net-Next`, JWT packages (`TaskManager.Infrastructure.csproj`)
- **Api:** `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore` (`TaskManager.Api.csproj`)

### Suspicious references

**None found** under the forbidden list. **FluentValidation** and **Swagger** are allowed and expected.

**Ongoing recommendation:** Re-run the `dotnet list ... | Select-String` (or equivalent) before final submit after any package change.

---

## 4. Architecture Review

### Folder / project structure

```
backend/
  src/ TaskManager.Domain | Application | Infrastructure | Api
  tests/ TaskManager.UnitTests | TaskManager.IntegrationTests
frontend/
  task-manager-web/   (Angular app)
docs/                 (guides, process logs, brief materials)
docker-compose.yml
README.md
```

### Layer responsibilities

- **Domain:** Entities and enums — `backend/src/TaskManager.Domain/`
- **Application:** Services, DTOs, `Result`, abstractions — `TaskManager.Application/`
- **Infrastructure:** ADO.NET repositories, seed, JWT/password implementations, SignalR worker, DI extension — `TaskManager.Infrastructure/`
- **Api:** Controllers, validation, middleware, auth/CORS/Swagger/SignalR map — `TaskManager.Api/`

### Dependency direction

**Api** → Application + Infrastructure; **Infrastructure** → Application + Domain; **Application** → Domain only (see diagram in `docs/guide-architecture.md`).

### Frontend structure

- **Shell:** `app.component.*`, routes in `app.routes.ts`
- **Feature pages:** `pages/login`, `pages/register`, `pages/tasks`
- **Core:** `core/auth`, `core/tasks`, `core/notifications`, `core/realtime`, `core/http-api-error.ts`

### Documentation structure

See §1 table; files use **`guide-*`**, **`reference-*`**, **`process-*`**, and **`brief/`** per `docs/README.md`.

### Documentation drift (resolved)

**Follow-up pass:** Integration test count (**13**), JWT/FluentValidation/WebApplicationFactory and SignalR wording, README **`GET /api/notifications`**, and AI-usage “planned” sections were aligned with the codebase. Any remaining “planned” wording elsewhere should refer only to **optional future** work (not shipped features).

---

## 5. Backend Review

### Controllers / endpoints

| Area | File | Summary |
|------|------|---------|
| Health | `HealthController.cs` | Health endpoint for liveness |
| Auth | `AuthController.cs` | `register`, `login` (anonymous) |
| Tasks | `TasksController.cs` | CRUD under `/api/tasks`, authorized |
| Notifications | `NotificationsController.cs` | `GET` list, `POST mark-read`, authorized |

**Evidence:** `backend/src/TaskManager.Api/Controllers/*.cs`, routes summarized in `README.md` (includes `GET /api/notifications` and `POST /api/notifications/mark-read`).

### Auth / JWT

- JWT bearer configuration and `access_token` query for SignalR path — `Program.cs`
- Token creation — `Infrastructure/Security/JwtTokenService.cs`
- User id for SignalR — `SignalRUserIdProvider.cs`

### Password hashing

- **BCrypt** via Infrastructure — `PasswordHasher` (referenced from `DependencyInjection.cs` / auth flow; package `BCrypt.Net-Next` in Infrastructure csproj).

### Validation

- **FluentValidation** validators for auth/task requests — `backend/src/TaskManager.Api/Validation/`
- Application-layer validation outcomes via **`Result`** — e.g. `TaskService`, `AuthService`

### Error handling / correlation

- **Global exception middleware** — `Middleware/ExceptionHandlingMiddleware.cs`
- **Correlation id middleware** — `Middleware/CorrelationIdMiddleware.cs`
- Wired in `Program.cs` before CORS/auth pipeline

### ADO.NET data access

- **Connection factory** — `Infrastructure/Data/SqlConnectionFactory.cs`
- **Repositories** — `UserRepository`, `TaskRepository`, `NotificationRepository` under `Persistence/`
- **Parameterized SQL** — consistent use of `SqlParameter` in repository code reviewed (`NotificationRepository.cs`, etc.)

### Database initialization and seed

- **`DatabaseInitializer`** creates schema and applies seed/demo user and tasks — `Persistence/DatabaseInitializer.cs`
- Demo constants — `Infrastructure/Seed/DemoSeed.cs` (referenced in tests and README)

### SignalR / notifications / background processing

- Hub: **`NotificationsHub`** — `Infrastructure/SignalR/NotificationsHub.cs` (minimal hub; authorization at connection)
- Map: **`/hubs/notifications`** — `Program.cs`
- Dispatch: **`NotificationDispatchWorker`** reads channel, persists notification, sends to **`Clients.User(...)`** — `Infrastructure/Notifications/NotificationDispatchWorker.cs`
- Publisher/channel registration — `DependencyInjection.cs` (Infrastructure), `INotificationPublisher` / `TaskService` hooks

### Known limitations (honest)

- **SignalR** delivery to the browser is **not** covered by automated WebSocket tests; **developer-reported manual smoke** confirmed realtime notifications and mark-read persistence in the UI — `docs/reference-testing-requirements.md` still describes the automated gap.
- **In-memory `Channel<T>`** is not durable across process restarts (acceptable for exercise; called out in design docs).
- **`dotnet build` / `dotnet test`** can fail with **file locks** if **`TaskManager.Api` is running** — documented in `reference-testing-requirements.md`.

---

## 6. Frontend Review

### Structure

- **Angular** app root: `frontend/task-manager-web/`
- **Routes:** `src/app/app.routes.ts` — tasks child routes (`''`, `new`, `:id`), login/register with guards

### Pages / components

- **Login / Register** — `pages/login/*`, `pages/register/*`
- **Task list / form** — `pages/tasks/task-list.component.*`, `task-form.component.*`

### Services

- **Auth:** `core/auth/auth.service.ts`, `auth-token.store.ts`, `auth.storage.ts`
- **Tasks:** `core/tasks/tasks.service.ts`
- **Notifications:** `core/notifications/notification-center.service.ts`, `notification.models.ts`
- **SignalR:** `core/realtime/signalr-notifications.service.ts`

### Auth flow

- Login/register → **stored auth** (token + user metadata) — `auth.storage.ts` / `AuthTokenStore`
- **`authInterceptor`** attaches Bearer token — `auth.interceptor.ts`
- **`authGuard`** / **`guestGuard`** — `auth.guard.ts`, `guest.guard.ts`

### Task CRUD flow

- List → navigate to **new** or **:id** — `TasksService` for HTTP
- After create/update: navigate to list, optional flash banner — `task-form.component.ts`, `task-list.component.ts` (session flash pattern)

### Notifications / SignalR

- **App shell** hosts bell, drawer, toasts — `app.component.html`, `app.component.ts`
- **`SignalRNotificationsService`**: connect, toast, merge into `NotificationCenterService`
- **`APP_INITIALIZER`** + **`primeConnection`** after login — `app.config.ts`, `login.component.ts`, `register.component.ts`
- **Read state** persisted via **`POST /api/notifications/mark-read`** — `notification-center.service.ts`

### Loading / error handling

- **Signals** for loading/error on forms and list
- **`http-api-error.ts`** for normalized API errors (used on auth flows)

### Responsiveness / UX

- Desktop-first layout with max-width container — `app.component.scss` (`.main`)
- Tables scroll horizontally when needed — `task-list.component.scss` (`.table-wrap`)
- Theming / Ballast-style palette — `frontend/task-manager-web/src/styles.scss`, branded nav — `app.component.html`

### Known limitations

- No E2E tests; **developer manual smoke** confirmed SignalR notifications and mark-read behavior in the browser (not assistant-verified).
- **Angular budget warning:** `app.component.scss` slightly over style budget (warning on build) — low risk, optional cleanup before submission if CI treats warnings as errors.

---

## 7. Tests Review

### Unit tests (8)

**File:** `backend/tests/TaskManager.UnitTests/AuthServiceTests.cs`, `TaskServiceTests.cs`  
**Covers:** duplicate email, bad password, happy login, task title/status validation, ownership on update, delete not found.

### Integration tests (13)

**Files:** `backend/tests/TaskManager.IntegrationTests/`

| Test file | Role |
|-----------|------|
| `HealthApiTests.cs` | Basic API up |
| `AuthApiTests.cs` | Demo login, register+login flow, invalid password |
| `TasksApiTests.cs` | 401 without auth, demo seeded tasks, full CRUD round-trip, **user isolation** |
| `NotificationsApiTests.cs` | 401, list OK, async notification after task create, **mark-read persistence** |

### Edge cases well covered

- **User isolation** — integration test `Another_user_cannot_read_update_or_delete_peer_task`
- **Auth boundaries** — 401 on tasks/notifications without token
- **Notification pipeline** — persistence polling test; **IsRead** persistence test

### Not covered (important gaps by design)

- **Browser/E2E** — no Playwright/Cypress
- **SignalR** end-to-end frame delivery — manual
- **Load / concurrency** — not in scope

### Current test results

- **During review (assistant):** A fresh full **`dotnet test .\backend\TaskManager.sln`** was **not** executed in the review environment—**could not verify** automated results at that time.
- **`docs/reference-testing-requirements.md`** describes **21 tests (8 unit + 13 integration)** and the MSB3027 lock issue when the API is running.
- **Developer-reported (post-review, manual validation):** The developer ran **`dotnet test .\backend\TaskManager.sln`** as part of final validation and confirmed tests pass (stop `TaskManager.Api` first if copy-lock errors occur). This is **manual/automation validation by the developer**, not a claim that the assistant re-ran the suite.

**Command:**

```powershell
dotnet test .\backend\TaskManager.sln
```

(Stop `TaskManager.Api` first if you see MSB3027 copy errors.)

---

## 8. GenAI Documentation Review

Paths below reflect **current repo naming** (`process-*`, `guide-*`). The assignment asked for names like `docs/ai-usage.md`; content lives in **`docs/process-ai-usage.md`**.

| Document | What it contains | Aligned with assignment? | Notes |
|----------|------------------|---------------------------|-------|
| **`docs/process-ai-usage.md`** | Tools, GenAI narrative, validation, forbidden-deps | **Yes** | **Historical vs current** validation narrative; assignment compliance + ongoing package review commands |
| **`docs/process-development-log.md`** | Chronological implementation log | **Yes** | Latest entry records doc alignment + **developer** manual smoke / tests |
| **`docs/guide-design-decisions.md`** | Rationale | **Yes** | JWT, FluentValidation, **`WebApplicationFactory`** described as **implemented** |
| **`docs/guide-architecture.md`** | Layers, status, diagrams | **Yes** | **13** integration tests; SignalR hub mapping described as shipped |
| **`docs/guide-presentation.md`** | Demo script | **Yes** | **21** tests (8 + **13**); **developer** manual smoke called out |
| **`README.md`** | Runbook | **Yes** | Prerequisites; **`GET /api/notifications`** + mark-read in route table |

---

## 9. Setup and Run Instructions (README)

| Topic | Present in `README.md`? | Notes |
|-------|-------------------------|-------|
| Prerequisites (.NET 8, Node/npm, Docker Desktop) | **Yes** | Explicit list in `README.md` |
| Start SQL / Docker Compose | **Yes** | `docker compose up -d sqlserver` |
| Run backend | **Yes** | `dotnet run --project ...TaskManager.Api.csproj` |
| Run frontend | **Yes** | `cd frontend/task-manager-web`; `npm start` |
| Run tests | **Yes** | `dotnet test` |
| Demo credentials | **Yes** | Table + pointer to `docs/reference-credentials.md` |
| Main URLs / SignalR | **Yes** | Base URL note + SignalR section |

**Residual (submitter):** Confirm **GitHub** visibility and **no secrets** before publish—the assistant cannot verify remote settings from the repo alone.

---

## 10. Final Smoke Test Checklist (manual, before submission)

Use this as a **last run** before handing the repo to reviewers.

**Developer-completed smoke (reported after review):** The items below were **manually validated by the developer** on their machine (not executed or witnessed by the assistant). They are shown checked for **historical record**; another machine or reviewer may still re-run the list.

- [x] **Docker:** `docker compose up -d sqlserver` succeeded; container healthy.
- [x] **Backend:** API started without exceptions; DB initializer ran.
- [x] **Swagger:** opened successfully in Development; JWT authorize workflow OK.
- [x] **Register** — worked (API/UI as exercised).
- [x] **Login** — worked; JWT-protected endpoints returned **401** without token.
- [x] **Tasks:** create, list, update, delete — authenticated flow OK.
- [x] **User isolation** — validated (peer user could not access another user’s task).
- [x] **Frontend:** started; login/register from UI OK; task CRUD from UI OK.
- [x] **Notifications:** SignalR notifications worked; **mark-read** persisted correctly after refresh.
- [x] **Browser console:** no relevant errors reported.
- [x] **Tests:** `dotnet test .\backend\TaskManager.sln` — **manually run/checked by developer** as part of final validation (see §7).
- [x] **Docs:** README route table and doc “planned” drift addressed in follow-up pass.
- [ ] **GitHub:** repo visibility / link readiness — **could not verify** from inspection alone (developer confirms).
- [ ] **Secrets:** confirm **no production secrets** committed; `appsettings.Development.json` uses **dev-only** keys — acceptable for exercise if called out; rotate if you ever reused keys.

---

## 11. Risks and Possible Improvements

### High priority (before submission)

1. ~~**Doc accuracy:**~~ **Done** in follow-up pass: `guide-design-decisions.md`, `guide-architecture.md`, `guide-presentation.md`, `process-ai-usage.md`, `README.md`, `final-project-review.md`, `process-development-log.md`.
2. **Submitter:** Confirm **GitHub** visibility and **no committed secrets**; re-run smoke on a fresh clone if submitting from a different environment.

### Medium priority (if time)

- **`process-session-checkpoint.md`**: delete or mark **archived** if it confuses reviewers.
- **CI:** optional GitHub Action — `dotnet build`, `dotnet test`, forbidden package grep.

### Low priority (polish)

- Resolve **Angular component style budget** warning for `app.component.scss` (89 bytes over) if your pipeline treats warnings as errors.
- **Favicon / title** already branded “BallastLane” in Angular — ensure README intro still matches your demo story (still says “Task Manager” in prose — intentional or align wording).

### Do not change now (risk of regression)

- **Core ADO.NET SQL** and **schema** unless a bug is proven — high regression risk close to deadline.
- **Auth/JWT claim shape** — tied to SignalR `IUserIdProvider` and tests.

---

## 12. Final Submission Readiness

**Assessment:** **Ready to submit**, provided the **developer** completes the remaining **non-code** checks (**GitHub** visibility / link hygiene, **secret** scan, optional **fresh-clone** smoke).

**Why “ready to submit” now (with the above caveat)**

- **Code and features** were already complete at review time; **documentation** was aligned in a follow-up pass to match **shipped** behavior (**13** integration tests, routes, no misleading “planned” JWT/FluentValidation/WAF wording).
- **Developer-reported manual smoke** (Docker, API, DB init, Swagger, auth, task CRUD, isolation, frontend, SignalR, mark-read persistence, console clean, **`dotnet test`**) addresses the gap where the **assistant** could not personally run your environment.

**Residual (assistant could not verify):** Remote **GitHub** settings and workspace **secrets** policy.

---

*Review artifact: **`docs/final-project-review.md`**. Amended after initial generation to record **developer-performed** validation and documentation fixes.*
