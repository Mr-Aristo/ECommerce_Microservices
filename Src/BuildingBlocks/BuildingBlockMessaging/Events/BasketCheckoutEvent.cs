namespace BuildingBlockMessaging.Events;

// OUTBOX/SAGA: checkout integration event persisted in Basket outbox and published to Order.
public record BasketCheckoutEvent : IntegrationEvent
{
    public Guid CheckoutId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public decimal TotalPrice { get; set; } = default!;
    public List<BasketCheckoutItemEvent> Items { get; set; } = [];

    // Shipping and BillingAddress
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;

    // Tokenized payment details (PAN/CVV must not be transported).
    public string CardName { get; set; } = default!;
    public string PaymentToken { get; set; } = default!;
    public string PaymentReference { get; set; } = default!;
    public string CardLast4 { get; set; } = default!;
    public string CardBrand { get; set; } = default!;
    public int PaymentMethod { get; set; } = default!;
}
