namespace CatalogAPI.Products.GetProductById;

public record GetProductByIdQuery(Guid id) : IQuery<GetProdcutByIdResult>;

public record GetProdcutByIdResult(Product Product);

internal class GetProductByIdQueryHandler
    (IDocumentSession session, ILogger<GetProductByIdQueryHandler> log)
    : IQueryHandler<GetProductByIdQuery, GetProdcutByIdResult>
{
    public async Task<GetProdcutByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(query.id, cancellationToken);
        if (product is null)
        {
             throw new ProductNotFoundException(query.id);
        }

        return new GetProdcutByIdResult(product);
    }

}
