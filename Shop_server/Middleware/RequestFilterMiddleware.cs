using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shop_server.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RequestFilterMiddleware: IMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<RequestFilterMiddleware> _logger;

        public RequestFilterMiddleware(
            ILogger<RequestFilterMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);

            if (context.Response.HasStarted) return;

            _logger.LogWarning("Unknown endpoinn: {Method}/:{Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = StatusCodes.Status500InternalServerError,
                error = "Server error",
                message = "Nem létezik ilyen REST API végpont!",
                method = context.Request.Method,
                path = context.Request.Path.Value
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RequestFilterMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestFilterMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestFilterMiddleware>();
        }
    }
}
