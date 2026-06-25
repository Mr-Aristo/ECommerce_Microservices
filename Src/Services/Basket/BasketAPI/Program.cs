using BasketAPI.CheckoutSaga;
using BuildingBlock.Auth;
using BuildingBlock.Exceptions.Handlers;
using BuildingBlock.Logging;
using BuildingBlock.Observability;
using DiscountGrpc.Protos;
using Serilog;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace BasketAPI;

public class Program
{
    public static void Main(string[] args)
    {
        // Outbox stores UTC DateTime (CreatedAt/PublishedAt) into Marten 'timestamp without time zone'
        // columns that the dispatcher orders/filters on. Npgsql 6+ rejects that unless legacy behavior is on.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        //Serilog host (shared standard config: console + optional Seq)
        builder.Host.UseStandardSerilog("BasketAPI");

        //OpenTelemetry traces + metrics (OTLP)
        builder.Services.AddStandardOpenTelemetry("BasketAPI");

        //Keycloak JWT auth (available; enforcement added with the protected endpoints)
        builder.Services.AddStandardJwtAuth(builder.Configuration);

        //MediatR config
        var assembly = typeof(Program).Assembly;

        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));//<,> MediatR Open Generics 
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        builder.Services.AddValidatorsFromAssembly(assembly);

        //Grpc Config
        builder.Services.AddGrpc();

        //MinimalAPIs
        builder.Services.AddCarter();

        //Marten config
        builder.Services.AddMarten(opts =>
        {
            opts.Connection(builder.Configuration.GetConnectionString("PostgreDataBase")!);// With ! GetConnectionSting cannot be null
            opts.Schema.For<ShoppingCard>().Identity(x => x.UserName);
            // OUTBOX/SAGA: local outbox document schema for checkout integration events.
            opts.Schema.For<BasketCheckoutOutboxMessage>().Identity(x => x.Id);
        });

        //Redis Config
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "Basket";
        });

 
        // Register the concrete `BasketRepository` as the implementation for `IBasketRepository`
        // with a scoped lifetime (one instance per HTTP request). Scoped lifetime is commonly
        // chosen for repositories that depend on request-scoped services (e.g., DB sessions).
        builder.Services.AddScoped<IBasketRepository, BasketRepository>();
        
        // Apply the `CachedBasketRepository` decorator over `IBasketRepository` using Scrutor's
        // `Decorate` extension. The decorator wraps the inner `IBasketRepository` implementation
        // to provide caching behavior (for example, checking/updating Redis cache) without
        // changing the underlying repository implementation.
        //
        // Call order when resolving `IBasketRepository`:
        // Consumer -> `CachedBasketRepository` (decorator) -> `BasketRepository` (inner)
        //
        // Implementation note: the `CachedBasketRepository` must accept an `IBasketRepository`
        // (the inner/decorated dependency) in its constructor so Scrutor can inject it.
        builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();//From BuildingBlock
                                                                       //IBasketRepository calls → CachedBasketRepository runs → Cache check → (if there is no Cache) DB calls and cache updates.

        //Grpc Services
        builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
        {
            options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);//appsettings.json
        })
         .ConfigurePrimaryHttpMessageHandler(() =>
        {
            if (builder.Environment.IsDevelopment())
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            }

            return new HttpClientHandler();
        });

        //MessageBroker
        // OUTBOX/SAGA: register consumers (success/fail) from this assembly.
        builder.Services.AddMessageBroker(builder.Configuration, assembly);
        // OUTBOX/SAGA: background worker that publishes pending outbox records.
        builder.Services.AddHostedService<BasketCheckoutOutboxDispatcher>();


        //HealthCheck
        builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("PostgreDataBase")!)
            .AddRedis(builder.Configuration.GetConnectionString("Redis")!);


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSerilogRequestLogging();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                 ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }
        );

        app.Run();
    }
}
