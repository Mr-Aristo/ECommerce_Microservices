
namespace Order.Domain.ValueObjects;

/// <summary>
/// Value object representing the unique identifier for a <c>Customer</c>.
/// Enforces non-empty <see cref="Guid"/> values through the factory method.
/// </summary>
public record CustomerId
{
    /// <summary>
    /// The strongly-typed unique identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Private constructor to ensure creation only through the <see cref="Of"/> method.
    /// </summary>
    /// <param name="value">A non-empty <see cref="Guid"/> representing the customer ID.</param>
    private CustomerId(Guid value) => Value = value;

    /// <summary>
    /// Factory method to create a valid <see cref="CustomerId"/> instance.
    /// Throws <see cref="DomainException"/> if the provided <see cref="Guid"/> is empty.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> value to wrap.</param>
    /// <returns>A new <see cref="CustomerId"/> instance with the specified value.</returns>
    /// <exception cref="DomainException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static CustomerId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        if (value == Guid.Empty)
        {
            throw new DomainException("Customer Id cannot be empty"); 
        }

        return new CustomerId(value);
    }
}
