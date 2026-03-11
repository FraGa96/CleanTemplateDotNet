## Infrastructure layer

The `CleanTemplate.Infrastructure` project contains **implementation details**:

- Entity Framework Core DbContext and configurations.
- Repository implementations that depend on EF Core.
- `UnitOfWork` implementation.
- Other external services (email, file storage, message brokers, etc.).

It depends on **Domain** abstractions and is wired up in the **API** via DI.

---

### Folder structure (suggested)

```text
CleanTemplate.Infrastructure
 ├── Persistence
 │    ├── AppDbContext.cs
 │    ├── UnitOfWork.cs
 │    └── Repositories
 │         └── ProjectRepository.cs
 ├── Services
 │    └── EmailSender.cs
 └── Extensions
      └── ServiceCollectionExtensions.cs
```

---

### DbContext (AppDbContext)

The template uses ASP.NET Core Identity by inheriting from `IdentityDbContext<IdentityUser>`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanTemplate.Infrastructure.Persistence;

internal class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations if you add them
        // builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

> Note: `Project` is a domain entity (defined in `CleanTemplate.Domain.Entities`).

---

### Implementing a repository

Repository implementations should depend on `AppDbContext` and implement domain repository interfaces.

```csharp
using CleanTemplate.Domain.Entities;
using CleanTemplate.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace CleanTemplate.Infrastructure.Persistence.Repositories;

internal sealed class ProjectRepository(AppDbContext context) : IProjectRepository
{
    public async Task<int> CreateAsync(Project entity)
    {
        await context.Projects.AddAsync(entity);
        return await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project entity)
    {
        context.Projects.Remove(entity);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await context.Projects.AsNoTracking().ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await context.Projects.FindAsync(id);
    }

    public Task SaveChanges()
    {
        return context.SaveChangesAsync();
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        return await context.Projects.AnyAsync(p => p.Name == name);
    }
}
```

---

### Unit of Work

`UnitOfWork` coordinates transactions across repositories.

```csharp
using System.Data;
using CleanTemplate.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanTemplate.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public async Task BeginTransaction(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await context.SaveChangesAsync(cancellationToken);
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        context.Dispose();
    }
}
```

---

### Registering Infrastructure services

Use `ServiceCollectionExtensions` to register DbContext, repositories, and UnitOfWork:

```csharp
using CleanTemplate.Domain.Abstractions;
using CleanTemplate.Domain.Projects;
using CleanTemplate.Infrastructure.Persistence;
using CleanTemplate.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanTemplate.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        // Register other infrastructure services here

        return services;
    }
}
```

