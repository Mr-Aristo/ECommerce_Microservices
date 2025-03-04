namespace Order.API;


public static class DependecyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddCarter();

        service.AddExceptionHandler<CustomExceptionHandler>();

        service.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!);

        return service;

    }
    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.MapCarter();

        app.UseExceptionHandler();
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

        return app;

    }
}
