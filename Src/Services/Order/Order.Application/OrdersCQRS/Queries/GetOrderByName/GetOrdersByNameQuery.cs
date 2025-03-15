
namespace Order.Application.OrdersCQRS.Queries.GetOrderByName;

public record GetOrdersByNameQuery(string OrderName) : IQuery<GetOrderByNameResult>;
public record GetOrderByNameResult(IEnumerable<OrderDto> Orders);
