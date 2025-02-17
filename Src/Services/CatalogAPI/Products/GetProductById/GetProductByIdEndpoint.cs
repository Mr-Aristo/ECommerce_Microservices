
using OpenTelemetry.Trace;

namespace CatalogAPI.Products.GetProductById;

public record GetProductByIdResponse(Product Product);//This product parameter name must be the same ib handler for Mapster inder to map it 
public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));

            var response = result.Adapt<GetProductByIdResponse>();

            return Results.Ok(response);

        })
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get product by id.")
            .WithDescription("Get product by id.");
    }
}
