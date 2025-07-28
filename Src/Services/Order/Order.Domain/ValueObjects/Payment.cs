namespace Order.Domain.ValueObjects;

/// <summary>
/// Represents payment information associated with an order.
/// Encapsulates card details and payment method while enforcing validation rules.
/// </summary>
public record Payment
{
    public string? CardName { get; } = default!;
    public string CardNumber { get; } = default!;
    public string Expiration { get; } = default!;
    public string CVV { get; } = default!;
    public int PaymentMethod { get; } = default!;

    /// <summary>
    /// Protected parameterless constructor needed for ORM or serialization.
    /// </summary>
    protected Payment() { }

    /// <summary>
    /// Private constructor to enforce controlled creation via the <see cref="Of"/> factory method.
    /// </summary>
    private Payment(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        CVV = cvv;
        PaymentMethod = paymentMethod;
    }

    /// <summary>
    /// Factory method to create a valid <see cref="Payment"/> instance.
    /// Validates that required fields are not null or empty and CVV length is appropriate.
    /// </summary>
    /// <param name="cardName">Name on the card.</param>
    /// <param name="cardNumber">Card number.</param>
    /// <param name="expiration">Expiration date of the card.</param>
    /// <param name="cvv">CVV code.</param>
    /// <param name="paymentMethod">Payment method code.</param>
    /// <returns>A new <see cref="Payment"/> instance with validated data.</returns>
    public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(cvv);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(cvv.Length, 3);

        return new Payment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }
}
