# CleanTemplate (.NET 9 Clean Architecture Template)

This repository is a **.NET 9 Web API template** based on a lightweight variant of **Clean Architecture**, intended as a starting point for new projects.

The solution is split into the following layers/projects:

- **CleanTemplate.API**: presentation layer (Web API).
- **CleanTemplate.Application**: application layer (use cases, validations, orchestration).
- **CleanTemplate.Domain**: domain model, entities, and contracts (no dependencies on infrastructure).
- **CleanTemplate.Infrastructure**: persistence implementation and external integrations.

---

## Technologies and main packages

- **.NET 9 / ASP.NET Core** for the API.
- **ASP.NET Core Identity** (`IdentityUser`, `IdentityDbContext`) for user management.
- **Entity Framework Core** for data access (`AppDbContext` in `Infrastructure`).
- **MediatR** for use case handling / CQRS patterns in `Application`.
- **AutoMapper** for mapping between DTOs and entities.
- **FluentValidation** for application-level validations.
- **Swagger / OpenAPI** enabled in development.

---

## Solution structure

- `CleanTemplate.API`
  - `Program.cs`: application entry point.
  - `Extensions/WebApplicationBuilderExtensions.cs`: presentation configuration (controllers, Swagger, CORS, logging, etc.).
  - `Middlewares/`:
    - `ErrorHandlingMiddleware`: centralized exception handling.
    - `RequestTimeLoggingMiddleware`: logs request processing time.
- `CleanTemplate.Application`
  - `Extensions/ServiceCollectionExtensions.cs`: registers MediatR, AutoMapper, and FluentValidation.
  - This is where you put **use cases**, **DTOs**, **mappings**, and **validations**.
- `CleanTemplate.Domain`
  - `Entities/BaseEntity.cs`: base class for domain entities (`Id` as `Guid`).
  - `Abstractions/IUnitOfWork.cs` and `Repositories/IRepository.cs`: persistence contracts.
  - `Exceptions/`: domain-specific exceptions (`DomainException`, `NotFoundException`, `DuplicatedException`, etc.).
- `CleanTemplate.Infrastructure`
  - `Persistence/AppDbContext.cs`: main DbContext (inherits from `IdentityDbContext<IdentityUser>`).
  - `Persistence/UnitOfWork.cs`: `IUnitOfWork` implementation.
  - `Extensions/ServiceCollectionExtensions.cs`: registers DbContext, repositories, unit of work, etc.

---

## Setup and run

1. **Prerequisites**
   - .NET SDK 9 installed.
   - Local SQL Server (or any database configured in `appsettings.json`).

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Apply migrations (if you add them)**

   ```bash
   dotnet ef database update -p CleanTemplate.Infrastructure -s CleanTemplate.API
   ```

4. **Run the API**

   ```bash
   dotnet run --project CleanTemplate.API
   ```

5. **Swagger**

   In development, Swagger is available at:

   - `https://localhost:5001/swagger` (or the port configured in `launchSettings.json`).

---

## Using this as a template for new projects

### Install as a dotnet template

From the folder containing this repository:

```bash
dotnet new install ./CleanTemplateDotNet
```

After installation, you can create a new solution with:

```bash
dotnet new clean-arch-api -n MyCompany.MyProduct
```

This will:

- Create a new folder `MyCompany.MyProduct`.
- Generate the solution and projects with namespaces **based on `MyCompany.MyProduct`** (using `sourceName = CleanTemplate`).

### Next steps in the generated solution

1. **Add your domain entities** in `*.Domain`.
2. **Create use cases (MediatR handlers)** in `*.Application`.
3. **Configure persistence** (tables, repositories, migrations) in `*.Infrastructure`.
4. **Add controllers/endpoints** in `*.API`, using the handlers from `Application`.

### Available template parameters

- **`UseSwagger`** (`bool`, default: `true`):
  - Controls whether Swagger / OpenAPI middleware is included in `Program.cs`.
  - Example (disable Swagger in the generated solution):
    ```bash
    dotnet new clean-arch-api -n MyCompany.MyProduct --UseSwagger false
    ```

---

## Recommended conventions

- Keep **`Domain` free of dependencies** on other layers.
- Use **MediatR** as the single entry point for use cases from the API.
- Implement **validations with FluentValidation** in `Application`.
- Keep **infrastructure logic** (EF Core, repositories, external services) only in `Infrastructure`.

