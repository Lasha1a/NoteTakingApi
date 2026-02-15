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
            await _next(context); // let request continiue if there is no exception
        }
        catch(Exception ex) // catch any unhandled exception
        {
            //get correlation id from context
            var correlationId = context.Items["X-Correlation-Id"]?.ToString(); 


            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");

            context.Response.ContentType = "application/problem+json"; // set content type to problem+json for standardized error response
            context.Response.StatusCode = ((int)HttpStatusCode.InternalServerError); // set status code to 500

            var problem = new // create a problem details with information about the error
            {
                type = "https://httpstatuses.com/500",
                title = "Internal Server Error",
                status = 500,
                detail = "An unexpected error occurred",
                instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem)); // write the problem details as json response
        }
    }
}
