# Presentation Notes

Use this document as a **short speaker outline** for the technical demo and **code review**. Pair it with `docs/guide-architecture.md`, `docs/guide-design-decisions.md`, `docs/process-ai-usage.md`, and `docs/reference-testing-requirements.md` for depth.

---

## Project overview

- **Purpose:** Full stack **Task Manager** for the Ballast Lane–style Senior .NET exercise: authenticated users manage **their own** tasks (CRUD), with clean layering, custom persistence (no EF/Dapper/MediatR), tests, and documented GenAI usage.
- **Backend:** .NET 8, ASP.NET Core Web API with **Controllers**, Clean Architecture, SQL Server, **ADO.NET**, **JWT** register/login + protected task routes, FluentValidation on requests.
- **Frontend:** Angular SPA: login/register, task CRUD, responsive UI, JWT interceptor, SignalR client (**toasts + notification center drawer** after task events).
- **Docs:** README runbook, architecture/design/AI logs, development log per milestone, **testing/requirements traceability** (`docs/reference-testing-requirements.md`).

---

## User story

As an **authenticated user**, I want to **manage my tasks**, so I can track what needs to be done, update progress, and organize deadlines.

---

## Architecture summary

- **Domain:** Entities and enums (`User`, `TaskItem`, `Notification`, `TaskItemStatus`, …).
- **Application:** Use cases (`AuthService`, `TaskService`), DTOs, repository/token/password **interfaces**, **`Result`/`Result<T>`** for expected failures.
- **Infrastructure:** ADO.NET repositories, DB initializer + demo seed, BCrypt password hashing, symmetric JWT token creation (`JwtTokenService`).
- **Api:** REST controllers, FluentValidation, JWT bearer, correlation + exception middleware; **SignalR** notifications hub + camelCase JSON payloads for browser clients.

**Dependency rule:** inner layers do not depend on outer layers; Infrastructure implements interfaces defined in Application.

See **`docs/guide-architecture.md`** for implementation status and sequence diagrams.

---

## Main flows (demo script)

1. Register a user.
2. Log in and receive a JWT via **`POST /api/auth/login`** (or register first).
3. Create a task.
4. List tasks — **only** current user’s tasks.
5. Update and delete a task.
6. Call a protected endpoint **without** token → **401**.
7. Trigger task create/update/delete → **real-time notification** (toast + optional bell/drawer history).

---

## Testing approach

| Layer | Goal |
|--------|------|
| **Unit tests** | Fast feedback on **application rules**: duplicate email, invalid credentials, empty title, invalid enum payload, not-found / wrong-user paths via mocked repositories (`Moq`). |
| **Integration tests** | **`WebApplicationFactory`**: real HTTP pipeline + database, JWT, **401** without token, task CRUD, **user isolation** between two users, notifications list + persisted row after task create, **notification mark-read persistence**. |
| **Manual / smoke** | README checklist + browser: login/register, CRUD, notifications, **no relevant console errors**, forbidden-deps verification (see `docs/reference-testing-requirements.md`). |

**Today:** **8** unit tests + **13** integration tests (**21** automated tests total). **SignalR delivery** to the browser is not covered by WebSocket-level integration tests; **the developer completed final manual smoke testing** (realtime toasts/center, mark-read after refresh, clean console) before submission—see `docs/process-development-log.md` (latest entry).

---

## GenAI usage summary

- **Tool:** Cursor for planning, docs, and Step 2 Domain/Application/tests generation.
- **Process:** Prompt → generated structure/code → **human-led review** (layering, naming clashes, `Result` modeling, assignment bans).
- **Evidence:** Full audit trail in **`docs/process-ai-usage.md`** (prompts paraphrased, representative outputs, validation commands, corrections).

**Talking point:** GenAI accelerated scaffolding; **engineering judgment** enforced constraints (no EF/Dapper/MediatR, parameterized SQL plan, core-before-SignalR).

---

## Trade-offs

| Choice | Benefit | Cost |
|--------|---------|------|
| ADO.NET vs ORM | Meets exercise constraint; explicit SQL | More boilerplate; manual mapping |
| `Result` vs exceptions for validation | Predictable API mapping; easy unit testing | Slightly more ceremony than throwing |
| SignalR after core green | Stable demo path; interview-safe sequencing | Real-time features ship after baseline CRUD — now **done** for this repo |
| Angular vs lighter SPA | Aligns with enterprise full-stack profile | More setup than minimal vanilla |

---

## Future improvements

- CI pipeline: build, test, **forbidden package** guard (`dotnet list package` / grep).
- OpenAPI/Swagger is already available at **`/swagger`** in Development; further **production** work could add versioning, auth hardening, or published artifacts.
- Replace in-memory notification channel with **durable queue** (Azure Service Bus, RabbitMQ) for production parity.
- Refresh tokens or shorter-lived access tokens + rotation policy.
- Structured logging + **distributed tracing** (beyond inbound **`X-Correlation-ID`** and JSON exception payloads today).

---

## Talking points (code review)

- Clean Architecture boundaries and **why** Infrastructure references Application.
- **Task ownership:** every task operation filtered by authenticated **`UserId`** at persistence boundary.
- **Security:** parameterized SQL only; JWT bearer configured at startup; password hashing via Infrastructure (BCrypt).
- **Testing pyramid:** unit coverage for rules; integration tests for pipeline + DB + auth + notification persistence; manual check for WebSocket UX.

---

## Final smoke test checklist

**Developer-completed (pre-submission):** The repository author ran final **manual** smoke testing locally (Docker SQL, API, Swagger, auth, task CRUD, user isolation, Angular flows, SignalR notifications, mark-read persistence, `dotnet test`, console check) and recorded it in `docs/process-development-log.md`. Reviewers may still repeat this list on their machine.

- Frontend login/register.
- Task list/create/edit/delete.
- Notification received (toast and/or **notification center**); mark-read survives refresh.
- No relevant browser console errors.
- Forbidden dependencies check: no EF Core, no Dapper, no MediatR.
- Automated: `dotnet test .\backend\TaskManager.sln` (stop the API first if builds report file-lock / MSB3027 errors).
