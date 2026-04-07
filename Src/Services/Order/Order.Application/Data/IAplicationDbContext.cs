
namespace Order.Application.Data;

/// <summary>
/// The IApplicationDbContext interface defines the contract for the application's database context.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }
    DbSet<Orders> Orders { get; }
    DbSet<OrderItem> OrdersItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}