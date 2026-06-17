
namespace BasketAPI.Basket.GetBasket;

//public record GetBasketRequest(string UserName); 
public record GetBasketResponse(ShoppingCard Cart);

public class GetBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket", async (ClaimsPrincipal user, ISender sender) =>
        {
            // Identity comes from the token (sub), not the route.
            var result = await sender.Send(new GetBasketQuery(user.GetUserId()));

            var response = result.Adapt<GetBasketResponse>();

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("GetBasket")
        .Produces<GetBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Basket")
        .WithDescription("Get the basket for the given user name");
    }
}