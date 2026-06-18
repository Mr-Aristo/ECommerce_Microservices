using BuildingBlock.Logging;
using BuildingBlock.Observability;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Host.UseStandardSerilog("PaymentAPI");
builder.Services.AddStandardOpenTelemetry("PaymentAPI");
builder.Services.AddCarter();

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("PostgreDataBase")!);
    opts.Schema.For<PaymentRecord>().Identity(x => x.Id);
}).UseLightweightSessions();

// Registers this assembly's consumers (capture / refund) with RabbitMQ.
builder.Services.AddMessageBroker(builder.Configuration, assembly);

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreDataBase")!);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapCarter();
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
