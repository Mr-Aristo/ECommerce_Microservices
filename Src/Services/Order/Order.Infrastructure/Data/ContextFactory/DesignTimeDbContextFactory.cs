using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Order.Infrastructure.Data.ContextFactory;

/// <summary>
/// The DesignTimeDbContextFactory class is responsible for creating an instance of the ApplicationDbContext during design-time operations,
/// such as migrations. It implements the IDesignTimeDbContextFactory interface, which allows it to be used by tools like
/// Entity Framework Core CLI to create the database context when needed. The factory reads the database connection string 
/// from the appsettings.json configuration file and configures the DbContextOptions accordingly before returning a new 
/// instance of ApplicationDbContext.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Database"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
