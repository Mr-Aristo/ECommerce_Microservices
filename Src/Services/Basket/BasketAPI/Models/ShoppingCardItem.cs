namespace BasketAPI.Models;

/// <summary>
/// The ShoppingCardItem class represents an individual item in a shopping basket. 
/// It contains properties for the quantity of the item, its color, price, product ID, and product name. 
/// This class is used as part of the ShoppingCard model to manage the items that a user has added to their shopping basket. 
/// </summary>
public class ShoppingCardItem
{
    public int Quantity { get; set; } = default!;
    public string Color { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public Guid ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
}
