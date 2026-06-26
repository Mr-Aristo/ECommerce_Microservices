using BasketAPI.Basket.CheckoutBasket;
using BasketAPI.DTOs;
using BasketAPI.Exceptions;
using BasketAPI.Models;
using BuildingBlock.Exceptions;
using BuildingBlockMessaging.Events;
using Marten;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerce_Tests.Basket;

public class CheckoutBasketHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMarkBasketPending_AndStoreOutboxMessage()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var sut = new CheckoutBasketHandler(session.Object, cache.Object);

        var dto = new BasketCheckoutDto
        {
            UserName = "checkout-user",
            FirstName = "Jane",
            LastName = "Doe",
            EmailAddress = "user@test.com",
            AddressLine = "Somewhere",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "Jane Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            CustomerId = Guid.NewGuid()
        };

        var basket = new ShoppingCard(dto.UserName)
        {
            Status = BasketStatus.Active,
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

        session
            .Setup(s => s.LoadAsync<ShoppingCard>(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        session
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.Handle(new CheckoutBasketCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BasketStatus.CheckoutPending, basket.Status);
        Assert.NotNull(basket.PendingCheckoutId);

        session.Verify(
            s => s.Store(It.Is<ShoppingCard>(b =>
                b.UserName == dto.UserName &&
                b.Status == BasketStatus.CheckoutPending &&
                b.PendingCheckoutId.HasValue)),
            Times.Once);

        session.Verify(
            s => s.Store(It.Is<BasketCheckoutOutboxMessage>(m =>
                m.UserName == dto.UserName &&
                m.CheckoutId == basket.PendingCheckoutId &&
                m.Status == CheckoutOutboxStatus.Pending &&
                m.Payload.UserName == dto.UserName &&
                m.Payload.PaymentToken == dto.PaymentToken &&
                m.Payload.PaymentReference == dto.PaymentReference &&
                m.Payload.CardLast4 == dto.CardLast4 &&
                m.Payload.CardBrand == dto.CardBrand &&
                m.Payload.Items.Count == 1)),
            Times.Once);

        session.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(
            c => c.SetAsync(
                dto.UserName,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenBasketIsEmpty()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var sut = new CheckoutBasketHandler(session.Object, cache.Object);

        var dto = new BasketCheckoutDto
        {
            UserName = "checkout-user",
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "user@test.com",
            AddressLine = "Somewhere",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "John Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            CustomerId = Guid.NewGuid()
        };

        session
            .Setup(s => s.LoadAsync<ShoppingCard>(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShoppingCard(dto.UserName) { Items = [] });

        // Act + Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Handle(new CheckoutBasketCommand(dto), CancellationToken.None));

        session.Verify(s => s.Store(It.IsAny<ShoppingCard>()), Times.Never);
        session.Verify(s => s.Store(It.IsAny<BasketCheckoutOutboxMessage>()), Times.Never);
        session.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenCheckoutAlreadyPending()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var sut = new CheckoutBasketHandler(session.Object, cache.Object);

        var dto = new BasketCheckoutDto
        {
            UserName = "checkout-user",
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "user@test.com",
            AddressLine = "Somewhere",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "John Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            CustomerId = Guid.NewGuid()
        };

        session
            .Setup(s => s.LoadAsync<ShoppingCard>(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShoppingCard(dto.UserName)
            {
                Status = BasketStatus.CheckoutPending,
                PendingCheckoutId = Guid.NewGuid(),
                Items =
                [
                    new ShoppingCardItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Item-1",
                        Quantity = 1,
                        Price = 100
                    }
                ]
            });

        // Act + Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Handle(new CheckoutBasketCommand(dto), CancellationToken.None));

        session.Verify(s => s.Store(It.IsAny<ShoppingCard>()), Times.Never);
        session.Verify(s => s.Store(It.IsAny<BasketCheckoutOutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBasketNotFound_WhenBasketDoesNotExist()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var sut = new CheckoutBasketHandler(session.Object, cache.Object);

        var dto = new BasketCheckoutDto
        {
            UserName = "checkout-user",
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "user@test.com",
            AddressLine = "Somewhere",
            Country = "TR",
            State = "IST",
            ZipCode = "34000",
            CardName = "John Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            CustomerId = Guid.NewGuid()
        };

        session
            .Setup(s => s.LoadAsync<ShoppingCard>(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShoppingCard?)null);

        // Act + Assert
        await Assert.ThrowsAsync<BasketNotFoundException>(() =>
            sut.Handle(new CheckoutBasketCommand(dto), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReplayResult_WhenIdempotencyKeyAlreadyProcessed()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var sut = new CheckoutBasketHandler(session.Object, cache.Object);

        const string key = "idem-key-123";
        var dto = new BasketCheckoutDto { UserName = "checkout-user", CustomerId = Guid.NewGuid() };
        var scopedKey = $"{dto.UserName}:{key}";   // key is scoped to the authenticated user

        session
            .Setup(s => s.LoadAsync<IdempotencyRecord>(scopedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdempotencyRecord
            {
                Key = scopedKey,
                CheckoutId = Guid.NewGuid(),
                UserName = dto.UserName,
                IsSuccess = true
            });

        // Act
        var result = await sut.Handle(new CheckoutBasketCommand(dto, key), CancellationToken.None);

        // Assert — replays the prior result; no second checkout is started.
        Assert.True(result.IsSuccess);
        session.Verify(s => s.LoadAsync<ShoppingCard>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        session.Verify(s => s.Store(It.IsAny<ShoppingCard>()), Times.Never);
        session.Verify(s => s.Store(It.IsAny<BasketCheckoutOutboxMessage>()), Times.Never);
        session.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldStoreIdempotencyRecord_WhenKeyProvidedAndNew()
    {
        // Arrange
        var session = new Mock<IDocumentSession>();
        var cache = new Mock<IDistributedCache>();
        var sut = new CheckoutBasketHandler(session.Object, cache.Object);

        const string key = "idem-key-new";
        var dto = new BasketCheckoutDto
        {
            UserName = "checkout-user",
            EmailAddress = "user@test.com",
            CardName = "Jane Doe",
            PaymentToken = "tok_abc_123",
            PaymentReference = "pi_123",
            CardLast4 = "1111",
            CardBrand = "VISA",
            PaymentMethod = 1,
            CustomerId = Guid.NewGuid()
        };
        var scopedKey = $"{dto.UserName}:{key}";   // key is scoped to the authenticated user

        var basket = new ShoppingCard(dto.UserName)
        {
            Status = BasketStatus.Active,
            Items = [new ShoppingCardItem { ProductId = Guid.NewGuid(), ProductName = "Item-1", Quantity = 1, Price = 100 }]
        };

        session
            .Setup(s => s.LoadAsync<IdempotencyRecord>(scopedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);
        session
            .Setup(s => s.LoadAsync<ShoppingCard>(dto.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        session
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.Handle(new CheckoutBasketCommand(dto, key), CancellationToken.None);

        // Assert — first time through, the key is recorded in the same transaction.
        Assert.True(result.IsSuccess);
        session.Verify(
            s => s.Store(It.Is<IdempotencyRecord>(r => r.Key == scopedKey && r.UserName == dto.UserName && r.IsSuccess)),
            Times.Once);
        session.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
