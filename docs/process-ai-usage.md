# AI Usage

This document records how Generative AI tools are used during the project, what was produced, how outputs were reviewed, what was corrected, and how compliance with assignment constraints is verified.

## Tools Used

- **Cursor** — primary assistant for planning, documentation, backend structure, Domain/Application code, and unit tests.
- **ChatGPT** — optional; not used for Steps 1–2 unless noted in a later revision.

---

## Step 1 — Planning and repository skeleton

### Prompt / context (summary)

Paraphrased intent communicated to Cursor:

- Build a full stack Task Manager for a Senior .NET technical exercise: Clean Architecture, ASP.NET Core Web API, SQL Server, plain ADO.NET, Angular, tests, and documentation.
- Explicitly **do not** use Entity Framework, Dapper, or MediatR.
- Start with Step 1 only: folder structure, solution/projects, docs skeleton, Docker Compose for SQL Server, README skeleton, development log — **no** Domain/Application/API/Angular/SignalR implementation yet.
- Define **core green** before SignalR; document GenAI usage.

### Representative output

- Monorepo layout (`backend/`, `frontend/`, `docs/`), `docker-compose.yml` for SQL Server, initial README and docs files, and solution/project wiring.

### Review and validation

- Confirmed solution contains expected projects and references.
- Ran `dotnet build` on the backend solution after skeleton creation (successful build).
- Docker Compose file authoring only; full `docker compose config` validation deferred where Docker CLI was unavailable in that environment.

### Corrections / decisions

- **Scope sequencing:** SignalR, background notification processing, and notification history documented as **after** the agreed core-green checklist (backend build, SQL via Compose, schema/seed, auth, JWT-protected tasks, CRUD scoped by user, tests, README).

---

## Step 2 — Domain, Application, and unit tests

### Prompt / context (summary)

Paraphrased intent communicated to Cursor:

- Proceed with the **next implementation step**: Domain entities/enums, Application DTOs and abstractions (`IAuthService`, `ITaskService`, repository and auth-related interfaces), `AuthService` and `TaskService` behavior, and **focused unit tests** with **Moq**.
- **Do not** implement Infrastructure (ADO.NET), API controllers/endpoints, JWT middleware, Angular, or SignalR in this step.

### Representative summary of AI-generated output

- **Domain:** `User`, `TaskItem`, `Notification`; enums `TaskItemStatus`, `NotificationType`.
- **Application:** `Result` / `Result<T>`; auth and task DTOs; repository contracts (`IUserRepository`, `ITaskRepository`, `INotificationRepository`); `IPasswordHasher`, `ITokenService`; `AuthService`, `TaskService`.
- **Tests:** `AuthServiceTests`, `TaskServiceTests`; Moq package added; placeholder test file removed.

### How the output was reviewed and validated

