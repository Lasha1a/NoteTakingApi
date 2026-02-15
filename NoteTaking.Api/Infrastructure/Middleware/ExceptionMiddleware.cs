using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NoteTaking.Api.Infrastructure.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = ((int)HttpStatusCode.InternalServerError);

            var problem = new
            {
                type = "https://httpstatuses.com/500",
                title = "Internal Server Error",
                status = 500,
                detail = "An unexpected error occurred",
                instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
