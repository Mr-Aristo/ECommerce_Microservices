using Order.Application.Security;

namespace Order.Application.OrdersCQRS.Commands.CreateOrder;
public class CreateOrderHandler(IApplicationDbContext dbcontext) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(command.Order.Id);
        var existingOrder = await dbcontext.Orders.FindAsync([orderId], cancellationToken);
        if (existingOrder is not null)
        {
            return new CreateOrderResult(existingOrder.Id.Value);
        }

        await EnsureReferenceDataExistsAsync(command.Order, cancellationToken);

        var order = CreateNewOrder(command.Order);
        dbcontext.Orders.Add(order);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id.Value);
    }

    private Orders CreateNewOrder(OrderDto orderDto)
    {
        var shippingAddress = Address.Of(
            orderDto.ShippingAddress.FirstName,
            orderDto.ShippingAddress.LastName,
            orderDto.ShippingAddress.EmailAddress,
            orderDto.ShippingAddress.AddressLine,
            orderDto.ShippingAddress.Country, 
            orderDto.ShippingAddress.State,
            orderDto.ShippingAddress.ZipCode);

        var billingAddress = Address.Of(
            orderDto.BillingAddress.FirstName, 
            orderDto.BillingAddress.LastName, 
            orderDto.BillingAddress.EmailAddress,
            orderDto.BillingAddress.AddressLine, 
            orderDto.BillingAddress.Country,
            orderDto.BillingAddress.State,
            orderDto.BillingAddress.ZipCode);

        var newOrder = Orders.Create(
                id: OrderId.Of(orderDto.Id),
                customerId: CustomerId.Of(orderDto.CustomerId),
                orderName: OrderName.Of(orderDto.OrderName),
                shippingAddress: shippingAddress,
                billingAddress: billingAddress,
                payment: Payment.Of(
                    orderDto.Payment.CardName,
                    PaymentDataSanitizer.NormalizePaymentToken(orderDto.Payment.CardNumber),
                    orderDto.Payment.Expiration,
                    PaymentDataSanitizer.RedactCvv(),
                    orderDto.Payment.PaymentMethod)
                );

        foreach (var orderItemDto in orderDto.OrderItems)
        {
            newOrder.Add(ProductId.Of(orderItemDto.ProductId), orderItemDto.Quantity, orderItemDto.Price);
        }

        return newOrder;
    }

    private async Task EnsureReferenceDataExistsAsync(OrderDto orderDto, CancellationToken cancellationToken)
    {
        var hasChanges = false;

        var customerId = CustomerId.Of(orderDto.CustomerId);
        var customerExists = await dbcontext.Customers
            .AnyAsync(c => c.Id == customerId, cancellationToken);

        if (!customerExists)
        {
            dbcontext.Customers.Add(
                Customer.Create(
                    customerId,
                    BuildCustomerName(orderDto),
                    SelectCustomerEmail(orderDto)));
            hasChanges = true;
        }

        foreach (var item in orderDto.OrderItems
                     .GroupBy(i => i.ProductId)
                     .Select(g => g.First()))
        {
            var productId = ProductId.Of(item.ProductId);
            var productExists = await dbcontext.Products
                .AnyAsync(p => p.Id == productId, cancellationToken);

            if (productExists)
            {
                continue;
            }

            dbcontext.Products.Add(
                Product.Create(
                    productId,
                    BuildFallbackProductName(item.ProductId),
                    item.Price > 0 ? item.Price : 0.01m));
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbcontext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string BuildCustomerName(OrderDto orderDto)
    {
        var firstName = orderDto.ShippingAddress.FirstName?.Trim();
        var lastName = orderDto.ShippingAddress.LastName?.Trim();

        var fullName = string.Join(
            " ",
            new[] { firstName, lastName }
                .Where(part => !string.IsNullOrWhiteSpace(part)));

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        return !string.IsNullOrWhiteSpace(orderDto.OrderName)
            ? orderDto.OrderName
            : "guest";
    }

    private static string SelectCustomerEmail(OrderDto orderDto)
    {
        if (!string.IsNullOrWhiteSpace(orderDto.ShippingAddress.EmailAddress))
        {
            return orderDto.ShippingAddress.EmailAddress;
        }

        return orderDto.BillingAddress.EmailAddress;
    }

    private static string BuildFallbackProductName(Guid productId)
        => $"CatalogProduct-{productId:N}";
}
