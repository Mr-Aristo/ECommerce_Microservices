
namespace BasketAPI.Basket.StoreBasket;
public record StoreBasketRequest(ShoppingCard Cart);
public record StoreBasketResponse(string UserName);

public class StoreBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket-store", async (StoreBasketRequest request, ClaimsPrincipal user, ISender sender) =>
        {
            // The basket is always keyed by the authenticated user (sub), never client-supplied.
            request.Cart.UserName = user.GetUserId();

            var command = request.Adapt<StoreBasketCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<StoreBasketResponse>();

            return Results.Created($"/basket/{response.UserName}", response);
        })
        .RequireAuthorization()
        .WithName("StoreBasket")
        .Produces<StoreBasketResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Store Basket")
        .WithDescription("Create or update the caller's basket");
    }
}