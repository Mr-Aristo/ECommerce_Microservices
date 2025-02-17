using BuildingBlock.Exceptions;

namespace CatalogAPI.Exceptions;

public class ProductNotFoundException : NotFoundExceptions
{
    public ProductNotFoundException(Guid Id): base ("Product",Id)
    {
        
    }

}
