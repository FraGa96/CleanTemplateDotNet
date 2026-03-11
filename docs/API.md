## API (presentation layer)

The `CleanTemplate.API` project is the **presentation layer**:

- ASP.NET Core Web API startup (`Program.cs`).
- Middleware pipeline (error handling, logging, timeouts, etc.).
- HTTP endpoints (minimal APIs or controllers) that call **Application** layer use cases.

It should not contain business logic; instead, it **delegates to MediatR** commands/queries.

---

### Startup (Program.cs)

The template wires up all layers in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddPresentation();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeLoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseRequestTimeouts();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");

app.UseHttpsRedirection();

app.MapGroup("identity")
    .WithTags("Identity")
    .MapIdentityApi<IdentityUser>();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

`AddPresentation()` is the extension that configures controllers, Swagger, CORS, etc.

---

### Folder structure (suggested)

```text
CleanTemplate.API
 ├── Controllers
 │    └── ProjectsController.cs
 ├── Middlewares
 │    ├── ErrorHandlingMiddleware.cs
 │    └── RequestTimeLoggingMiddleware.cs
 ├── Extensions
 │    └── WebApplicationBuilderExtensions.cs
 └── Program.cs
```

---

### Creating a new controller endpoint

Controllers should:

- Accept HTTP requests.
- Convert them into **Application commands/queries**.
- Send them through **IMediator**.
- Return appropriate HTTP status codes.

```csharp
using CleanTemplate.Application.Projects.Commands.CreateProject;
using CleanTemplate.Application.Projects.Queries.GetProjectById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanTemplate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await mediator.Send(new GetProjectByIdQuery(id), cancellationToken);

        return project is null
            ? NotFound()
            : Ok(project);
    }
}
```

---

### Error handling and logging

The template uses:

- `ErrorHandlingMiddleware` for centralized exception handling and unified error responses.
- `RequestTimeLoggingMiddleware` to log request execution times.
- `UseSerilogRequestLogging()` (if Serilog is configured) for structured request logs.

Keep middleware focused on **cross-cutting concerns** (logging, auth, correlation IDs, etc.), not business rules.

