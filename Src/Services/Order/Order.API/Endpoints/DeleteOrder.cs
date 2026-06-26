using Order.Application.OrdersCQRS.Commands.DeleteOrder;

namespace Order.API.Endpoints;

public record DeleteOrderResponse(bool IsSuccess);
public class DeleteOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/orders/{id}", async(Guid id,ISender sender) => {

            var result = await sender.Send(new DeleteOrderCommand(id));

            var response = result.Adapt<DeleteOrderResponse>();

            return Results.Ok(response);
        })
        // Admin-only: deleting an order is destructive; narrowest role.
        .RequireAuthorization(policy => policy.RequireRole("super-admin"))
        .WithName("DeleteOrder")
        .Produces<DeleteOrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Delete Order")
        .WithDescription("Delete Order");

    }
}
