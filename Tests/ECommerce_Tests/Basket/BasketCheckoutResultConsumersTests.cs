using BasketAPI.CheckoutSaga;
using BasketAPI.Models;
using BuildingBlockMessaging.Events;
using Marten;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ECommerce_Tests.Basket;

public class BasketCheckoutResultConsumersTests
{
    [Fact]
    public async Task SucceededConsumer_ShouldDeleteBasket_WhenCheckoutIdMatches()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<BasketCheckoutSucceededEventConsumer>>();
        var sut = new BasketCheckoutSucceededEventConsumer(session.Object, cache.Object, logger.Object);

        var checkoutId = Guid.NewGuid();
        var userName = "checkout-user";
        session
            .Setup(s => s.LoadAsync<ShoppingCard>(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShoppingCard(userName)
            {
                Status = BasketStatus.CheckoutPending,
                PendingCheckoutId = checkoutId
            });
        session
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var context = new Mock<ConsumeContext<BasketCheckoutSucceededEvent>>();
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        context.SetupGet(c => c.Message).Returns(new BasketCheckoutSucceededEvent
        {
            CheckoutId = checkoutId,
            UserName = userName,
            OrderId = Guid.NewGuid()
        });

        // Act
        await sut.Consume(context.Object);

        // Assert
        session.Verify(s => s.Delete<ShoppingCard>(userName), Times.Once);
        session.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.RemoveAsync(userName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FailedConsumer_ShouldReactivateBasket_WhenCheckoutIdMatches()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<BasketCheckoutFailedEventConsumer>>();
        var sut = new BasketCheckoutFailedEventConsumer(session.Object, cache.Object, logger.Object);

        var checkoutId = Guid.NewGuid();
        var userName = "checkout-user";
        var basket = new ShoppingCard(userName)
        {
            Status = BasketStatus.CheckoutPending,
            PendingCheckoutId = checkoutId,
            Items =
            [
                new ShoppingCardItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Item",
                    Quantity = 1,
                    Price = 100
                }
            ]
        };

        session
            .Setup(s => s.LoadAsync<ShoppingCard>(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        session
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var context = new Mock<ConsumeContext<BasketCheckoutFailedEvent>>();
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        context.SetupGet(c => c.Message).Returns(new BasketCheckoutFailedEvent
        {
            CheckoutId = checkoutId,
            UserName = userName,
            Reason = "validation failed"
        });

        // Act
        await sut.Consume(context.Object);

        // Assert
        Assert.Equal(BasketStatus.Active, basket.Status);
        Assert.Null(basket.PendingCheckoutId);
        session.Verify(s => s.Store(It.Is<ShoppingCard>(b => b.UserName == userName && b.Status == BasketStatus.Active)), Times.Once);
        cache.Verify(
            c => c.SetAsync(userName, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
