

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
