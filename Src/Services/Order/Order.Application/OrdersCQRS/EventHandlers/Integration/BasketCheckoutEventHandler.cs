
using BuildingBlockMessaging.Events;
using MassTransit;
using Order.Application.OrdersCQRS.Commands.CreateOrder;
using Order.Application.Security;
namespace Order.Application.OrdersCQRS.EventHandlers.Integration;

/// <summary>
/// The BasketCheckoutEventHandler class is responsible for handling the BasketCheckoutEvent integration event.
/// When a BasketCheckoutEvent is received, this handler processes the event by mapping it to a CreateOrderCommand 
/// and sending it to the appropriate command handler using the ISender interface. 
/// It also logs the handling of the integration event for monitoring and debugging purposes. 
/// The MapToCreateOrderCommand method is used to convert the incoming BasketCheckoutEvent data into a CreateOrderCommand, which contains all
/// the necessary information to create a new order in the system based on the data received from the basket checkout process.
/// </summary>
public class BasketCheckoutEventHandler(ISender sender, ILogger<BasketCheckoutEventHandler> logger) : IConsumer<BasketCheckoutEvent>
{
    /// <summary>
    /// Consumes the BasketCheckoutEvent, processes it by mapping to a CreateOrderCommand, and sends the command for further handling.
    /// </summary>
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        logger.LogInformation("Integration event handled:{IntegrationEvent}", context.Message.GetType().Name);

        var command = MapToCreateOrderCommand(context.Message);
        await sender.Send(command);
    }

    private CreateOrderCommand MapToCreateOrderCommand(BasketCheckoutEvent message)
    {
        if (message.Items is null || message.Items.Count == 0)
        {
            throw new ValidationException("BasketCheckoutEvent items cannot be null or empty.");
        }

        // Create full order with incoming event data
        var addressDto = new AddressDto(
            message.FirstName,
            message.LastName,
            message.EmailAddress,
            message.AddressLine,
            message.Country,
            message.State,
            message.ZipCode);

        var paymentDto = new PaymentDto(
            message.CardName,
            PaymentDataSanitizer.MaskCardNumber(message.CardNumber),
            message.Expiration,
            PaymentDataSanitizer.RedactCvv(),
            message.PaymentMethod);

        var orderId = Guid.NewGuid();
        var orderItems = message.Items
            .Select(item => new OrderItemDto(orderId, item.ProductId, item.Quantity, item.Price))
            .ToList();


        var orderDto = new OrderDto(
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: message.UserName,
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            Status: Order.Domain.Enums.OrderStatus.Pending,
            OrderItems: orderItems);

        return new CreateOrderCommand(orderDto);
    }
}
