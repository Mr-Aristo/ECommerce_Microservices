using BasketAPI.Exceptions;

namespace BasketAPI.Data;

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
        await session.SaveChangesAsync();
        return basket;
    }
    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        session.Delete(userName);
        await session.SaveChangesAsync();
        return true;
    }
        
}
