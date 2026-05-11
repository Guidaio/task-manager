# Documentation index

Documentation under **`docs/`** supports the Ballast Lane–style technical exercise: **architecture and decisions**, **GenAI disclosure**, **implementation log**, and **references** for local setup and validation.

The repository **[README.md](../README.md)** is the main **runbook** (build, run, smoke checklist).

## Exercise prompts

| Path | Description |
|------|-------------|
| [brief/ballast-lane-task-manager.en.txt](brief/ballast-lane-task-manager.en.txt) | Exercise specification (English). |
| [brief/ballast-lane-task-manager.pt-BR.txt](brief/ballast-lane-task-manager.pt-BR.txt) | Same (Portuguese). |
| [brief/source/](brief/source/) | Original PDF/DOCX materials supplied with the exercise. |

## Guides (story, architecture, rationale, demo)

| File | Description |
|------|-------------|
| [guide-architecture.md](guide-architecture.md) | Clean Architecture, layers, data and auth flows, implementation status. |
| [guide-design-decisions.md](guide-design-decisions.md) | Rationale: ADO.NET, JWT, FluentValidation, `Result`, tests, SignalR sequencing, etc. |
| [guide-presentation.md](guide-presentation.md) | Demo outline and interview talking points. |
| [final-project-review.md](final-project-review.md) | Pre-submission alignment checklist (requirements, tests, docs, risks). |

## Process (planning and execution)

| File | Description |
|------|-------------|
| [process-ai-usage.md](process-ai-usage.md) | GenAI tools, prompts (paraphrased), representative outputs, validation, corrections (exercise requirement). |
| [process-development-log.md](process-development-log.md) | Chronological milestone log and what was validated. |

## Reference (credentials, tests, operations)

| File | Description |
|------|-------------|
| [reference-credentials.md](reference-credentials.md) | Local URLs, demo user, SQL/JWT notes for Development. |
| [reference-testing-requirements.md](reference-testing-requirements.md) | Requirements ↔ tests traceability, test inventory, forbidden-deps checks. |
| [reference-troubleshooting.md](reference-troubleshooting.md) | Docker/SQL, DB reset, MSB3027 locks, integration tests, optional API Docker image. |
| [reference-security.md](reference-security.md) | Security topics at exercise scope (JWT, isolation, parameterized SQL, secrets). |
