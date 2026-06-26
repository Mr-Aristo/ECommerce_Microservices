namespace BasketAPI.Basket.CheckoutBasket;

// Records for API Endpoints
public record CheckoutBasketRequest(BasketCheckoutDto BasketCheckoutDto);
public record CheckoutBasketResponse(bool IsSuccess);


public class CheckoutBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ClaimsPrincipal user, HttpContext httpContext, ISender sender) =>
        {
            // Identity is taken from the token (sub), not from the client payload.
            var sub = user.GetUserId();
            request.BasketCheckoutDto.UserName = sub;
            request.BasketCheckoutDto.CustomerId = Guid.TryParse(sub, out var customerId) ? customerId : Guid.Empty;

            // Optional client idempotency key: a retried checkout with the same key replays the
            // original result instead of starting a second checkout. Absent = legacy behavior.
            var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();

            var command = new CheckoutBasketCommand(request.BasketCheckoutDto, idempotencyKey);

            var result = await sender.Send(command);

            var response = result.Adapt<CheckoutBasketResponse>();

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("CheckoutBasket")
        .Produces<CheckoutBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Checkout Basket")
        .WithDescription("Checkout Basket");
    }
}