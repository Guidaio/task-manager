# Design Decisions

## Task Manager Use Case

The task manager use case aligns with the exercise prompt and provides enough behavior to demonstrate authentication, authorization, CRUD operations, validation, user data ownership, persistence, tests, and frontend integration.

## Single Repository

The project uses one repository with separate backend, frontend, and documentation folders so reviewers can run and inspect the submission from one place.

## .NET 8 and ASP.NET Core Web API

.NET 8 is a modern LTS runtime and fits the role expectations. ASP.NET Core Web API makes HTTP behavior, routing, authorization, and request/response contracts explicit for review.

## Controllers

We use **Controllers** rather than Minimal APIs as the primary HTTP surface because:

- Endpoint grouping (`AuthController`, `TasksController`, `HealthController`) maps cleanly to the assignment’s CRUD and auth stories.
- `[Authorize]` and conventional verbs/readability match **enterprise-style APIs** often reviewed in Senior interviews.
- The distinction between HTTP models and Application DTOs stays explicit.

(Minimal endpoints remain usable internally where helpful, but the visible API is controller-centric.)

## Clean Architecture

Clean Architecture keeps business rules independent from HTTP and SQL Server details. This also makes the no-ORM data layer easier to test and explain.

## SQL Server and ADO.NET

SQL Server is a natural fit for a .NET technical exercise. **ADO.NET** is required because Entity Framework, Dapper, and Mediator/MediatR are **explicitly forbidden** by the assignment. Plain SQL with **`Microsoft.Data.SqlClient`** keeps persistence transparent and review-friendly.

**Referential integrity:** `Notifications.TaskId` references `TaskItems` with **`ON DELETE NO ACTION`** (not `SET NULL`) so SQL Server does not reject the schema due to **multiple cascade paths** (`Users` → `TaskItems` CASCADE plus `Users` → `Notifications` CASCADE).

## Angular

Angular matches the selected frontend direction and demonstrates enterprise-style frontend patterns such as services, routing, guards, interceptors, and forms.

## JWT authentication (**planned wiring**)

**JWT Bearer** is chosen because:

- The assignment requires **user creation**, **login**, **authorized** vs **non-authorized** endpoints; JWT fits stateless Web APIs.
- Angular can attach tokens via an **HTTP interceptor** without server session state.
- Claims carry a stable **user identifier** used to **scope all task operations**.

Concrete ASP.NET authentication middleware, signing keys, and token lifetime policies are implemented in Infrastructure/API **after** the Domain/Application contracts exist.

## FluentValidation (**planned on API layer**)

**FluentValidation** validates **HTTP request models** (register/login/task payloads) so rules stay declarative and testable **without** leaking framework attributes into Application services.

**Division of responsibility:**

- **FluentValidation:** shape of input (required fields, lengths, formats).
- **Application services:** business outcomes (duplicate email, invalid credentials, ownership, not-found).

## Result / error modeling (Application layer)

Application services return **`Result`** / **`Result<T>`** for expected failures (validation, duplicates, not-found) instead of throwing for routine control flow.

**Why:**

- Keeps **business outcomes explicit** in unit tests (`Succeeded`, `Error`).
- Lets API translate failures to consistent HTTP status codes and bodies later (`400`, `401`, `404`, etc.).
- Avoids mixing validation exceptions with unexpected fault exceptions.

## xUnit, Moq, and WebApplicationFactory

- **xUnit:** Standard, CI-friendly test runner for .NET with good tooling defaults.
- **Moq:** Isolates Application services from SQL/JWT while proving **business rules** quickly (Step 2 already uses Moq for repositories and collaborators).
- **`WebApplicationFactory`:** (**planned**) Spins up the API **in-process** with a test configuration (including real SQL or test doubles) to validate middleware, JWT, routing, and ADO.NET repositories together — the highest-value checks after unit coverage.

## Docker Compose

Docker Compose runs **SQL Server** locally with predictable ports and credentials so reviewers can reproduce the database dependency **without** installing a full instance manually. Optional future containers for API/UI may be added; the **minimum** valuable target is SQL Server for integration tests and demos.

## SignalR + BackgroundService + Channel\<T\>

Real-time feedback improves UX after task mutations. **SignalR** pushes messages to the Angular client; an in-memory **`Channel<T>`** plus **`BackgroundService`** decouple HTTP request completion from notification delivery.

**Why not a cloud broker in this exercise:** Azure Service Bus (etc.) adds secrets and operational overhead; the assignment allows showcasing asynchronous notification flow locally.

**Sequencing:** Implemented **only after core green** (working CRUD + JWT + tests + README baseline) so the core demo stays stable first — aligned with the project **SignalR after core green** rule.

## Task status enum naming

The domain uses `TaskItemStatus` rather than `TaskStatus` because `System.Threading.Tasks.TaskStatus` exists in the default implicit usings for modern SDK-style projects, which causes ambiguous type errors on entities like `TaskItem`.
