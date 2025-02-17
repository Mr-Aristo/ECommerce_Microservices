namespace CatalogAPI.Products.CreateProduct;


public record CreateProductRequest(Guid Id,string Name,List<string> Category,string Description,string ImageFile,decimal Price);
public record CreateProductResponse(Guid Id);


//Carter library installed for endpoints.
public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/product-create",
            async (CreateProductRequest request, ISender sender) =>//ISender from mediator to send command
        {
            //mapster for mapping and to other class. //CreateProductCommand From handler
            var command = request.Adapt<CreateProductCommand>();// Adaptation of handler command

            var result = await sender.Send(command);
            var response = result.Adapt<CreateProductResponse>();// Adabtation of handler response

            return Results.Created($"/products/{response.Id}", response);
        })
            //CarterExtentions for endpoint
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Create Product");

    }
}
