using BuildingBlock.Logging;
using BuildingBlock.Observability;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Host.UseStandardSerilog("NotificationAPI");
builder.Services.AddStandardOpenTelemetry("NotificationAPI");

// Registers this assembly's consumers (order/return notifications) with RabbitMQ.
builder.Services.AddMessageBroker(builder.Configuration, assembly);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");

app.Run();
