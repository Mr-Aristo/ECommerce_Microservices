namespace Order.Domain.Models;

/// <summary>
/// Represents a product in the domain with a name and price.
/// Inherits from <see cref="Entity{ProductId}"/> to enforce strongly typed identity.
/// </summary>
public class Product : Entity<ProductId>
{
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;

    /// <summary>
    /// Factory method to create a new <see cref="Product"/> instance with the required properties.
    /// Throws an exception if name is null/empty or price is zero/negative.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="name">The name of the product.</param>
    /// <param name="price">The price of the product.</param>
    /// <returns>A new instance of <see cref="Product"/>.</returns>
    public static Product Create(ProductId id, string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        var product = new Product
        {
            Id = id,
            Name = name,
            Price = price
        };

        return product;
    }

}