using BuildingBlock.Exceptions;

namespace BasketAPI.Exceptions;

/// <summary>
/// The BasketNotFoundException class is a custom exception that inherits from NotFoundExceptions.
/// It is thrown when a shopping basket cannot be found for a given user name. The constructor takes 
/// the user name as a parameter and passes it to the base NotFoundExceptions class along with the entity 
/// name "Basket". This allows for consistent error handling and messaging when a basket is not found in the system.
/// </summary>
public class BasketNotFoundException : NotFoundExceptions
{
    public BasketNotFoundException(string userName) : base("Basket",userName)
    {
    }
}
