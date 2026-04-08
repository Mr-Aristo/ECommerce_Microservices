namespace BasketAPI.DTOs;

public class BasketCheckoutDto
{
    // User and Basket Information
    public string UserName { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public decimal TotalPrice { get; set; } = default!;

    // Shipping and BillingAddress
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;

    // Tokenized Payment Information
    public string CardName { get; set; } = default!;
    public string PaymentToken { get; set; } = default!;
    public string PaymentReference { get; set; } = default!;
    public string CardLast4 { get; set; } = default!;
    public string CardBrand { get; set; } = default!;
    public int PaymentMethod { get; set; } = default!;
}
