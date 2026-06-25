
namespace Order.Application.OrdersCQRS.Queries.GetOrderByName;

public class GetOrdersByNameHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersByNameQuery, GetOrderByNameResult>
{
    public async Task<GetOrderByNameResult> Handle(GetOrdersByNameQuery request, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderName.Value.Contains(request.OrderName))
            // Order by the underlying string; OrderBy on the OrderName value object isn't translatable (throws 500).
            .OrderBy(o => o.OrderName.Value)
            .ToListAsync(cancellationToken);

        return new GetOrderByNameResult(orders.ToOrderDtoList());
    }
}
