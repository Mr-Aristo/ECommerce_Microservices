using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static MassTransit.Logging.OperationName;


namespace BuildingBlockMessaging.MassTransit;

public static class Extentions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection service, IConfiguration configuration, Assembly? assembly = null)
    {
        service.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            //Assembly’i yalnızca “consumer” olan serviste veriyoruz, çünkü MassTransit o assembly’i IConsumer<>
            //sınıflarını bulup kaydetmek için tarıyor (reflection). Publisher’da consumer yok, dolayısıyla taranacak bir şey de yok.
            if (assembly is not null)
                config.AddConsumers(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"]!);
                    host.Password(configuration["MessageBroker:Password"]!);
                });
                configurator.ConfigureEndpoints(context);
            });
            
        });

        return service; 
    }
}
