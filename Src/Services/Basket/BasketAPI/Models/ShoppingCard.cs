namespace BasketAPI.Models
{
    /// <summary>
    /// The ShoppingCard class represents a user's shopping basket in the BasketAPI. 
    /// It contains properties for the user's name, a list of items in the basket, 
    /// and a computed total price based on the items and their quantities. 
    /// The class provides constructors for initializing the shopping card with a 
    /// user name or as an empty card. This model is used throughout the application 
    /// to manage and manipulate the contents of a user's shopping basket.
    /// </summary>
    public class ShoppingCard
    {
        //string? vs default! - 1 - can be null but need to check in code / it's okay to be null at the begining but after that cannot be null

        public string UserName { get; set; } = default!;

        public List<ShoppingCardItem> Items { get; set; } = new();

        public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);

        public ShoppingCard(string userName)
        {
            UserName = userName;
        }
        public ShoppingCard()
        { }

    }
}
