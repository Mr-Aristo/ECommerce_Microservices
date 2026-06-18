namespace PaymentAPI.Endpoints;

// Dev/admin visibility into the simulated payment/refund record for an order.
public class GetPayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/payments/{orderId:guid}", async (Guid orderId, IDocumentSession session) =>
        {
            var payment = await session.LoadAsync<PaymentRecord>(orderId);
            return payment is null ? Results.NotFound() : Results.Ok(payment);
        })
        .WithName("GetPayment")
        .Produces<PaymentRecord>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get the payment record for an order");
    }
}
