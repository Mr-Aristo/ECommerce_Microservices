using BuildingBlock.Logging;
using BuildingBlock.Observability;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Add services to the conatiner.

//Serilog host (shared standard config: console + optional Seq)
builder.Host.UseStandardSerilog("YarpApiGateway");

//OpenTelemetry traces + metrics (OTLP)
builder.Services.AddStandardOpenTelemetry("YarpApiGateway");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Rate Limit
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 5;
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseSerilogRequestLogging();

// RateLimit pipeline
app.UseRateLimiter();

app.MapHealthChecks("/health");

// YARP proxy pipeline
app.MapReverseProxy();

app.Run();
