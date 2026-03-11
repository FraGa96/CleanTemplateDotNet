using CleanTemplate.API.Extensions;
using CleanTemplate.API.Middlewares;
using CleanTemplate.Application.Extensions;
using CleanTemplate.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddPresentation();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeLoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseRequestTimeouts();

#if (UseSwagger)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endif

app.UseCors("AllowAnyOrigin");

app.UseHttpsRedirection();

app.MapGroup("identity")
    .WithTags("Identity")
    .MapIdentityApi<IdentityUser>();

app.UseAuthorization();

app.MapControllers();

app.Run();

