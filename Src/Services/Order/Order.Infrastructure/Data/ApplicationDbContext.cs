

using Order.Domain.Models;
using System.Reflection;

namespace Order.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) :base(dbContext)
    { }
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products =>Set<Product>();

    public DbSet<Orders> Orders => Set<Orders>();

    public DbSet<OrderItem> OrdersItems => Set<OrderItem>();

    /// <summary>
    /// Configurations/..../ 
    ///
    /// Configures the EF Core model when the DbContext is being built.
    /// 
    /// This method uses <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/> 
    /// to automatically scan the current assembly (via <see cref="Assembly.GetExecutingAssembly"/>)
    /// for all classes that implement the <see cref="IEntityTypeConfiguration{TEntity}"/> interface.
    /// 
    /// - EF Core uses reflection to find these configuration classes (e.g., OrderConfiguration, CustomerConfiguration).
    /// - Each configuration is automatically applied to the model, so you don’t need to register them manually.
    /// - This approach helps keep entity configuration organized in separate classes and follows the 
    ///   Single Responsibility Principle, instead of putting all configuration inside the DbContext.
    /// 
    /// Finally, the base implementation (<see cref="DbContext.OnModelCreating"/>) is called 
    /// to ensure any additional EF Core internal setup is preserved.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
