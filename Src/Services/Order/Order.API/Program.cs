
using BuildingBlock.Logging;
using BuildingBlock.Observability;
using Order.Infrastructure.Data.Extentions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container

//Serilog host (shared standard config: console + optional Seq)
builder.Host.UseStandardSerilog("Order.API");

//OpenTelemetry traces + metrics (OTLP)
builder.Services.AddStandardOpenTelemetry("Order.API");

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    // Auto Migrate
    await app.InitialiseDatabaseAsync();
    app.UseExceptionHandler("/error");
}

// Configure the http request pipeline
app.Run();
