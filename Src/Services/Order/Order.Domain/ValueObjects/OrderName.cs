namespace Order.Domain.ValueObjects;
public record OrderName
{
    private const int DefaultLenght = 10;

    public string Value { get; }

    private OrderName(string value) => Value = value;

    public static OrderName Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new OrderName(value);

    }

}