- **Layering:** Confirmed Application references Domain only; no SQL or ASP.NET types leaked into Domain/Application.
- **Compile:** `dotnet build backend/TaskManager.sln` — zero warnings, zero errors after fixes.
- **Tests:** `dotnet test backend/TaskManager.sln` — **8** unit tests passed; integration test project present but **no integration tests** written yet (expected for this step).
- **Assignment constraints:** Package list reviewed for Step 2 scope — no EF Core, Dapper, or MediatR packages added (see [Forbidden dependencies](#forbidden-dependencies-how-we-check) for ongoing commands).
- **Logic spot-check:** Registration duplicate email, login password failure, task title validation, invalid enum guardrails, and repository interaction verification via Moq.

### Corrections / decisions during Step 2

- **`TaskItemStatus` naming:** Initial naming as `TaskStatus` conflicted with **`System.Threading.Tasks.TaskStatus`** under SDK implicit usings. Renamed the domain enum to **`TaskItemStatus`** with the same member values (Pending, InProgress, Completed, Cancelled). Documented in `docs/guide-design-decisions.md`.
- **Result / error modeling:** Application services return **`Result`** / **`Result<T>`** for expected failures (validation, not-found, duplicate email) so HTTP mapping can stay thin and consistent later; this avoids using exceptions for routine control flow.

---

## Historical planning notes (early milestones)

During Step 1–2, documentation listed **validation ideas** for capabilities that had not been built yet (JWT HTTP wiring, SignalR, deep integration coverage). Those sections were **forward-looking checklists** for humans reviewing AI output—not a statement that the features were missing forever.

## Current implementation — what shipped and how it is validated

The items below are **implemented** in this repository. Validation combines **automated tests** and **developer-led manual smoke** (recorded in `docs/process-development-log.md`).

### Authentication and JWT

- **Shipped:** JWT bearer authentication, BCrypt password hashing, `POST /api/auth/register` and `POST /api/auth/login`, JWT-protected task and notification routes, SignalR **`access_token`** query for browser negotiate.
- **Validate (automated):** Integration tests cover register/login, **401** without token on protected routes, and **user isolation** on tasks (`backend/tests/TaskManager.IntegrationTests/`).
- **Validate (manual, developer):** Swagger authorize flow; browser login/register; peer user cannot access another user’s tasks (spot-check alongside tests).

### SignalR and notifications

- **Shipped:** `NotificationsHub`, in-memory `Channel<T>`, `BackgroundService` dispatcher, SQL notification persistence, `GET /api/notifications`, **`POST /api/notifications/mark-read`**.
- **Validate (automated):** Integration tests cover notification list, async row after task mutation, and **mark-read persistence** + list `isRead`.
- **Validate (manual, developer):** Angular connection, toast/center updates after task CRUD, **no critical console errors**, mark-read survives refresh (see latest development-log entry).

### SQL / ADO.NET

- **Validate (ongoing):** Code review + grep for string-concat SQL; assignment forbids EF Core/Dapper/MediatR — see [Forbidden dependencies](#forbidden-dependencies-how-we-check).

---

## Forbidden dependencies — how we check

The assignment forbids **Entity Framework (Core)**, **Dapper**, and **MediatR** / **Mediator** libraries.

### Package review

Run from the repository root (adjust path if needed):

```powershell
dotnet list .\backend\TaskManager.sln package --include-transitive
```

Repeat for individual projects if desired:

```powershell
dotnet list .\backend\src\TaskManager.Api\TaskManager.Api.csproj package --include-transitive
dotnet list .\backend\src\TaskManager.Infrastructure\TaskManager.Infrastructure.csproj package --include-transitive
```

Forbidden package **id** patterns to watch for include (non-exhaustive): `Microsoft.EntityFrameworkCore`, `Dapper`, `MediatR`, `Mediator.Abstractions`, `Mediator.SourceGenerator`, etc.

### Repository search

Search the codebase for forbidden APIs or package references:

```powershell
# Example: ripgrep-style search from repo root (use your preferred tool)
rg -i "EntityFrameworkCore|Microsoft\.EntityFrameworkCore|UseNpgsql|UseSqlServer|DbContext" backend
rg -i "Dapper|SqlMapper" backend
rg -i "MediatR|IMediator|IRequestHandler" backend
```

Also inspect `*.csproj` files for `<PackageReference>` entries.

### Ongoing discipline

- Prefer explicit dependency review **before each milestone merge** and before submission.
- Optionally add a CI step later that fails if forbidden package IDs appear in `dotnet list package` output.

---

## Consolidated corrections timeline (living)

| When | Correction |
|------|----------------|
| Planning | Core-green checklist before SignalR/notifications. |
| Step 2 | `TaskStatus` → `TaskItemStatus` to avoid BCL name clash. |
| Step 2 | Adopt `Result`/`Result<T>` for application-level failures. |
| Step 3 | ADO.NET repositories, initializer + seed, BCrypt hashing, symmetric JWT issuance + bearer middleware wired at startup. |
| Step 3 | Notifications FK to `TaskItems` uses **`ON DELETE NO ACTION`** to avoid SQL Server multiple cascade paths. |
| Step 4 | REST API (auth + tasks + health), FluentValidation, correlation + exception middleware, CORS; **Swagger UI** at `/swagger` in Development with JWT bearer for Try-it-out. |
| Later | Angular SPA, notifications REST + SignalR, notification mark-read, task list **filter + pagination + sort + search**, **21** integration tests (**31** total with **10** unit tests); reference **troubleshooting** + **security** docs; **developer manual smoke** before submission (`docs/process-development-log.md`). |

Further rows should be appended as the project evolves.
