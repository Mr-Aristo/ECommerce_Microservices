namespace Order.Domain.ValueObjects;

public record OrderItemId
{
    public Guid Value { get; }

    private OrderItemId(Guid value) => Value = value;

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