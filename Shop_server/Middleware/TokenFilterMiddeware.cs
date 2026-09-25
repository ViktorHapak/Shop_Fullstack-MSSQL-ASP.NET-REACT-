
using Microsoft.AspNetCore.Authorization;
using Shop_server.Security;
using System.Text.Json;

namespace Shop_server.Middleware
{
    public class TokenFilterMiddleware : IMiddleware
    {

        private readonly RequestDelegate _next;

        private readonly ILogger<TokenFilterMiddleware> _logger;

        private readonly JwtUnit jwtUnit;

        public TokenFilterMiddleware(
            ILogger<TokenFilterMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Endpoint? endpoint = context.GetEndpoint();

            bool requiresAuthorization = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>().Count > 0;

            if (!requiresAuthorization)
            {
                await next(context);
                return;
            }

            /*
             * UseAuthentication is already done
             * If JWT is correct, it will be autenticated here.
             * ClaimsPrincipal.
             */
            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var response = new
                {
                    status = StatusCodes.Status401Unauthorized,
                    error = "Autenticational error",
                    message = "Autentiációs hiba!",
                    method = context.Request.Method,
                    path = context.Request.Path.Value
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            await next(context);
        }
    }
}
