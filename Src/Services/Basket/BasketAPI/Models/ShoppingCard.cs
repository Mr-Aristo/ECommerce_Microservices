namespace BasketAPI.Models
{
    public class ShoppingCard
    {
        //string? vs default! - 1 - can be null but need to check in code / it okay to be null at the begining but after that cannot be null

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
