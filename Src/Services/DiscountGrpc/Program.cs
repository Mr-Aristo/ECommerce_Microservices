using BuildingBlock.Logging;
using BuildingBlock.Observability;
using DiscountGrpc.Data;
using DiscountGrpc.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Serilog host (shared standard config: console + optional Seq)
builder.Host.UseStandardSerilog("DiscountGrpc");

//OpenTelemetry traces + metrics (OTLP)
builder.Services.AddStandardOpenTelemetry("DiscountGrpc");

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddDbContext<DiscountContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
app.UseMigration();
app.MapHealthChecks("/health");
// Map the gRPC service to the application's request pipeline, allowing it to handle incoming gRPC requests.
app.MapGrpcService<DiscountService>();

app.Run();
