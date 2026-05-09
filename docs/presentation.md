# Presentation Notes

Use this document as a **short speaker outline** for the technical demo and **code review**. Pair it with `docs/architecture.md`, `docs/design-decisions.md`, and `docs/ai-usage.md` for depth.

---

## Project overview

- **Purpose:** Full stack **Task Manager** for the Ballast Lane–style Senior .NET exercise: authenticated users manage **their own** tasks (CRUD), with clean layering, custom persistence (no EF/Dapper/MediatR), tests, and documented GenAI usage.
- **Backend:** .NET 8, ASP.NET Core Web API with **Controllers**, Clean Architecture, SQL Server, **ADO.NET**, **JWT** register/login + protected task routes, FluentValidation on requests.
- **Frontend:** Angular SPA (**planned**): login/register, task CRUD, responsive UI, JWT interceptor, SignalR client for notifications (**after core green**).
- **Docs:** README runbook, architecture/design/AI logs, development log per milestone.

---

## User story

As an **authenticated user**, I want to **manage my tasks**, so I can track what needs to be done, update progress, and organize deadlines.

---

## Architecture summary

- **Domain:** Entities and enums (`User`, `TaskItem`, `Notification`, `TaskItemStatus`, …).
- **Application:** Use cases (`AuthService`, `TaskService`), DTOs, repository/token/password **interfaces**, **`Result`/`Result<T>`** for expected failures.
- **Infrastructure:** ADO.NET repositories, DB initializer + demo seed, BCrypt password hashing, symmetric JWT token creation (`JwtTokenService`).
- **Api:** REST controllers, FluentValidation, JWT bearer, correlation + exception middleware; SignalR still planned after core green.

**Dependency rule:** inner layers do not depend on outer layers; Infrastructure implements interfaces defined in Application.

See **`docs/architecture.md`** for **current vs planned** status and sequence diagrams.

---

## Main flows (demo script)

1. Register a user.
2. Log in and receive a JWT via **`POST /api/auth/login`** (or register first).
3. Create a task.
4. List tasks — **only** current user’s tasks.
5. Update and delete a task.
6. Call a protected endpoint **without** token → **401**.
7. (**After core green**) Trigger task action → **real-time notification** in Angular.

---

## Testing approach

| Layer | Goal |
|--------|------|
| **Unit tests** | Fast feedback on **application rules**: duplicate email, invalid credentials, empty title, invalid enum payload, not-found / wrong-user paths via mocked repositories (`Moq`). |
| **Integration tests** (**planned**) | **`WebApplicationFactory`**: real HTTP pipeline + database (test container or local SQL), JWT issuance, **401** without token, happy-path CRUD, **user isolation** between two users. |
| **Manual / smoke** | README checklist + browser: login/register, CRUD, notifications, **no relevant console errors**, forbidden-deps verification. |

**Today:** eight unit tests on Application services; **no** integration tests committed yet — say so honestly if asked.

---

## GenAI usage summary

- **Tool:** Cursor for planning, docs, and Step 2 Domain/Application/tests generation.
- **Process:** Prompt → generated structure/code → **human-led review** (layering, naming clashes, `Result` modeling, assignment bans).
- **Evidence:** Full audit trail in **`docs/ai-usage.md`** (prompts paraphrased, representative outputs, validation commands, corrections).

**Talking point:** GenAI accelerated scaffolding; **engineering judgment** enforced constraints (no EF/Dapper/MediatR, parameterized SQL plan, core-before-SignalR).

---

## Trade-offs

| Choice | Benefit | Cost |
|--------|---------|------|
| ADO.NET vs ORM | Meets exercise constraint; explicit SQL | More boilerplate; manual mapping |
| `Result` vs exceptions for validation | Predictable API mapping; easy unit testing | Slightly more ceremony than throwing |
| SignalR after core green | Stable demo path; interview-safe sequencing | Real-time features arrive later |
| Angular vs lighter SPA | Aligns with enterprise full-stack profile | More setup than minimal vanilla |

---

## Future improvements

- CI pipeline: build, test, **forbidden package** guard (`dotnet list package` / grep).
- OpenAPI/Swagger for reviewer ergonomics.
- Replace in-memory notification channel with **durable queue** (Azure Service Bus, RabbitMQ) for production parity.
- Refresh tokens or shorter-lived access tokens + rotation policy.
- Structured logging + **distributed tracing** (beyond inbound **`X-Correlation-ID`** and JSON exception payloads today).

---

## Talking points (code review)

- Clean Architecture boundaries and **why** Infrastructure references Application.
- **Task ownership:** every task operation filtered by authenticated **`UserId`** at persistence boundary.
- **Security:** parameterized SQL only; JWT bearer configured at startup; password hashing via Infrastructure (BCrypt).
- **Testing pyramid:** unit coverage for rules now; integration tests next for pipeline + DB + auth.

---

## Final smoke test checklist

- Frontend login/register.
- Task list/create/edit/delete.
- Notification received (**after SignalR phase**).
- No relevant browser console errors.
- Forbidden dependencies check: no EF Core, no Dapper, no MediatR.
