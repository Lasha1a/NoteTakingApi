using Serilog.Context;

namespace NoteTaking.Api.Infrastructure.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        //read correlation id from the request header, if it exists
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        // If client didn't send one, generate a new one
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Add the correlation id to the response header so clients can see it
        context.Items[CorrelationIdHeader] = correlationId;

        //add it to the response header so clients can see it
        context.Response.Headers[CorrelationIdHeader] = correlationId;
         
        using (LogContext.PushProperty("CorrelationId", correlationId)) //push to serilog
        {
            await _next(context);
        }
    }
}
