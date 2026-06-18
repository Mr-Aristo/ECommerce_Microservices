namespace CatalogAPI.Products.GetProductsByIds;

public record GetProductsByIdsQuery(Guid[] Ids) : IQuery<GetProductsByIdsResult>;
public record GetProductsByIdsResult(IEnumerable<Product> Products);

internal class GetProductsByIdsQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductsByIdsQuery, GetProductsByIdsResult>
{
    public async Task<GetProductsByIdsResult> Handle(GetProductsByIdsQuery query, CancellationToken cancellationToken)
    {
        if (query.Ids is null || query.Ids.Length == 0)
            return new GetProductsByIdsResult(Enumerable.Empty<Product>());

        var products = await session.Query<Product>()
            .Where(p => query.Ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        return new GetProductsByIdsResult(products);
    }
}
