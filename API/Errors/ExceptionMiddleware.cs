using System.Net;
using System.Text.Json;

namespace API.Errors
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                ApiException exception = new(context.Response.StatusCode, ex.Message, env.IsDevelopment() ?
                    ex.StackTrace : "Internal server error");

                string results = JsonSerializer.Serialize(exception, options);
                await context.Response.WriteAsync(results);
            }
        }
    }
}
