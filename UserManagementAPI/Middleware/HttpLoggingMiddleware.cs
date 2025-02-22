using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace UserManagementAPI.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        
        public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Logging the request
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            
            _logger.LogInformation($"HTTP Request Information: \n" +
                                   $"Scheme: {context.Request.Scheme} \n" +
                                   $"Host: {context.Request.Host} \n" +
                                   $"Path: {context.Request.Path} \n" +
                                   $"Query String: {context.Request.QueryString} \n" +
                                   $"Body: {requestBody}");
            
            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            await _next(context);
            
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            
            _logger.LogInformation($"HTTP Response Information: \n" +
                                   $"Status Code: {context.Response.StatusCode} \n" +
                                   $"Body: {responseText}");
            
            // Copy the contents of the new memory stream (which contains the response) to the original stream.
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}