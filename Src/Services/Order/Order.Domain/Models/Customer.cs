namespace Order.Domain.Models;

/// <summary>
/// Represents a customer entity within the domain.
/// Contains basic customer information such as name and email.
/// Inherits from <see cref="Entity{CustomerId}"/> to provide a strongly typed identifier.
/// </summary>
public class Customer : Entity<CustomerId>
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    /// <summary>
    /// Factory method to create a new <see cref="Customer"/> instance with the required parameters.
    /// Validates that name and email are not null or whitespace.
    /// </summary>
    /// <param name="id">The unique identifier for the customer.</param>
    /// <param name="name">The name of the customer.</param>
    /// <param name="email">The email address of the customer.</param>
    /// <returns>A new instance of <see cref="Customer"/>.</returns>
    public static Customer Create(CustomerId id, string name, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var customer = new Customer
        {
            Id = id,
            Name = name,
            Email = email
        };

        return customer;
    }

}
