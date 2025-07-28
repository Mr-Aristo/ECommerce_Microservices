namespace Order.Domain.ValueObjects;

/// <summary>
/// Value object representing a strongly-typed identifier for an <c>OrderItem</c>.
/// Guards against using default or invalid <see cref="Guid"/> values,
/// promoting clarity and safety by avoiding primitive obsession.
/// </summary>
/// <remarks>
/// Strongly-typed identifiers improve domain modeling by reducing the chance of mixing unrelated IDs (e.g., using a ProductId where an OrderItemId is expected).
/// </remarks>
public record OrderItemId
{
    public Guid Value { get; }

    private OrderItemId(Guid value) => Value = value;

    /// <summary>
    /// Factory method to create a valid <see cref="OrderItemId"/> instance.
    /// </summary>
    /// <param name="value">The unique identifier for the order item.</param>
    /// <returns>A new instance of <see cref="OrderItemId"/>.</returns>
    /// <exception cref="DomainException">Thrown if the provided <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static OrderItemId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value == Guid.Empty)
        {
            throw new DomainException("OrderItemId cannot be empty.");
        }
        return new OrderItemId(value);
    }
}

/*
 *Of metodu, yazılım dünyasında genellikle factory method (fabrika metodu) 
 *olarak kullanılır ve belirli bir türde nesne oluşturma işlevi görür.
 *Bu, özellikle Value Object veya belirli doğrulama kurallarına sahip
 *nesnelerin oluşturulmasında tercih edilir. Of metodu, nesne oluşturma
 *sürecini daha kontrollü ve anlaşılır hale getirir. 
 * 
 */