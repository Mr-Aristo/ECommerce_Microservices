using BuildingBlock.Behaviors;
using BuildingBlockMessaging.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Reflection;


namespace Order.Application;

public static class DependencyInjections
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection service, IConfiguration configuration)
    {

        service.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        service.AddFeatureManagement();
        //service.AddMessageBroker(configuration, Assembly.GetExecutingAssembly);

        return service;
    }

}
/*
Assembly.GetExecutingAssembly() Bu, kodun çalıştığı assembly'yi (çalıştırılan derlemeyi)
temsil eder ve ilgili metotlara bu assembly'yi parametre olarak geçmeye yarar. 
 */
