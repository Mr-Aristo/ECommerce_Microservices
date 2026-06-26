namespace BasketAPI.Models;

// Maps a client-supplied Idempotency-Key to the checkout it already started, so a retried
// POST /basket/checkout (network timeout, double-submit) replays the original result instead
// of starting a second checkout. Keyed by the Idempotency-Key string.
public class IdempotencyRecord
{
    public string Key { get; set; } = default!;
    public Guid CheckoutId { get; set; }
    public string UserName { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
