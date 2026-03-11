## Application layer

The `CleanTemplate.Application` project coordinates **use cases** for the system:

- Commands and queries (input models) and their **handlers**.
- DTOs and mapping profiles (with AutoMapper).
- Validation rules (with FluentValidation).

This layer depends on **Domain** abstractions and **does not know** about Infrastructure or API.

---

### Folder structure (suggested)

Group use cases by feature or aggregate:

```text
CleanTemplate.Application
 ├── Projects
 │    ├── Commands
 │    │    ├── CreateProject
 │    │    │    ├── CreateProjectCommand.cs
 │    │    │    ├── CreateProjectCommandHandler.cs
 │    │    │    └── CreateProjectCommandValidator.cs
 │    │    └── UpdateProject
 │    ├── Queries
 │    │    └── GetProjectById
 │    │         ├── GetProjectByIdQuery.cs
 │    │         └── GetProjectByIdQueryHandler.cs
 │    ├── Dtos
 │    │    └── ProjectDto.cs
 │    ├── Mappings
 │    │    └── ProjectProfile.cs
 └── Extensions
      └── ServiceCollectionExtensions.cs
```

---

### Creating a new command + handler

Use **MediatR** to model commands and their handlers.

```csharp
using MediatR;

namespace CleanTemplate.Application.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(string Name, string? Description)
    : IRequest<Guid>;
```

Handler example:

```csharp
using CleanTemplate.Domain.Abstractions;
using CleanTemplate.Domain.Entities;
using CleanTemplate.Domain.Projects;
using MediatR;

namespace CleanTemplate.Application.Projects.Commands.CreateProject;

internal sealed class CreateProjectCommandHandler(
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProjectCommand, Guid>
{
    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project(request.Name, request.Description);

        await projectRepository.CreateAsync(project);
        await unitOfWork.CommitAsync(cancellationToken);

        return project.Id;
    }
}
```

> Handlers should orchestrate domain entities and repositories, not contain complex business rules themselves.

---

### Creating a new query + handler

Queries usually return DTOs tailored for reading.

```csharp
using MediatR;

namespace CleanTemplate.Application.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto?>;
```

DTO example:

```csharp
namespace CleanTemplate.Application.Projects.Dtos;

public sealed record ProjectDto(Guid Id, string Name, string? Description);
```

Handler example:

```csharp
using AutoMapper;
using CleanTemplate.Application.Projects.Dtos;
using CleanTemplate.Domain.Projects;
using MediatR;

namespace CleanTemplate.Application.Projects.Queries.GetProjectById;

internal sealed class GetProjectByIdQueryHandler(
    IProjectRepository projectRepository,
    IMapper mapper) : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.Id);

        return project is null
            ? null
            : mapper.Map<ProjectDto>(project);
    }
}
```

---

### Validation with FluentValidation

Define validators per command in `Validators/`:

```csharp
using FluentValidation;

namespace CleanTemplate.Application.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

Validators are automatically registered by `ServiceCollectionExtensions`:

```csharp
services.AddValidatorsFromAssembly(applicationAssembly)
        .AddFluentValidationAutoValidation();
```

---

### Mapping profiles (AutoMapper)

Use AutoMapper profiles to map between domain entities and DTOs:

```csharp
using AutoMapper;
using CleanTemplate.Application.Projects.Dtos;
using CleanTemplate.Domain.Entities;

namespace CleanTemplate.Application.Projects.Mappings;

public sealed class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<Project, ProjectDto>();
    }
}
```

