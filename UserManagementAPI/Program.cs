using System.Net;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using UserManagementAPI.Middleware;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Title", Version = "v1" });
    
    // Define the API key security scheme
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. API Key must be in the 'X-API-Key' header.",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    // Add the security requirement to include the API key in requests
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddSingleton<ApiCallTrackingService>();
builder.Services.AddScoped<TokenAuthenticationService>();

// Add HTTP logging services (custom middleware will be used instead of built-in app.UseHttpLogging())
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

// Add Rate Limiting services (using built-in middleware in .NET 7/8)
builder.Services.AddRateLimiter(options =>
{
    // Global limiter based on client IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
    {
        var clientIp = context.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        return RateLimitPartition.GetFixedWindowLimiter(clientIp,
            partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // maximum 100 requests per window
                Window = TimeSpan.FromMinutes(1), // 1-minute window
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // No queuing
            });
    });

    // Callback when a request is rejected.
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return new ValueTask();
    };
});

var app = builder.Build();

// Register the custom exception handling middleware at the beginning
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable static files (for serving swagger assets, etc.)
app.UseStaticFiles();

// Register the token validation middleware before other middleware that should be protected.
app.UseMiddleware<TokenValidationMiddleware>();

// Use custom HTTP logging and API tracking middleware.
app.UseMiddleware<HttpLoggingMiddleware>();
app.UseMiddleware<ApiCallTrackingMiddleware>();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    // Only one error handling strategy should be used.
    // app.UseExceptionHandler("/Home/error");
}

app.Run();

