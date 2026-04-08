namespace BasketAPI.Models;

// OUTBOX/SAGA: basket lifecycle state used during checkout orchestration.
public enum BasketStatus
{
    Active = 0,
    CheckoutPending = 1
}
