using BuildingBlock.Exceptions.Handlers;
using BuildingBlock.Logging;
using BuildingBlock.Observability;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using UsersAPI.Integration.Catalog;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

// Cross-cutting (shared building blocks)
builder.Host.UseStandardSerilog("UsersAPI");
builder.Services.AddStandardOpenTelemetry("UsersAPI");
builder.Services.AddStandardJwtAuth(builder.Configuration);

// CQRS + validation
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();

// Catalog HTTP client for favorites enrichment (best-effort read; degrades if Catalog is down)
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog"] ?? "http://localhost:6000");
});

// Persistence (Marten / PostgreSQL); UserProfile keyed by Keycloak sub
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("PostgreDataBase")!);
    opts.Schema.For<UserProfile>().Identity(x => x.Id);
}).UseLightweightSessions();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreDataBase")!);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
