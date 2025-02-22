using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UserManagementAPI.Services;

namespace UserManagementAPI.Middleware
{
    public class TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<TokenValidationMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {

            var path = context.Request.Path.ToString();
    
            // Bypass API key authentication for Swagger UI and its resources
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Check for token in the header ("X-Api-Key").
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var token))
            {
                _logger.LogWarning("No API token provided.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: API token was not provided.");
                return;
            }

        
            // Resolve the scoped TokenAuthenticationService from the current request scope.
            var tokenAuthService = context.RequestServices.GetRequiredService<TokenAuthenticationService>();

            var tokenString = token.ToString();
            if (string.IsNullOrEmpty(tokenString) || !tokenAuthService.IsValidToken(tokenString))
            {
                _logger.LogWarning("Invalid API token provided.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid API token.");
                return;
            }

            // Proceed to next middleware if token is valid.
            await _next(context);
        }
    }
}