using DiscountGrpc.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscountGrpc.Data;

public class DiscountContext :DbContext
{
    public DbSet<Coupon> Coupons { get; set; } = default!;

	public DiscountContext(DbContextOptions<DiscountContext> options):base(options)
	{
	}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().HasData(
           new Coupon { Id = 1, ProductName = "IPhone X", Description = "IPhone Discount", Amount = 150 },
           new Coupon { Id = 2, ProductName = "Samsung 10", Description = "Samsung Discount", Amount = 100 },
           new Coupon { Id = 3, ProductName = "Samsung 12", Description = "Samsung Discount", Amount = 200 },
           new Coupon { Id = 4, ProductName = "Samsung 13", Description = "Samsung Discount", Amount = 150 },
           new Coupon { Id = 5, ProductName = "Samsung 14", Description = "Samsung Discount", Amount = 100 }
           );
    }
}
