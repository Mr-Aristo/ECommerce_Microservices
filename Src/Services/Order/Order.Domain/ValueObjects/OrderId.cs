namespace Order.Domain.ValueObjects;

public record OrderId
{
    //Strongly types value id
    public Guid Value { get; }
    private OrderId(Guid value) => Value = value;
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