using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UserManagementAPI.Services;

namespace UserManagementAPI.Middleware
{
    public class ApiCallTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiCallTrackingMiddleware> _logger;
        private readonly ApiCallTrackingService _trackingService;

        public ApiCallTrackingMiddleware(RequestDelegate next, ILogger<ApiCallTrackingMiddleware> logger, ApiCallTrackingService trackingService)
        {
            _next = next;
            _logger = logger;
            _trackingService = trackingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            _trackingService.TrackCall(path);

            _logger.LogInformation($"API route '{path}' has been called {_trackingService.GetApiCallCounts()[path]} times.");

            await _next(context);
        }
    }
}