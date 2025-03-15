namespace Order.Application.OrdersCQRS.Queries.GetOrders;

public record GetOrdersQuery(PaginationRequest PaginationRequest) : IQuery<GetOrderResult>;

public record GetOrderResult(PaginatedResult<OrderDto> Orders);
