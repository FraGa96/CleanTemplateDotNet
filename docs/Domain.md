## Domain layer

The `CleanTemplate.Domain` project contains **pure domain code**:

- Entities and value objects.
- Domain contracts (`IRepository<>`, `IUnitOfWork`, domain services).
- Domain-specific exceptions.

It **must not depend** on other projects (Application, Infrastructure, API).

---

### Folder structure (suggested)

- `Entities/` – domain entities and value objects.
- `Repositories/` – repository interfaces.
- `Abstractions/` – cross-cutting domain contracts (e.g. `IUnitOfWork`).
- `Exceptions/` – domain-level exceptions.

Example:

```text
CleanTemplate.Domain
 ├── Entities
 │    ├── BaseEntity.cs
 │    └── User.cs
 ├── Repositories
 │    └── IUserRepository.cs
 ├── Abstractions
 │    └── IUnitOfWork.cs
 └── Exceptions
      ├── DomainException.cs
      └── NotFoundException.cs
```

---

### Creating a new entity

Extend `BaseEntity` for aggregate roots or main entities that require an `Id`.

```csharp
namespace CleanTemplate.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Project() { } // For EF

    public Project(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Project name is required.");
        }

        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Project name is required.");
        }

        Name = name;
        Description = description;
    }
}
```

---

### Creating a new repository interface

Prefer **specific repository interfaces** that extend `IRepository<TEntity, TKey>` when needed.

```csharp
using CleanTemplate.Domain.Entities;
using CleanTemplate.Domain.Repositories;

namespace CleanTemplate.Domain.Projects;

public interface IProjectRepository : IRepository<Project, Guid>
{
    Task<bool> ExistsWithNameAsync(string name);
}
```

The implementation of this interface belongs in the **Infrastructure** project.

---

### Using domain exceptions

Domain exceptions live in `CleanTemplate.Domain.Exceptions` and represent **business rule violations**.

```csharp
using CleanTemplate.Domain.Exceptions;

namespace CleanTemplate.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; } = default!;
    public bool IsCompleted { get; private set; }

    public TaskItem(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Task title cannot be empty.");
        }

        Title = title;
    }

    public void Complete()
    {
        if (IsCompleted)
        {
            throw new DomainException("Task is already completed.");
        }

        IsCompleted = true;
    }
}
```

