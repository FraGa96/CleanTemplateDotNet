namespace CleanTemplate.API.Middlewares;

public class RequestTimeLoggingMiddleware(RequestDelegate next, ILogger<RequestTimeLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        logger.LogInformation("Request {Method} {Path} completed in {ElapsedMilliseconds} ms",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds);
    }
}

