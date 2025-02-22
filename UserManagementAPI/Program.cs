using System.Net;
using System.Threading.RateLimiting;
using UserManagementAPI.Middleware;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddSingleton<ApiCallTrackingService>();

// Add services to the container.
builder.Services.AddHttpLogging(logging => {
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

// Add Rate Limiting services (using built-in middleware in .NET 7/8)
builder.Services.AddRateLimiter(options =>
{
    // Define a global limiter (per-IP-based fixed window limiter)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
    {
        // Use the client IP address.
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

    // Optionally, set a callback when a request is rejected.
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return new ValueTask();
    };
});

var app = builder.Build();

app.UseHttpLogging();
app.UseRateLimiter(); // Enable the rate limiter middleware
app.UseMiddleware<ApiCallTrackingMiddleware>();
app.UseAuthorization();
app.UseExceptionHandler("/error");
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/error");
}

app.Run();

