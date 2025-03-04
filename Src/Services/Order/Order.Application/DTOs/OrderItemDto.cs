namespace Order.Application.DTOs;

public record class OrderItemDto
    (
        Guid OrderId,
        Guid ProductId,
        int Quantity,
        decimal Price
    );


