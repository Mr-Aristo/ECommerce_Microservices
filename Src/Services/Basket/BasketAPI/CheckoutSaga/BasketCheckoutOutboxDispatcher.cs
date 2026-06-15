using BuildingBlockMessaging.Events;
using MassTransit;

namespace BasketAPI.CheckoutSaga;

// OUTBOX/SAGA: background outbox dispatcher publishes pending checkout events asynchronously.
public class BasketCheckoutOutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<BasketCheckoutOutboxDispatcher> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxRetryCount = 10;
    // Keep published rows briefly for diagnostics, then purge so the outbox table stays small.
    private static readonly TimeSpan PublishedRetention = TimeSpan.FromHours(1);

    /// <summary>
    /// This function works till app is closed
    /// "stoppingToken" gets true while app is closing, so we can use it to stop the function gracefully.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingMessagesAsync(stoppingToken);
                await CleanupPublishedMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while dispatching basket checkout outbox messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

    private async Task DispatchPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await using var session = store.LightweightSession();
        // OUTBOX/SAGA: pull only pending records from local outbox.
        var pendingMessages = await session.Query<BasketCheckoutOutboxMessage>()
            .Where(x => x.Status == CheckoutOutboxStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        foreach (var message in pendingMessages)
        {
            try
            {
                // OUTBOX/SAGA: actual broker publish happens here.
                await publishEndpoint.Publish(message.Payload, cancellationToken);

                message.Status = CheckoutOutboxStatus.Published;
                message.PublishedAt = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.LastError = ex.Message;

                if (message.RetryCount >= MaxRetryCount)
                {
                    message.Status = CheckoutOutboxStatus.Failed;
                }

                logger.LogWarning(
                    ex,
                    "Failed to publish checkout outbox message {MessageId} for checkout {CheckoutId}. RetryCount: {RetryCount}.",
                    message.Id,
                    message.CheckoutId,
                    message.RetryCount);
            }

            session.Store(message);
        }

        await session.SaveChangesAsync(cancellationToken);
    }

    // Purge already-published outbox rows older than the retention window.
    private async Task CleanupPublishedMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

        await using var session = store.LightweightSession();
        var cutoff = DateTime.UtcNow - PublishedRetention;
        session.DeleteWhere<BasketCheckoutOutboxMessage>(
            x => x.Status == CheckoutOutboxStatus.Published && x.PublishedAt != null && x.PublishedAt < cutoff);
        await session.SaveChangesAsync(cancellationToken);
    }
}
