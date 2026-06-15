using BasketAPI.Exceptions;

namespace BasketAPI.Data;

/// <summary>
/// Implements the IBasketRepository interface using Marten as the underlying data access technology. 
/// This repository provides methods to get, store, and delete shopping baskets for users. 
/// It interacts with a document session to perform CRUD operations on the ShoppingCard documents in the database. 
/// The GetBasket method retrieves a basket by user name and throws a BasketNotFoundException if no basket is found. 
/// The StoreBasket method saves a given ShoppingCard to the database, while the DeleteBasket method removes a basket associated 
/// with a specific user name.
/// </summary>
public class BasketRepository(IDocumentSession session) : IBasketRepository
{
    public async Task<ShoppingCard> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var basket = await session.LoadAsync<ShoppingCard>(userName, cancellationToken);
        return basket is null ? throw new BasketNotFoundException(userName) : basket;
    }
    public async Task<ShoppingCard> StoreBasket(ShoppingCard basket, CancellationToken cancellationToken = default)
    {
        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken);
        return basket;
    }
    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        session.Delete<ShoppingCard>(userName);
        await session.SaveChangesAsync(cancellationToken);
        return true;
    }
        
}
