# Design Decisions

## Task Manager Use Case

The task manager use case aligns with the exercise prompt and provides enough behavior to demonstrate authentication, authorization, CRUD operations, validation, user data ownership, persistence, tests, and frontend integration.

## Single Repository

The project uses one repository with separate backend, frontend, and documentation folders so reviewers can run and inspect the submission from one place.

## .NET 8 and ASP.NET Core Web API

.NET 8 is a modern LTS runtime and fits the role expectations. ASP.NET Core Web API with Controllers makes HTTP behavior, routing, authorization, and request/response contracts explicit for review.

## Clean Architecture

Clean Architecture keeps business rules independent from HTTP and SQL Server details. This also makes the no-ORM data layer easier to test and explain.

## SQL Server and ADO.NET

SQL Server is a natural fit for a .NET technical exercise. ADO.NET is used because Entity Framework, Dapper, and Mediator/MediatR are explicitly forbidden.

## Angular

Angular matches the selected frontend direction and demonstrates enterprise-style frontend patterns such as services, routing, guards, interceptors, and forms.

## SignalR After Core Green

SignalR with `BackgroundService` and `Channel<T>` remains in scope, but it starts only after the core backend, authentication, CRUD, tests, SQL Server setup, and minimum README instructions are working.
