using BuildingBlock.Exceptions;

namespace CatalogAPI.Exceptions;

/// <summary>
/// The ProductNotFoundException class is a custom exception that inherits from NotFoundExceptions.
/// It is thrown when a product cannot be found for a given product ID. The constructor takes
/// the product ID as a parameter and passes it to the base NotFoundExceptions class along with the entity name "Product".
/// </summary>
public class ProductNotFoundException : NotFoundExceptions
{
    public ProductNotFoundException(Guid Id): base ("Product",Id)
    {
        
    }

}
