namespace BasketAPI.Basket.CheckoutBasket;

// Records for API Endpoints
public record CheckoutBasketRequest(BasketCheckoutDto BasketCheckoutDto);
public record CheckoutBasketResponse(bool IsSuccess);


public class CheckoutBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ClaimsPrincipal user, ISender sender) =>
        {
            // Identity is taken from the token (sub), not from the client payload.
            var sub = user.GetUserId();
            request.BasketCheckoutDto.UserName = sub;
            request.BasketCheckoutDto.CustomerId = Guid.TryParse(sub, out var customerId) ? customerId : Guid.Empty;

            var command = request.Adapt<CheckoutBasketCommand>();

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