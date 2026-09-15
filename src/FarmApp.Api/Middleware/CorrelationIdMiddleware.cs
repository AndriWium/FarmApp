using Serilog.Context;

namespace FarmApp.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var correlationId = Guid.NewGuid().ToString();
        ctx.Items["CorrelationId"] = correlationId;
        ctx.Response.Headers["X-Correlation-Id"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(ctx);
        }
    }
}
