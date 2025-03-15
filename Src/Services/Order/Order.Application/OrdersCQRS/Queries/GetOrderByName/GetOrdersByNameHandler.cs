
namespace Order.Application.OrdersCQRS.Queries.GetOrderByName;

public class GetOrdersByNameHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersByNameQuery, GetOrderByNameResult>
{
    public async Task<GetOrderByNameResult> Handle(GetOrdersByNameQuery request, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderName.Value.Contains(request.OrderName))
            .OrderBy(o => o.OrderName)
            .ToListAsync();

        return new GetOrderByNameResult(orders.ToOrderDtoList());
    }
}
