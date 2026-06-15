using BuildingBlockMessaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.OrdersCQRS.Commands.CreateOrder;
using Order.Application.OrdersCQRS.EventHandlers.Integration;

namespace ECommerce_Tests.Order;

public class BasketCheckoutEventHandlerTests
{
    [Fact]
    public async Task Consume_ShouldMapOrderItems_AndPublishSuccessEvent()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<BasketCheckoutEventHandler>>();
        var sut = new BasketCheckoutEventHandler(sender.Object, publishEndpoint.Object, logger.Object);

        var checkoutId = Guid.NewGuid();
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var message = new BasketCheckoutEvent
        {
            CheckoutId = checkoutId,
            UserName = "checkout-user",
            CustomerId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            EmailAddress = "jane@doe.com",
            AddressLine = "Main Street",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "Jane Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            Items =
            [
                new BasketCheckoutItemEvent
                {
                    ProductId = item1Id,
                    ProductName = "Product-1",
                    Quantity = 2,
                    Price = 100
                },
                new BasketCheckoutItemEvent
                {
                    ProductId = item2Id,
                    ProductName = "Product-2",
                    Quantity = 1,
                    Price = 300
                }
            ]
        };

        var context = new Mock<ConsumeContext<BasketCheckoutEvent>>();
        context.SetupGet(c => c.Message).Returns(message);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);

        CreateOrderCommand? capturedCommand = null;
        var createdOrderId = Guid.NewGuid();

        sender
            .Setup(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            // ISender.Send resolves to Send<TResponse>(IRequest<TResponse>, ...), so the callback
            // signature must use IRequest<CreateOrderResult>, not CreateOrderCommand.
            .Callback<IRequest<CreateOrderResult>, CancellationToken>((request, _) => capturedCommand = request as CreateOrderCommand)
            .ReturnsAsync(new CreateOrderResult(createdOrderId));

        publishEndpoint
            .Setup(p => p.Publish(It.IsAny<BasketCheckoutSucceededEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.Consume(context.Object);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal(checkoutId, capturedCommand!.Order.Id);
        Assert.Equal(2, capturedCommand!.Order.OrderItems.Count);
        Assert.Contains(capturedCommand.Order.OrderItems, i => i.ProductId == item1Id && i.Quantity == 2 && i.Price == 100);
        Assert.Contains(capturedCommand.Order.OrderItems, i => i.ProductId == item2Id && i.Quantity == 1 && i.Price == 300);
        Assert.Equal("tok_abc_123", capturedCommand.Order.Payment.CardNumber);
        Assert.Equal("pi_123", capturedCommand.Order.Payment.Expiration);

        publishEndpoint.Verify(
            p => p.Publish(
                It.Is<BasketCheckoutSucceededEvent>(e =>
                    e.CheckoutId == checkoutId &&
                    e.UserName == message.UserName &&
                    e.OrderId == createdOrderId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        publishEndpoint.Verify(
            p => p.Publish(It.IsAny<BasketCheckoutFailedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_ShouldPublishFailedEvent_WhenPayloadIsInvalid()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<BasketCheckoutEventHandler>>();
        var sut = new BasketCheckoutEventHandler(sender.Object, publishEndpoint.Object, logger.Object);

        var checkoutId = Guid.NewGuid();
        var message = new BasketCheckoutEvent
        {
            CheckoutId = checkoutId,
            UserName = "checkout-user",
            CustomerId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            EmailAddress = "jane@doe.com",
            AddressLine = "Main Street",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "Jane Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            Items = []
        };

        var context = new Mock<ConsumeContext<BasketCheckoutEvent>>();
        context.SetupGet(c => c.Message).Returns(message);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);

        publishEndpoint
            .Setup(p => p.Publish(It.IsAny<BasketCheckoutFailedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.Consume(context.Object);

        // Assert
        sender.Verify(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        publishEndpoint.Verify(
            p => p.Publish(
                It.Is<BasketCheckoutFailedEvent>(e =>
                    e.CheckoutId == checkoutId &&
                    e.UserName == message.UserName &&
                    !string.IsNullOrWhiteSpace(e.Reason)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
