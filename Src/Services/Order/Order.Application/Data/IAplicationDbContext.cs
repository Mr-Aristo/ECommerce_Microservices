
namespace Order.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }
    DbSet<Orders> Orders { get; }
    DbSet<OrderItem> OrdersItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}