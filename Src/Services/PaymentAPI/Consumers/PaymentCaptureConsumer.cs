namespace PaymentAPI.Consumers;

// Mock capture: records a (always-successful) payment when a checkout is published.
public class PaymentCaptureConsumer(IDocumentSession session, ILogger<PaymentCaptureConsumer> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var message = context.Message;
        var id = message.CheckoutId == Guid.Empty ? Guid.NewGuid() : message.CheckoutId;

        // Idempotent: ignore duplicate deliveries.
        if (await session.LoadAsync<PaymentRecord>(id, context.CancellationToken) is not null)
            return;

        session.Store(new PaymentRecord
        {
            Id = id,
            CustomerId = message.CustomerId,
            Amount = message.TotalPrice,
            Status = PaymentStatus.Captured
        });
        await session.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Payment captured (mock) for checkout {CheckoutId}, amount {Amount}.", id, message.TotalPrice);
    }
}
