namespace BuildingBlockMessaging.Events;

public record BasketCheckoutItemEvent
{
    public Guid ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    public decimal Price { get; set; } = default!;
}
