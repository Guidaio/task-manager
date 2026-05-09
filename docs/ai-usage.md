# AI Usage

This document records how GenAI tools are used during the project and how their output is validated.

## Tools Used

- Cursor
- ChatGPT, if used during implementation or review

## Relevant Prompts

Initial planning prompt:

> Build a full stack Task Manager for a Senior .NET technical exercise using Clean Architecture, ASP.NET Core Web API, SQL Server, plain ADO.NET, Angular, tests, documentation, and conscious GenAI usage. Do not use Entity Framework, Dapper, or MediatR.

## Representative Output

The initial output was a project plan covering architecture, implementation order, test strategy, documentation, and scope sequencing.

## Validation Approach

Validation will include:

- Building the backend solution.
- Running unit and integration tests.
- Verifying SQL access uses parameterized ADO.NET commands.
- Checking package references for forbidden dependencies.
- Testing JWT authentication and protected task endpoints.
- Testing user isolation for task operations.
- Running frontend smoke tests in the browser.

## Corrections and Improvements

The first scope correction was to define a core green checklist and defer SignalR, background processing, and notification history until the core application is stable.
