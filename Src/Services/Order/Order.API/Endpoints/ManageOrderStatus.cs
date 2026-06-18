using Order.Application.OrdersCQRS.Commands.ChangeOrderStatus;

namespace Order.API.Endpoints;

// Admin (fulfillment) order status transitions. Invalid transitions are rejected by the aggregate (400).
public class ManageOrderStatus : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders/{id:guid}")
            .RequireAuthorization(policy => policy.RequireRole("fulfillment-manager", "super-admin"))
            .WithTags("Order status");

        group.MapPost("/confirm", (Guid id, ISender sender) => Transition(sender, id, OrderStatusAction.Confirm));
        group.MapPost("/process", (Guid id, ISender sender) => Transition(sender, id, OrderStatusAction.Process));
        group.MapPost("/ship", (Guid id, ISender sender) => Transition(sender, id, OrderStatusAction.Ship));
        group.MapPost("/deliver", (Guid id, ISender sender) => Transition(sender, id, OrderStatusAction.Deliver));
        group.MapPost("/cancel", (Guid id, ISender sender) => Transition(sender, id, OrderStatusAction.Cancel));
    }

    private static async Task<IResult> Transition(ISender sender, Guid id, OrderStatusAction action)
    {
        var result = await sender.Send(new ChangeOrderStatusCommand(id, action));
        return Results.Ok(result);
    }
}
