using Order.Application.OrdersCQRS.Queries.GetOrderByCustomer;

namespace Order.API.Endpoints;

public record GetOrdersByCustomerResponse(IEnumerable<OrderDto> Orders);
public class GetOrdersByCustomer : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/by-customer/{customerId:guid}", async (Guid customerId , ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));

            var response = result.Adapt<GetOrdersByCustomerResponse>();

            return Results.Ok(response);
        })
        // Admin-only: arbitrary customerId lookup. Customers use GET /me/orders instead.
        .RequireAuthorization(policy => policy.RequireRole("fulfillment-manager", "support-agent", "super-admin"))
        .WithName("GetOrdersByCustomer")
        .Produces<GetOrdersByCustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Orders By Customer")
        .WithDescription("Get Orders By Customer");
    }
}
