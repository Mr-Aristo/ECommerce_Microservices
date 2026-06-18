namespace PaymentAPI.Consumers;

// Mock refund: when a return is approved, refund the payment and tell Order it completed.
public class ReturnRefundConsumer(IDocumentSession session, IPublishEndpoint publishEndpoint, ILogger<ReturnRefundConsumer> logger)
    : IConsumer<ReturnApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ReturnApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        var payment = await session.LoadAsync<PaymentRecord>(message.OrderId, context.CancellationToken)
            ?? new PaymentRecord
            {
                Id = message.OrderId,
                CustomerId = message.CustomerId,
                Amount = message.Amount,
                Status = PaymentStatus.Captured
            };

        if (payment.Status != PaymentStatus.Refunded)
        {
            payment.Status = PaymentStatus.Refunded;
            payment.RefundedAt = DateTime.UtcNow;
            session.Store(payment);
            await session.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Refund processed (mock) for order {OrderId}, amount {Amount}.", message.OrderId, message.Amount);
        }

        await context.Publish(new RefundCompletedIntegrationEvent
        {
            OrderId = message.OrderId,
            CustomerId = message.CustomerId,
            Amount = message.Amount
        });
    }
}
