using BuildingBlock.Exceptions.Handlers;
using DiscountGrpc.Protos;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace BasketAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        //MediatR config
        var assembly = typeof(Program).Assembly;

        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));//<,> MediatR Open Generics
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Add services to the container.
        builder.Services.AddGrpc();

        //MinimalAPIs
        builder.Services.AddCarter();

        //Marten config
        builder.Services.AddMarten(opts =>
        {
            opts.Connection(builder.Configuration.GetConnectionString("PostgreDataBase")!);// With ! GetConnectionSting cannot be null
            opts.Schema.For<ShoppingCard>().Identity(x => x.UserName);
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
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            return handler;
        });

        //MessageBroker
        builder.Services.AddMessageBroker(builder.Configuration); // Basket is publisher and no need to send assmebly


        //HealthCheck
        builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("PostgreDataBase")!)
            .AddRedis(builder.Configuration.GetConnectionString("Redis")!);


        var app = builder.Build();

        // Configure the HTTP request pipeline.
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