using BasketAPI.Basket.CheckoutBasket;
using BasketAPI.Data.Abstracts;
using BasketAPI.DTOs;
using BasketAPI.Models;
using BuildingBlockMessaging.Events;
using MassTransit;

namespace ECommerce_Tests.Basket;

public class CheckoutBasketHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUseUserNameAndPublishEvent_AndDeleteBasket()
    {
        // Arrange
        var repository = new Mock<IBasketRepository>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var sut = new CheckoutBasketHandler(repository.Object, publishEndpoint.Object);

        var dto = new BasketCheckoutDto
        {
            UserName = "checkout-user",
            FirstName = "different-first-name",
            LastName = "Doe",
            EmailAddress = "user@test.com",
            AddressLine = "Somewhere",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "checkout-user",
            CardNumber = "4111111111111111",
            Expiration = "12/30",
            CVV = "123",
            PaymentMethod = 1,
            CustomerId = Guid.NewGuid()
        };

        var basket = new ShoppingCard(dto.UserName)
        {
            Items =
            [
                new ShoppingCardItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Item-1",
                    Quantity = 2,
                    Price = 100,
                    Color = "Blue"
                }
            ]
        };

        repository
            .Setup(r => r.GetBasket(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        repository
            .Setup(r => r.DeleteBasket(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        publishEndpoint
            .Setup(p => p.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.Handle(new CheckoutBasketCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repository.Verify(r => r.GetBasket(dto.UserName, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(r => r.GetBasket(dto.FirstName, It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(r => r.DeleteBasket(dto.UserName, It.IsAny<CancellationToken>()), Times.Once);
        publishEndpoint.Verify(p => p.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
