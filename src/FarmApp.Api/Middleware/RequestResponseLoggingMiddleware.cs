using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FarmApp.Api.Middleware;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    private static readonly string[] SkipPaths = ["/swagger", "/openapi", "/health"];

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (SkipPaths.Any(p => ctx.Request.Path.StartsWithSegments(p)))
        {
            await next(ctx);
            return;
        }

        ctx.Request.EnableBuffering();
        var reqBody = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
        ctx.Request.Body.Position = 0;

        var original = ctx.Response.Body;
        await using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;

        var sw = Stopwatch.StartNew();
        try
        {
            await next(ctx);
        }
        catch
        {
            // An unhandled exception skips the normal completion below, which would otherwise
            // leave ctx.Response.Body pointed at our (now-abandoned) buffer. Restore it before
            // rethrowing so ExceptionHandlingMiddleware's ProblemDetails actually reaches the
            // client instead of writing into a MemoryStream nobody reads.
            ctx.Response.Body = original;
            throw;
        }
        sw.Stop();

        buffer.Position = 0;
        var resBody = await new StreamReader(buffer).ReadToEndAsync();

        var level = ctx.Response.StatusCode >= 500 ? LogLevel.Error
            : ctx.Response.StatusCode >= 400 ? LogLevel.Warning
            : LogLevel.Information;

        logger.Log(level, "HTTP {Method} {Path} by {User} => {Status} in {Ms}ms | req {Req} | res {Res}",
            ctx.Request.Method, ctx.Request.Path, ctx.User.Identity?.Name ?? "anon",
            ctx.Response.StatusCode, sw.ElapsedMilliseconds,
            Redact(Truncate(reqBody)), Redact(Truncate(resBody)));

        buffer.Position = 0;
        await buffer.CopyToAsync(original);
    }

    private static string Truncate(string s) => s.Length > 8000 ? s[..8000] + "...[truncated]" : s;

    private static readonly Regex SensitiveField =
        new("\"(password|pin|refreshToken)\"\\s*:\\s*\"[^\"]*\"", RegexOptions.IgnoreCase);

    private static string Redact(string s) => SensitiveField.Replace(s, "\"$1\":\"***\"");
}
