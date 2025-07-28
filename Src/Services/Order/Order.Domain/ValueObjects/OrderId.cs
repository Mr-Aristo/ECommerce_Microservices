namespace Order.Domain.ValueObjects;

/// <summary>
/// Value object representing a strongly-typed identifier for an <c>Order</c>.
/// Enforces non-empty <see cref="Guid"/> values and helps avoid primitive obsession.
/// </summary>
/// <remarks>
/// Using strongly-typed IDs improves code readability, reduces ambiguity,
/// and prevents common mistakes caused by misusing primitive types (e.g., passing a ProductId where an OrderId is expected).
/// </remarks>
public record OrderId
{
    //Strongly types value id

    // <summary>
    /// The encapsulated <see cref="Guid"/> value representing the order's unique identity.
    /// </summary>
    public Guid Value { get; }


    /// <summary>
    /// Private constructor to ensure instantiation only through the <see cref="Of"/> factory method.
    /// </summary>
    /// <param name="value">A valid, non-empty <see cref="Guid"/>.</param>
    private OrderId(Guid value) => Value = value;

    /// <summary>
    /// Factory method to create a valid <see cref="OrderId"/> instance.
    /// Ensures the provided <see cref="Guid"/> is not <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The unique identifier for the order.</param>
    /// <returns>A new <see cref="OrderId"/> instance.</returns>
    /// <exception cref="DomainException">Thrown if <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static OrderId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("OrderId cannot be empty.");
        }

        return new OrderId(value);
    }
}
//Avoiding primitive obsession (it is a anti-pattern). As a id like giud, int... 
//Strongly typed Ids reduces mistakes and mistunderstading and decreses readabilty of the code.