using BuildingBlock.Exceptions;

namespace Order.Application.Extentions;

/// <summary>
/// The OrderNotFoundException class is a custom exception that inherits from NotFoundExceptions.
/// </summary>
public class OrderNotFoundException : NotFoundExceptions
{
    public OrderNotFoundException(Guid id) : base("Order", id)
    {
    }
}
