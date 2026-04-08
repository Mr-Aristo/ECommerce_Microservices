namespace BasketAPI.Models;

// OUTBOX/SAGA: local outbox record state.
public enum CheckoutOutboxStatus
{
    Pending = 0,
    Published = 1,
    Failed = 2
}
