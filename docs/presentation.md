# Presentation Notes

## User Story

As an authenticated user, I want to manage my tasks, so I can track what needs to be done, update progress, and organize deadlines.

## Demo Flow

1. Register a user.
2. Log in and receive a JWT.
3. Create a task.
4. List tasks for the authenticated user.
5. Update a task.
6. Delete a task.
7. Show that task endpoints are protected.
8. Show real-time notification behavior after the core implementation is green.

## Talking Points

- Clean Architecture and dependency direction.
- Why the project uses ADO.NET instead of EF Core or Dapper.
- How task ownership is enforced by authenticated `UserId`.
- How unit and integration tests validate the highest-risk behavior.
- How GenAI output was reviewed, corrected, and validated.

## Final Smoke Test Checklist

- Frontend login/register.
- Task list/create/edit/delete.
- Notification received.
- No relevant browser console errors.
- Forbidden dependencies check: no EF Core, no Dapper, no MediatR.
