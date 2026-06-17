using Order.Application.OrdersCQRS.Queries.GetOrderByCustomer;

namespace Order.API.Endpoints;

public record MyOrdersResponse(IEnumerable<OrderDto> Orders);

// Customer-facing order history; the caller only ever sees their own orders (CustomerId == sub).
public class MyOrders : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/me/orders", async (ClaimsPrincipal user, ISender sender) =>
        {
            var customerId = Guid.TryParse(user.GetUserId(), out var id) ? id : Guid.Empty;
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));
            return Results.Ok(new MyOrdersResponse(result.Orders));
        })
        .RequireAuthorization()
        .WithName("GetMyOrders")
        .Produces<MyOrdersResponse>(StatusCodes.Status200OK)
        .WithSummary("Get my orders");
    }
}
