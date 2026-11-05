namespace TaskFlow.Api.Middleware;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var incomingId = context.Request.Headers[HeaderName].FirstOrDefault();

        var id = IsValid(incomingId)
            ? incomingId!
            : Guid.NewGuid().ToString("N");

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = id;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = id
        }))
        {
            await next(context);
        }
    }

    private static bool IsValid(string? value)
    {
        return !string.IsNullOrEmpty(value)
            && value.Length <= 64
            && value.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');
    }
}