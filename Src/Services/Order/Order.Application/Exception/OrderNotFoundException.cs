using BuildingBlock.Exceptions;

namespace Order.Application.Extentions;

public class OrderNotFoundException : NotFoundExceptions
{
    public OrderNotFoundException(Guid id) : base("Order", id)
    {

    }

}
