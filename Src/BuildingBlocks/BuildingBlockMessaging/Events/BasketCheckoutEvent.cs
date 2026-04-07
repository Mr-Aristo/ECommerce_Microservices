namespace BuildingBlockMessaging.Events;

public record BasketCheckoutEvent : IntegrationEvent
{
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

    // Payment
    public string CardName { get; set; } = default!;
    // CardNumber is expected to be masked before publish (example: **** **** **** 1111).
    public string CardNumber { get; set; } = default!;
    public string Expiration { get; set; } = default!;
    // CVV is expected to be redacted before publish.
    public string CVV { get; set; } = default!;
    public int PaymentMethod { get; set; } = default!;
}
