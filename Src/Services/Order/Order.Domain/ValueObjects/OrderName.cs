namespace Order.Domain.ValueObjects;

/// <summary>
/// Value object representing the name or title of an order.
/// Encapsulates validation logic to ensure non-empty values.
/// </summary>
public record OrderName
{
    private const int DefaultLenght = 10;

    /// <summary>
    /// The string value of the order name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor to enforce controlled creation through the <see cref="Of"/> factory method.
    /// </summary>
    /// <param name="value">The order name string.</param>
    private OrderName(string value) => Value = value;

    /// <summary>
    /// Factory method to create a valid <see cref="OrderName"/> instance.
    /// Throws an exception if the value is null or whitespace.
    /// </summary>
    /// <param name="value">The order name to encapsulate.</param>
    /// <returns>A new instance of <see cref="OrderName"/>.</returns>
    public static OrderName Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new OrderName(value);

    }

}
