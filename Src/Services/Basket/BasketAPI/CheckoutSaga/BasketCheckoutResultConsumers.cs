using BuildingBlockMessaging.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace BasketAPI.CheckoutSaga;

// OUTBOX/SAGA: success event completes saga and clears basket.
public class BasketCheckoutSucceededEventConsumer(
    IDocumentSession session,
    IDistributedCache cache,
    ILogger<BasketCheckoutSucceededEventConsumer> logger) : IConsumer<BasketCheckoutSucceededEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutSucceededEvent> context)
    {
        var message = context.Message;
        var basket = await session.LoadAsync<ShoppingCard>(message.UserName, context.CancellationToken);

        if (basket is null)
        {
            logger.LogInformation(
                "Received checkout success for user {UserName} and checkout {CheckoutId}, but basket was not found.",
                message.UserName,
                message.CheckoutId);
            return;
        }

        if (basket.PendingCheckoutId != message.CheckoutId)
        {
            logger.LogInformation(
                "Ignoring checkout success for user {UserName}. Pending checkout id mismatch. Expected {ExpectedCheckoutId}, got {ActualCheckoutId}.",
                message.UserName,
                basket.PendingCheckoutId,
                message.CheckoutId);
            return;
        }

        // OUTBOX/SAGA: success compensation path -> finalize by deleting pending basket.
        session.Delete<ShoppingCard>(message.UserName);
        await session.SaveChangesAsync(context.CancellationToken);
        await cache.RemoveAsync(message.UserName, context.CancellationToken);
    }
}

// OUTBOX/SAGA: failure event compensates saga and reactivates basket for retry.
public class BasketCheckoutFailedEventConsumer(
    IDocumentSession session,
    IDistributedCache cache,
    ILogger<BasketCheckoutFailedEventConsumer> logger) : IConsumer<BasketCheckoutFailedEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutFailedEvent> context)
    {
        var message = context.Message;
        var basket = await session.LoadAsync<ShoppingCard>(message.UserName, context.CancellationToken);

        if (basket is null)
        {
            logger.LogInformation(
                "Received checkout failure for user {UserName} and checkout {CheckoutId}, but basket was not found.",
                message.UserName,
                message.CheckoutId);
            return;
        }

        if (basket.PendingCheckoutId != message.CheckoutId)
        {
            logger.LogInformation(
                "Ignoring checkout failure for user {UserName}. Pending checkout id mismatch. Expected {ExpectedCheckoutId}, got {ActualCheckoutId}.",
                message.UserName,
                basket.PendingCheckoutId,
                message.CheckoutId);
            return;
        }

        basket.Status = BasketStatus.Active;
        basket.PendingCheckoutId = null;

        // OUTBOX/SAGA: failure compensation path -> return basket back to active state.
        session.Store(basket);
        await session.SaveChangesAsync(context.CancellationToken);

        await cache.SetStringAsync(
            basket.UserName,
            JsonSerializer.Serialize(basket),
            context.CancellationToken);

        logger.LogWarning(
            "Checkout failed for user {UserName} and checkout {CheckoutId}. Basket was reactivated. Reason: {Reason}",
            message.UserName,
            message.CheckoutId,
            message.Reason);
    }
}
