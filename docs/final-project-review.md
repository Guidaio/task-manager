# Final project review (pre-submission)

This document summarizes **alignment between the Ballast Lane–style exercise, the shipped code, and the documentation** after the **2026-05-12 documentation alignment** pass. It is not a substitute for the full runbook ([`README.md`](../README.md)) or traceability inventory ([`reference-testing-requirements.md`](reference-testing-requirements.md)).

---

## Assignment coverage (high level)

| Theme | Status | Notes |
|--------|--------|--------|
| .NET 8 Web API, Clean Architecture | Met | `backend/src/TaskManager.*` layering; Application references Domain only. |
| ADO.NET (no EF / Dapper / MediatR) | Met | Repositories use `Microsoft.Data.SqlClient`; CI + docs describe forbidden-deps checks. |
| Auth: register, login, JWT, public vs protected routes | Met | `AuthController`, `[Authorize]` on tasks/notifications, `HealthController` / auth register-login open. |
| Task CRUD scoped by user | Met | Integration isolation + `TaskService` / repositories. |
| Angular frontend | Met | `frontend/task-manager-web`. |
| Notifications + realtime | Met | SignalR hub, persisted rows, mark-read, **clear all** (`DELETE /api/notifications`), **30-day retention** on `GET` list load. |
| Unit + integration tests | Met | **10** unit + **23** integration = **33** total (`dotnet test backend/TaskManager.sln`). |
| README, demo seed, GenAI docs | Met | Root README + [`process-ai-usage.md`](process-ai-usage.md) + [`guide-presentation.md`](guide-presentation.md). |

---

## Automated test totals (source of truth)

| Project | Count |
|---------|--------|
| `TaskManager.UnitTests` | **10** |
| `TaskManager.IntegrationTests` | **23** |
| **Total** | **33** |

Inventory and requirement mapping: [`reference-testing-requirements.md`](reference-testing-requirements.md).

---

## Notification HTTP surface (documented vs code)

| Method | Route | Behavior (summary) |
|--------|--------|---------------------|
| GET | `/api/notifications` | List for current user; **deletes** rows older than **30 days** (UTC) for that user before returning. |
| POST | `/api/notifications/mark-read` | Body `ids[]`; marks notifications read (max 50 ids). |
| DELETE | `/api/notifications` | Deletes **all** notifications for the current user; **204 No Content**. |

Realtime: **`/hubs/notifications`** with JWT via **`access_token`** query on negotiate. Implementation: `NotificationsController`, `NotificationRepository`, `NotificationDispatchWorker`, Angular notification center + toasts.

---

## Documentation status

- **README:** Route table includes all three notification endpoints; SignalR section describes persistence, mark-read, Clear all, retention.
- **Architecture:** [`guide-architecture.md`](guide-architecture.md) uses **`INotificationPublisher`**, **`Channel<T>`**, and **`NotificationDispatchWorker`** (no obsolete `INotificationQueue_Channel` label).
- **GenAI:** [`process-ai-usage.md`](process-ai-usage.md) reflects shipped notification API, **33** tests, CI in [`.github/workflows/ci.yml`](../.github/workflows/ci.yml), and separates **historical** Step 2 notes (e.g. **8** unit tests at that milestone) from **current** behavior.
- **Historical log entries** in [`process-development-log.md`](process-development-log.md) still record past test counts (e.g. Step 2: **8** unit only); they are **chronological**, not current totals.

---

## Forbidden dependencies

Per [`reference-testing-requirements.md`](reference-testing-requirements.md) and CI: no Entity Framework Core, Dapper, or MediatR in package graph (verify with `dotnet list ... --include-transitive` and `.csproj` grep).

---

## Readiness

**Ready to submit** from a **documentation vs implementation** perspective, provided:

- Local **`dotnet test`** passes with SQL available and **no** `TaskManager.Api` file locks (MSB3027).
- Presenter uses **33** / **23** / **10** when discussing test counts.

For demo talking points, use [`guide-presentation.md`](guide-presentation.md).
