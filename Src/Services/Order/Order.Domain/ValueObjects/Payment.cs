namespace Order.Domain.ValueObjects;

/// <summary>
/// Represents payment information associated with an order.
/// Card PAN/CVV are never persisted; tokenized values are stored instead.
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
    /// Ensures sensitive fields are persisted in a safe form.
    /// </summary>
    /// <param name="cardName">Name on the card.</param>
    /// <param name="cardNumber">Tokenized payment value (legacy field name kept for DB compatibility).</param>
    /// <param name="expiration">Payment reference value (legacy field name kept for DB compatibility).</param>
    /// <param name="cvv">CVV code (never persisted in clear text).</param>
    /// <param name="paymentMethod">Payment method code.</param>
    /// <returns>A new <see cref="Payment"/> instance with validated data.</returns>
    public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);

        var redactedCvv = "***";

        return new Payment(cardName, cardNumber, expiration, redactedCvv, paymentMethod);
    }
}
