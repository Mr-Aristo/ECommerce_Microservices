using Microsoft.EntityFrameworkCore;
using Order.Application.DTOs;
using Order.Application.OrdersCQRS.Commands.CreateOrder;
using Order.Domain.Enums;
using Order.Domain.Models;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Data;

namespace ECommerce_Tests.Order;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateMissingCustomerAndProductReferences()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"create-order-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        var sut = new CreateOrderHandler(dbContext);

        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new CreateOrderCommand(CreateOrder(customerId, productId, 250));

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.id);

        var customer = await dbContext.Customers.FindAsync([CustomerId.Of(customerId)], CancellationToken.None);
        var product = await dbContext.Products.FindAsync([ProductId.Of(productId)], CancellationToken.None);

        Assert.NotNull(customer);
        Assert.NotNull(product);
        Assert.StartsWith("CatalogProduct-", product!.Name, StringComparison.Ordinal);

        Assert.Equal(1, await dbContext.Orders.CountAsync());
        Assert.Equal(1, await dbContext.OrdersItems.CountAsync());
    }

    [Fact]
    public async Task Handle_ShouldNotDuplicateCustomerOrProduct_WhenAlreadyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"create-order-existing-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new ApplicationDbContext(options);

        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        dbContext.Customers.Add(Customer.Create(CustomerId.Of(customerId), "Jane Doe", "jane@doe.com"));
        dbContext.Products.Add(Product.Create(ProductId.Of(productId), "Existing Product", 120));
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var sut = new CreateOrderHandler(dbContext);
        var command = new CreateOrderCommand(CreateOrder(customerId, productId, 120));

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, await dbContext.Customers.CountAsync(c => c.Id == CustomerId.Of(customerId)));
        Assert.Equal(1, await dbContext.Products.CountAsync(p => p.Id == ProductId.Of(productId)));
        Assert.Equal(1, await dbContext.Orders.CountAsync());
    }

    private static OrderDto CreateOrder(Guid customerId, Guid productId, decimal price)
    {
        return new OrderDto(
            Id: Guid.NewGuid(),
            CustomerId: customerId,
            OrderName: "ORD_TEST",
            ShippingAddress: new AddressDto("Jane", "Doe", "jane@doe.com", "Main Street", "TR", "IST", "34000"),
            BillingAddress: new AddressDto("Jane", "Doe", "jane@doe.com", "Main Street", "TR", "IST", "34000"),
            Payment: new PaymentDto("Jane Doe", "4111111111111111", "12/30", "123", 1),
            Status: OrderStatus.Pending,
            OrderItems:
            [
                new OrderItemDto(Guid.NewGuid(), productId, 1, price)
            ]);
    }
}
