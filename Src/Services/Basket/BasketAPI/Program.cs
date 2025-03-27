﻿using BuildingBlock.Exceptions.Handlers;
using DiscountGrpc.Protos;
using JasperFx.Core;

namespace BasketAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddGrpc();

        //MediatR config
        var assembly = typeof(Program).Assembly;
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));//<,> MediatR Open Generics
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        //MinimalAPIs
        builder.Services.AddCarter();

        //DataServices
        builder.Services.AddMarten(opts =>
        {
            opts.Connection(builder.Configuration.GetConnectionString("PostgreDataBase")!);// With ! GetConnectionSting cannot be null
            opts.Schema.For<ShoppingCard>().Identity(x => x.UserName);
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            //options.InstanceName = "Basket";
        });

        //Dependency Injection With Scrutor ("Decorator Pattern") library
        builder.Services.AddScoped<IBasketRepository ,BasketRepository>();
        builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();//From BuildingBlock
       //IBasketRepository calls → CachedBasketRepository runs → Cache check → (if there is no Cache) DB calls and cache updates.
      

        //HealthCheck
        builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("PostgreDataBase")!)
            .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

        //Grpc Services
        builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
        {
            options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);//appsettings.json
        });

        //MessageBroker
        builder.Services.AddMessageBroker(builder.Configuration);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseHealthChecks("/health",
        new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
     
        app.Run();
    }
}