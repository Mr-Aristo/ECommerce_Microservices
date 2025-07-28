namespace Order.Domain.ValueObjects;

/// <summary>
/// Represents a value object for customer or order address information.
/// Contains personal and geographical details such as name, email, and location data.
/// </summary>
public record Address
{
    public string FirstName { get; } = default!;
    public string LastName { get; } = default!;
    public string? EmailAddress { get; } = default!;
    public string AddressLine { get; } = default!;
    public string Country { get; } = default!;
    public string State { get; } = default!;
    public string ZipCode { get; } = default!;

    /// <summary>
    /// Protected parameterless constructor required for deserialization or ORM tools.
    /// </summary>
    protected Address() { }

    /// <summary>
    /// Private constructor to enforce controlled creation through the <see cref="Of"/> factory method.
    /// </summary>
    private Address(string firstName, string lastName, string emailAddress, string addressLine, string country, string state, string zipCode)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        AddressLine = addressLine;
        Country = country;
        State = state;
        ZipCode = zipCode;
    }

    /// <summary>
    /// Factory method to create a new instance of <see cref="Address"/> with validation.
    /// Throws exceptions if required fields are invalid.
    /// </summary>
    /// <param name="firstName">First name of the person associated with the address.</param>
    /// <param name="lastName">Last name of the person associated with the address.</param>
    /// <param name="emailAddress">Email address (optional but validated).</param>
    /// <param name="addressLine">Main address line (required).</param>
    /// <param name="country">Country of the address.</param>
    /// <param name="state">State or province of the address.</param>
    /// <param name="zipCode">Postal or ZIP code.</param>
    /// <returns>A valid <see cref="Address"/> instance.</returns>
    public static Address Of(string firstName, string lastName, string emailAddress, string addressLine, string country, string state, string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);

        return new Address(firstName, lastName, emailAddress, addressLine, country, state, zipCode);
    }
}

/*
   "Of" metodu, bir sınıfın veya bir türün örneğini oluşturmak için 
   kullanılan fabrika metodu (factory method) gibi çalışır.
   Bu metot, doğrudan yapıcı metodu (constructor) kullanmak 
   yerine, ek doğrulama veya özel mantık eklemek istediğinizde kullanılır.
   Amaç:
   Yeni bir nesne oluştururken ek kontrol veya mantık eklemek.
   Doğrudan new ile oluşturmayı engelleyip nesne yaratmayı daha güvenli hale getirmek.
 */