# Security considerations (exercise scope)

This document summarizes **how the submission handles common security topics** in **development** and what would change in **production**. It is aligned with the technical exercise (auth, isolation, SQL, no forbidden ORMs).

**Not a penetration test or formal threat model.** Use it in demos and reviews to show awareness without overstating assurance.

---

## Password storage

- Passwords are hashed with **BCrypt** before persistence (`TaskManager.Infrastructure` password hasher implementation).
- Plain passwords are **not** stored in SQL.
- Demo credentials in README / seed are **for local review only**; treat them as public if the repo is public.

---

## Authentication and JWT

- **JWT Bearer** protects task and notification routes; register/login issue signed tokens.
- **Development** signing key and issuer/audience live in **`appsettings.Development.json`** — suitable for local runs only.
- **Production** would use **short-lived access tokens**, **strong signing keys** (or asymmetric keys), **secret storage** (Key Vault / environment), **HTTPS-only**, and likely **refresh-token** or server-side session patterns depending on product requirements.
- **SignalR:** browsers send the JWT as **`access_token`** on the negotiate URL because the WebSocket handshake cannot set `Authorization` headers in the same way as `HttpClient`; the API is configured to accept that for the hub path only.

---

## Authorization and user isolation

- Task and notification operations are scoped to the authenticated user’s id (claims → `UserId`).
- Integration tests assert **cross-user access** is denied for another user’s tasks.
- **Application services** and **repositories** enforce ownership on mutations and reads for protected resources (see task repository queries).

---

## SQL injection

- Data access uses **ADO.NET** with **parameterized** commands (`SqlParameter`); user input is not concatenated into SQL strings for queries reviewed in this codebase.
- Assignment forbids **EF Core**, **Dapper**, and **MediatR**; custom SQL remains explicit and reviewable.

---

## Configuration and secrets

- **`appsettings.Development.json`** contains JWT signing key and connection strings for **local development**.
- **Do not** commit real production secrets, API keys, or production connection strings.
- If you fork this repo publicly, assume dev keys in history are **compromised** for any real deployment; rotate everything for production.

---

## Transport and CORS

- Local dev typically uses **HTTP** on localhost; production APIs should use **TLS** end-to-end.
- CORS is configured for the **Angular dev server** origin; production would lock origins to known frontends.

---

## Further reading in-repo

- **[reference-credentials.md](reference-credentials.md)** — URLs, SQL, JWT dev settings.
- **[reference-troubleshooting.md](reference-troubleshooting.md)** — setup and test issues.
- **[guide-architecture.md](guide-architecture.md)** — layers and auth flow.
- **[guide-design-decisions.md](guide-design-decisions.md)** — JWT, validation, ADO.NET rationale.
