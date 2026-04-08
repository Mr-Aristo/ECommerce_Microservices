using BuildingBlockMessaging.Events;
using FluentValidation;
using MassTransit;
using Order.Application.OrdersCQRS.Commands.CreateOrder;
using Order.Application.OrdersCQRS.EventHandlers.Integration;
using Microsoft.Extensions.Logging;

namespace ECommerce_Tests.Order;

public class BasketCheckoutEventHandlerTests
{
    [Fact]
    public async Task Consume_ShouldMapOrderItems_FromIncomingEventItems()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var logger = new Mock<ILogger<BasketCheckoutEventHandler>>();
        var sut = new BasketCheckoutEventHandler(sender.Object, logger.Object);

        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var message = new BasketCheckoutEvent
        {
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
            CardNumber = "4111111111111111",
            Expiration = "12/30",
            CVV = "123",
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

        CreateOrderCommand? capturedCommand = null;
        sender
            .Setup(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateOrderCommand, CancellationToken>((request, _) => capturedCommand = request)
            .ReturnsAsync(new CreateOrderResult(Guid.NewGuid()));

        // Act
        await sut.Consume(context.Object);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal(2, capturedCommand!.Order.OrderItems.Count);
        Assert.Contains(capturedCommand.Order.OrderItems, i => i.ProductId == item1Id && i.Quantity == 2 && i.Price == 100);
        Assert.Contains(capturedCommand.Order.OrderItems, i => i.ProductId == item2Id && i.Quantity == 1 && i.Price == 300);
        Assert.Equal("**** **** **** 1111", capturedCommand.Order.Payment.CardNumber);
        Assert.Equal("***", capturedCommand.Order.Payment.Cvv);
    }

    [Fact]
    public async Task Consume_ShouldThrowValidationException_WhenEventItemsAreNull()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var logger = new Mock<ILogger<BasketCheckoutEventHandler>>();
        var sut = new BasketCheckoutEventHandler(sender.Object, logger.Object);

        var message = new BasketCheckoutEvent
        {
            UserName = "legacy-user",
            CustomerId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            EmailAddress = "jane@doe.com",
            AddressLine = "Main Street",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "Jane Doe",
            CardNumber = "4111111111111111",
            Expiration = "12/30",
            CVV = "123",
            PaymentMethod = 1,
            Items = null!
        };

        var context = new Mock<ConsumeContext<BasketCheckoutEvent>>();
        context.SetupGet(c => c.Message).Returns(message);

        CreateOrderCommand? capturedCommand = null;
        sender
            .Setup(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateOrderCommand, CancellationToken>((request, _) => capturedCommand = request)
            .ReturnsAsync(new CreateOrderResult(Guid.NewGuid()));

        // Act + Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.Consume(context.Object));
        Assert.Null(capturedCommand);
        sender.Verify(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ShouldThrowValidationException_WhenEventItemsAreEmpty()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var logger = new Mock<ILogger<BasketCheckoutEventHandler>>();
        var sut = new BasketCheckoutEventHandler(sender.Object, logger.Object);

        var message = new BasketCheckoutEvent
        {
            UserName = "new-user",
            CustomerId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            EmailAddress = "jane@doe.com",
            AddressLine = "Main Street",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "Jane Doe",
            CardNumber = "4111111111111111",
            Expiration = "12/30",
            CVV = "123",
            PaymentMethod = 1,
            Items = []
        };

        var context = new Mock<ConsumeContext<BasketCheckoutEvent>>();
        context.SetupGet(c => c.Message).Returns(message);

        // Act + Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.Consume(context.Object));
        sender.Verify(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
