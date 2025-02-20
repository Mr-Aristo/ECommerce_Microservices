using Microsoft.EntityFrameworkCore;

namespace DiscountGrpc.Data;

public static class Extentions
{
    /// <summary>
    /// Applies pending EF Core migrations to the database during application startup.
    /// Ensures the database schema is up-to-date with the current application model.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The application builder instance for further configuration chaining.</returns>
    public static IApplicationBuilder UseMigration(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<DiscountContext>();
        dbContext.Database.MigrateAsync();

        return app; 
    }
}
