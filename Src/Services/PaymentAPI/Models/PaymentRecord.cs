namespace PaymentAPI.Models;

public enum PaymentStatus { Captured = 1, Refunded = 2 }

// Simulated payment record (no real provider). Id = OrderId / CheckoutId.
public class PaymentRecord
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RefundedAt { get; set; }
}
