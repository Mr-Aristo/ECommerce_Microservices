using System.Threading.RateLimiting;
using BuildingBlock.Auth;
using BuildingBlock.Logging;
using BuildingBlock.Observability;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Add services to the conatiner.

//Serilog host (shared standard config: console + optional Seq)
builder.Host.UseStandardSerilog("YarpApiGateway");

//OpenTelemetry traces + metrics (OTLP)
builder.Services.AddStandardOpenTelemetry("YarpApiGateway");

//Keycloak JWT auth (edge); route-level authz policies added later
builder.Services.AddStandardJwtAuth(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Trust forwarded headers so the REAL client IP (not the proxy hop) drives rate-limit
// partitioning. KnownProxies/Networks are cleared for local/dev; in production restrict
// these to the trusted load balancer range, otherwise X-Forwarded-For can be spoofed.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Rate Limit — partitioned per client (authenticated sub, else client IP) so one abusive
// caller can no longer exhaust a single shared bucket and lock everyone out.
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    rateLimiterOptions.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "10";
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", token);
    };

    static string PartitionKey(HttpContext http) =>
        http.User.FindFirst("sub")?.Value
        ?? http.Connection.RemoteIpAddress?.ToString()
        ?? "anonymous";

    // Strict: checkout / ordering (sensitive, write-heavy). Name kept as "fixed" for the existing ordering-route.
    rateLimiterOptions.AddPolicy("fixed", http =>
        RateLimitPartition.GetFixedWindowLimiter(PartitionKey(http),
            _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromSeconds(10), PermitLimit = 5 }));

    // Moderate: identity-adjacent routes (basket, users).
    rateLimiterOptions.AddPolicy("auth-sensitive", http =>
        RateLimitPartition.GetFixedWindowLimiter(PartitionKey(http),
            _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromSeconds(10), PermitLimit = 20 }));

    // Loose: public catalog reads (still capped per client).
    rateLimiterOptions.AddPolicy("catalog-loose", http =>
        RateLimitPartition.GetFixedWindowLimiter(PartitionKey(http),
            _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromSeconds(10), PermitLimit = 100 }));
});

builder.Services.AddHealthChecks();

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseSerilogRequestLogging();

// Resolve real client IP from X-Forwarded-For before auth/rate-limiting run.
app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();

// RateLimit pipeline
app.UseRateLimiter();

app.MapHealthChecks("/health");

// YARP proxy pipeline
app.MapReverseProxy();

app.Run();
