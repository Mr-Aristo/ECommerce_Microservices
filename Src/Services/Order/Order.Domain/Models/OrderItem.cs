namespace Order.Domain.Models;

public class OrderItem : Entity<OrderItemId>
{
    //Strongly typed ID OrderId, ProductId

    // <summary>
    /// Represents a line item in an order, containing product, quantity, and price details.
    /// Inherits from <see cref="Entity{OrderItemId}"/> with a strongly typed identifier.
    /// </summary>
    internal OrderItem(OrderId orderId, ProductId productId, int quantity, decimal price)
    {
        Id = OrderItemId.Of(Guid.NewGuid());
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
    public OrderId OrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;
}
