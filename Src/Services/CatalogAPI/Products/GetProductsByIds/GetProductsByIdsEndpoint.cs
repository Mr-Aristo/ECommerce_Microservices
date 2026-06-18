using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Products.GetProductsByIds;

public record GetProductsByIdsResponse(IEnumerable<Product> Products);

public class GetProductsByIdsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Batch read used by other services (e.g. Users favorites enrichment). Repeat the key: ?ids=..&ids=..
        app.MapGet("/products/by-ids", async ([FromQuery] Guid[] ids, ISender sender) =>
        {
            var result = await sender.Send(new GetProductsByIdsQuery(ids));

            return Results.Ok(new GetProductsByIdsResponse(result.Products));
        })
        .WithName("GetProductsByIds")
        .Produces<GetProductsByIdsResponse>(StatusCodes.Status200OK)
        .WithSummary("Get products by ids")
        .WithDescription("Batch fetch products by a set of ids");
    }
}
