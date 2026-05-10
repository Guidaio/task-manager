# Testing and requirements traceability

This document maps **what the exercise asks for** (as stated in-repo) to **how we verify it**, so reviewers can see coverage at a glance.

**Sources of truth**

- [`README.md`](../README.md) — stack, restrictions, core checklist, HTTP surface, smoke checklist.
- [`guide-presentation.md`](guide-presentation.md) — demo outline and submission framing (Ballast Lane–style Senior .NET exercise).
- **Note:** Authoritative PDFs from Ballast Lane live in [`brief/source/`](brief/source/). The original external job posting text is also captured there. Requirements in this doc still prioritize implemented code + [`README.md`](../README.md).

---

## Exercise requirements (consolidated)

| ID | Requirement | Notes |
|----|----------------|-------|
| R1 | .NET backend, Clean Architecture | Layering per `README` / `guide-architecture.md`. |
| R2 | ASP.NET Core Web API, controllers | REST routes in `README`. |
| R3 | SQL Server + **plain ADO.NET** (no EF, Dapper) | Infrastructure repositories. |
| R4 | No MediatR / mediator libraries | Same restriction block as ORMs. |
| R5 | JWT register/login; tasks require auth | Auth + tasks route table. |
| R6 | Task CRUD; **scoped by authenticated user** | See integration isolation test. |
| R7 | Angular frontend | SPA under `frontend/task-manager-web`. |
| R8 | Automated tests (unit + integration) | `README` core green checklist. |
| R9 | Notifications after core green | SignalR + persistence + UI (toasts + notification center). |
| R10 | Documentation / runbook | README + `docs/*`. |

---

## Automated tests (summary)

Run from repo root:

```powershell
dotnet test .\backend\TaskManager.sln
```

If **`dotnet test` fails with MSB3027 / “file is being used by another process”**, stop any running **`TaskManager.Api`** (or other processes locking `bin\Debug` under the API project), then run tests again.

**Latest verification (local):** **25** tests — **9** unit (`TaskManager.UnitTests`), **16** integration (`TaskManager.IntegrationTests`), all passing when SQL is up and file locks are avoided.

### Unit tests (application rules, mocked repositories)

| Test | File | Relates to |
|------|------|------------|
| `Register_ShouldFail_WhenEmailAlreadyExists` | `AuthServiceTests.cs` | R5 registration rules |
| `Login_ShouldFail_WhenPasswordIsInvalid` | `AuthServiceTests.cs` | R5 auth |
| `Login_ShouldReturnToken_WhenCredentialsAreValid` | `AuthServiceTests.cs` | R5 auth |
| `Create_ShouldFail_WhenTitleIsEmpty` | `TaskServiceTests.cs` | R6 validation |
| `Create_ShouldFail_WhenStatusIsInvalid` | `TaskServiceTests.cs` | R6 validation |
| `List_ReturnsPagedDtos_FromRepository` | `TaskServiceTests.cs` | R6 list/paging |
| `Create_ShouldCreateTask_WhenRequestIsValid` | `TaskServiceTests.cs` | R6 create |
| `Update_ShouldFail_WhenTaskDoesNotBelongToUser` | `TaskServiceTests.cs` | R6 ownership |
| `Delete_ShouldFail_WhenTaskDoesNotExist` | `TaskServiceTests.cs` | R6 delete |

### Integration tests (HTTP + real DB pipeline)

| Test | File | Relates to |
|------|------|------------|
| `Get_...` / health check | `HealthApiTests.cs` | API liveness |
| `Login_demo_user_returns_token` | `AuthApiTests.cs` | R5, seed |
| `Register_new_user_then_login_returns_token` | `AuthApiTests.cs` | R5 |
| `Login_invalid_password_is_unauthorized` | `AuthApiTests.cs` | R5 |
| `List_tasks_without_auth_returns_unauthorized` | `TasksApiTests.cs` | R5 |
| `Create_task_without_auth_returns_unauthorized` | `TasksApiTests.cs` | R5 |
| `Demo_user_lists_seeded_tasks` | `TasksApiTests.cs` | R6, seed |
| `Task_crud_round_trip_for_new_user` | `TasksApiTests.cs` | R6 CRUD |
| `Another_user_cannot_read_update_or_delete_peer_task` | `TasksApiTests.cs` | **R6 user isolation** |
| `List_tasks_invalid_status_returns_bad_request` | `TasksApiTests.cs` | R6 list validation |
| `List_tasks_filters_by_status` | `TasksApiTests.cs` | R6 list filter |
| `List_tasks_pagination_returns_total_and_slice` | `TasksApiTests.cs` | R6 paging |
| `List_notifications_without_auth_returns_unauthorized` | `NotificationsApiTests.cs` | R9 + R5 |
| `List_notifications_with_auth_returns_ok` | `NotificationsApiTests.cs` | R9 |
| `Task_create_eventually_persist_notification` | `NotificationsApiTests.cs` | R9 (DB + async worker) |
| `Mark_read_persists_and_list_returns_isRead` | `NotificationsApiTests.cs` | R9 read persistence |

---

## Manual / smoke (not fully automatable in-repo)

Aligned with [README final smoke checklist](../README.md):

- Angular login/register, task CRUD, **notification visible** (toast and/or notification center).
- Browser console free of relevant errors.
- Optional: Swagger `/swagger` try-out for reviewers.

**Gap by design:** there is **no** automated browser (E2E) test in this solution; realtime WebSocket behavior is validated manually and via API/notification persistence tests.

---

## Forbidden dependencies verification

The assignment forbids **Entity Framework / EF Core**, **Dapper**, and **MediatR** (and similar mediators).

**Checks performed (2026-05-10):**

1. **Project file grep** — search all `*.csproj` under `backend/` for `EntityFramework`, `Dapper`, `MediatR`: **no matches**.
2. **Package list filter** — from `backend/`:  
   `dotnet list TaskManager.sln package --include-transitive` filtered for those names: **no matches**.

To repeat:

```powershell
cd .\backend
dotnet list TaskManager.sln package --include-transitive | Select-String -Pattern "EntityFramework|Dapper|MediatR|Microsoft.EntityFrameworkCore"
```

(Optional hardening for CI: fail the job if that Select-String finds output.)

---

## Coverage gaps and intentional scope limits

| Area | Status |
|------|--------|
| SignalR WebSocket delivery to browser | **Manual** smoke; hub not exercised by integration tests (would need a test client or E2E). |
| Angular UI / notification center | **Manual** smoke; no Playwright/Cypress suite. |
| Server-side `IsRead` on notifications | **`POST /api/notifications/mark-read`** updates `IsRead` in SQL; Angular uses `isRead` from **`GET /api/notifications`** and after hover/close actions. |

These are **not** required to satisfy the README checklist; documenting them avoids overstating automated coverage.

---

## Optional improvements (only if you want more than the brief)

Reasonable for a real product, but **not necessary** to call this exercise complete:

- **CI:** `dotnet build`, `dotnet test`, forbidden-package grep on every PR.

Otherwise the stack matches a typical “Senior .NET + Angular + SQL + tests + SignalR notification” submission without over-building.
