using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            var correlationId = ctx.Items["CorrelationId"]?.ToString() ?? "unknown";
            logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = 500,
                Detail = $"Reference: {correlationId}",
            };
            await ctx.Response.WriteAsJsonAsync(problem);
        }
    }
}
