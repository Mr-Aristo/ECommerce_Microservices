using BasketAPI.Models;

namespace BasketAPI.Basket.GetBasket;

//public record GetBasketRequest(string UserName); 
public record GetBasketResponse(ShoppingCard Cart);

public class GetBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{userName}", async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new GetBasketQuery(userName));

            var respose = result.Adapt<GetBasketResponse>();

            return Results.Ok(respose);
        })
        .WithName("GetProductById")
        .Produces<GetBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Product By Id")
        .WithDescription("Get Product By Id");
    }
}