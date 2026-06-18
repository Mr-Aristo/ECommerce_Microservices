namespace Order.Domain.Models;

// Audit entry for an order status transition (persisted as a JSON column on Orders).
public class OrderStatusHistory
{
    public OrderStatus Status { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
