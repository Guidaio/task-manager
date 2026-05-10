# Architecture

## Overview

The project follows **Clean Architecture** with explicit separation between HTTP/API concerns, application business logic, domain concepts, and infrastructure (SQL Server access, auth token/password primitives, and **real-time notifications** via SignalR + background dispatch).

This document emphasizes **what exists in code today**. Any **optional future** enhancements (CI, durable queues, etc.) live in presentation/design notes—not as “not yet built” features for items already shipped here.

---

## Current implementation status

| Area | Status | Notes |
|------|--------|--------|
| Solution layout (`Domain`, `Application`, `Infrastructure`, `Api`, tests) | **Implemented** | Api hosts composition root, JWT, initializer, REST controllers, middleware, and CORS. |
| **Domain** (entities, enums) | **Implemented** | `User`, `TaskItem`, `Notification`, `TaskItemStatus`, `NotificationType`. |
| **Application** (services, DTOs, contracts, `Result`) | **Implemented** | `AuthService`, `TaskService`, repository/auth abstractions; unit tests cover core rules with mocks. |
| **Infrastructure** (ADO.NET, repositories, DB initializer, seed, hashing, JWT issuance) | **Implemented** | `SqlConnectionFactory`, `UserRepository`, `TaskRepository`, `NotificationRepository`, `DatabaseInitializer` (schema + demo seed), `PasswordHasher` (BCrypt), `JwtTokenService` (HS256). Registered via `AddInfrastructure`. |
| **API** (controllers, FluentValidation, middleware) | **Implemented** | `AuthController`, `TasksController`, `NotificationsController`, `HealthController`; correlation + exception middleware; CORS policy `AngularDev` for Angular dev server; JWT on task and notification routes. |
| **JWT authentication** | **Implemented** | Bearer validation + token issuance; HTTP login/register expose JWT to clients. |
| **Angular frontend** | **Implemented** | SPA in `frontend/task-manager-web`: auth, task CRUD, SignalR client, toasts, notification center (drawer). |
| **`SignalR` + `BackgroundService` + `Channel<T>`** | **Implemented** | Notifications hub, JWT via `access_token` query on negotiate; worker dispatches after task mutations; SQL persistence. |
| Unit tests (Application) | **Implemented** | Eight tests targeting `AuthService` / `TaskService`. |
| Integration tests (`WebApplicationFactory`) | **Implemented** | **Thirteen** tests: health, auth, tasks (including user isolation), notifications list + async persistence after task create + **mark-read persistence** (`Mark_read_persists_and_list_returns_isRead`). |

---

## Layers (target shape)

- **`TaskManager.Domain`:** Entities, enums, and simple domain rules.
- **`TaskManager.Application`:** Use cases, service implementations against interfaces, DTOs, repository/token/password contracts, validation-friendly results (`Result`).
- **`TaskManager.Infrastructure`:** ADO.NET (`Microsoft.Data.SqlClient`), connection factory, repository implementations, password hashing and JWT token issuance **implementations**, database initializer and seed data.
- **`TaskManager.Api`:** Controllers, middleware (exception handling, correlation ID), authentication/authorization configuration, **SignalR hub mapping** (`/hubs/notifications`), composition root (DI registration).

---

## Dependency direction

```mermaid
flowchart LR
    Api[TaskManager.Api] --> Application[TaskManager.Application]
    Api --> Infrastructure[TaskManager.Infrastructure]
    Infrastructure --> Application
    Infrastructure --> Domain[TaskManager.Domain]
    Application --> Domain
```

---

## Data access (implemented flow)

**Status: implemented** — repositories in Infrastructure use parameterized ADO.NET against SQL Server (see `TaskManager.Infrastructure.Persistence`).

```mermaid
sequenceDiagram
    participant Controller as Api_Controller
    participant AppSvc as Application_Service
    participant Repo as Infrastructure_SqlRepository
    participant Db as SQL_Server

    Controller->>AppSvc: Command/query with scoped user context
    AppSvc->>Repo: Repository method (parameterized SQL)
    Repo->>Db: SqlCommand with parameters
    Db-->>Repo: Rows / execution result
    Repo-->>AppSvc: Domain/DTO mapping
    AppSvc-->>Controller: Result or payload
```

**Rules:**

- Use **parameterized** ADO.NET exclusively in Infrastructure (no dynamic SQL built from raw user strings).
- Task reads/writes/deletes must filter by **`UserId`** derived from authenticated identity for protected operations.

---

## Authentication flow

**Status: implemented** — register/login HTTP endpoints issue JWTs; task routes require `Authorization: Bearer`.

```mermaid
sequenceDiagram
    participant Client as Client
    participant Api as Api
    participant Auth as AuthController
    participant App as AuthService
    participant Repo as UserRepository
    participant Jwt as TokenService

    Client->>Auth: POST register/login
    Auth->>App: RegisterAsync / LoginAsync
    App->>Repo: Persist / load user (ADO.NET)
    App->>Jwt: CreateToken(user claims)
    Jwt-->>App: JWT string
    App-->>Auth: Result with token
    Auth-->>Client: 200 + token / error body

    Client->>Api: Task API request + Authorization Bearer
    Api->>Api: Validate JWT + resolve UserId claim
    Api->>App: Task operations scoped to UserId
```

**Validated behavior (integration tests + manual API checks):**

- Unauthorized requests to protected routes return **401**.
- Token carries a stable **user identifier** used everywhere tasks and notifications are scoped.

---

## Notification flow (implemented)

**Status: implemented** — task create/update/delete enqueue notification work; `BackgroundService` drains a channel and pushes to SignalR; Angular shows toasts and a notification drawer backed by `GET /api/notifications`.

```mermaid
flowchart LR
    subgraph api [Api]
        Tasks[TasksController]
        Hub[NotificationHub]
    end
    subgraph app [Application]
        TaskSvc[TaskService]
        Queue[INotificationQueue_Channel]
    end
    subgraph infra [Infrastructure]
        Worker[BackgroundService_Dispatcher]
    end
    subgraph client [Angular]
        UI[Toast_or_feed]
    end

    Tasks --> TaskSvc
    TaskSvc --> Queue
    Queue --> Worker
    Worker --> Hub
    Hub --> UI
```

**Intent:**

1. Task mutation completes and persists.
2. A notification message is enqueued (in-memory `Channel<T>`).
3. A **BackgroundService** consumes the queue and pushes to the correct SignalR connection/group for the user.
Angular displays **toasts** and a **notification center** (recent items from the API + realtime merge).

---

## Data access constraint (assignment)

The exercise forbids Entity Framework, Dapper, and Mediator/MediatR. Persistence uses **plain ADO.NET** with parameterized SQL in **`TaskManager.Infrastructure`**.

---

## Core green definition

The core is considered green when:

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

SignalR, background processing, and notification history were introduced **after** the core green checklist; that ordering is preserved in the development log.
