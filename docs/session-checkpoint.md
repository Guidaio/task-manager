# Session checkpoint — resume after PC restart

**Purpose:** Pick up exactly where development stopped. **Step 3 (Infrastructure)** is implemented in your working tree but **not committed yet** until you verify it locally.

---

## Git status (as of this checkpoint)

- **Branch:** `master` (tracking `origin/master`).
- **Working tree:** **dirty** — Step 3 changes are **modified + new files**, **not committed**.
- **Last commits on remote:** Domain/Application (`feat/domain-application`), docs alignment (`docs: align …`).  
  **`feat`/Infrastructure Step 3 is still local-only until you commit and push.**

After verification, commit everything listed below under “Files involved”.

---

## What was finished locally (Step 3)

- ADO.NET repositories (`User`, `Task`, `Notification`), parameterized SQL only.
- `DatabaseInitializer`: schema + optional DB creation via master connection + **demo seed** user/tasks.
- `PasswordHasher` (BCrypt), `JwtTokenService` (HS256), `AddInfrastructure` DI.
- API startup: JWT bearer wired, initializer runs on startup, minimal `GET /` probe (**controllers still Step 4**).

See **`docs/development-log.md`** section **“Step 3: Infrastructure…”** for the full breakdown.

---

## Before Step 4 — validate locally (your checklist after restart)

1. **Docker Desktop** running (Linux containers / WSL 2).
2. From repo root:

   ```powershell
   docker compose up -d sqlserver
   ```

   Wait until SQL Server is healthy (first run can take ~30–60 seconds).

3. Run API (**Development** uses `appsettings.Development.json` — SA password matches `docker-compose.yml`):

   ```powershell
   dotnet run --project .\backend\src\TaskManager.Api\TaskManager.Api.csproj
   ```

4. **Smoke checks:**
   - Console shows startup **without** unhandled exceptions (initializer connects and applies schema/seed).
   - Browse or curl `GET /` — JSON mentioning Task Manager API.
   - Optional: connect with SSMS/Azure Data Studio to `localhost,1433` / DB **TaskManager** and confirm tables **`Users`**, **`TaskItems`**, **`Notifications`** and seeded demo row.

5. **Demo login credentials** (after seed): see **`README.md`** → Demo Credentials (`demo@taskmanager.local` / `Demo_user_12345`).  
   **Note:** HTTP login/register endpoints come in **Step 4**; for Step 3 you only prove DB + app startup.

6. **Regression:**

   ```powershell
   dotnet build .\backend\TaskManager.sln
   dotnet test .\backend\TaskManager.sln
   ```

---

## After verification — commit Step 3 (you or Cursor)

Suggested message:

```text
feat(infrastructure): ADO.NET repos, DB initializer, seed, JWT wiring
```

Then push to `origin/master` when ready:

```powershell
git add -A
git status
git commit -m "feat(infrastructure): ADO.NET repos, DB initializer, seed, JWT wiring"
git push origin master
```

*(Use Git Bash if `git` is not on PATH in PowerShell.)*

---

## Next milestone — Step 4 (`api-crud`)

Implement **HTTP surface**: `AuthController`, `TasksController`, `HealthController`, FluentValidation, exception + correlation middleware, map `Result` → HTTP status codes, `[Authorize]` on tasks.

Integration tests and Angular remain later todos.

---

## Files involved in uncommitted Step 3 (reference)

- `backend/src/TaskManager.Infrastructure/**` (new/changed)
- `backend/src/TaskManager.Api/Program.cs`, `*.csproj`, `appsettings*.json`
- `README.md`
- `docs/architecture.md`, `docs/development-log.md`, `docs/ai-usage.md`, `docs/presentation.md`

You can **delete this file** after Step 3 is committed and pushed if you no longer need the checkpoint.
