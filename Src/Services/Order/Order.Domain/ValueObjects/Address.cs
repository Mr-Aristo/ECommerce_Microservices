namespace Order.Domain.ValueObjects;

public record Address
{
    public string FirstName { get; } = default!;
    public string LastName { get; } = default!;
    public string? EmailAddress { get; } = default!;
    public string AddressLine { get; } = default!;
    public string Country { get; } = default!;
    public string State { get; } = default!;
    public string ZipCode { get; } = default!;

    protected Address() { }
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