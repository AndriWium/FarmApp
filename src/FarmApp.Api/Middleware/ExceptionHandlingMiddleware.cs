using FarmApp.Domain.Entities;
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
        catch (PeriodLockedException ex)
        {
            var correlationId = ctx.Items["CorrelationId"]?.ToString() ?? "unknown";
            logger.LogWarning(ex, "Rejected write into closed period {Year:D4}-{Month:D2}. CorrelationId={CorrelationId}",
                ex.Year, ex.Month, correlationId);

            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Accounting period is closed",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message,
            });
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
