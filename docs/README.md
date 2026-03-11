## CleanTemplate documentation index

This folder contains documentation for each layer of the template and how they work together.

- **[`API`](API.md)**: presentation layer (ASP.NET Core Web API, middlewares, controllers/endpoints).
- **[`Application`](Application.md)**: use cases, commands/queries and handlers, DTOs, mappings, validators.
- **[`Domain`](Domain.md)**: domain entities, repositories contracts, unit of work, domain exceptions.
- **[`Infrastructure`](Infrastructure.md)**: EF Core DbContext, repository implementations, unit of work, external services.

---

## End-to-end request flow

High-level flow for a typical write operation (e.g. "Create Project"):

1. **API layer**
   - A client sends an HTTP request to an endpoint (e.g. `POST /api/projects`).
   - The controller (or minimal API endpoint) receives the request and maps it into an **Application command** (e.g. `CreateProjectCommand`).
   - The endpoint sends the command through **MediatR** (`IMediator.Send`).

2. **Application layer**
   - MediatR locates the corresponding **command handler** (e.g. `CreateProjectCommandHandler`).
   - The handler:
     - Runs **FluentValidation** rules for the command.
     - Uses **Domain** repositories and entities to perform the business operation.
     - Optionally starts/commits a **unit of work** for transactional consistency.
   - The handler returns a result (e.g. the created entity Id or a DTO).

3. **Domain layer**
   - Domain entities (e.g. `Project`) enforce invariants and business rules (often throwing `DomainException` or derived types when a rule is violated).
   - Domain contracts (`IProjectRepository`, `IUnitOfWork`, etc.) describe the persistence operations required, but are not tied to any specific database or technology.

4. **Infrastructure layer**
   - The concrete implementations of repositories (e.g. `ProjectRepository`) and `UnitOfWork` use **EF Core** and `AppDbContext` to communicate with the database.
   - They persist or retrieve domain entities according to the contracts defined in the Domain layer.

5. **Back to API**
   - The Application handler returns control to the API endpoint.
   - The controller/endpoint converts the result into an appropriate HTTP status + body (e.g. `201 Created` with a location header, `200 OK` with a DTO, `400/404` on validation/domain errors, etc.).

---

## Where to start

- To understand how HTTP requests are wired to use cases, start with **[`API`](API.md)**.
- To see how use cases are modeled and validated, read **[`Application`](Application.md)**.
- To model your core business concepts, look at **[`Domain`](Domain.md)**.
- To plug in persistence and external services, follow **[`Infrastructure`](Infrastructure.md)**.

