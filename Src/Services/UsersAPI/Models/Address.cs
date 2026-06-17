namespace UsersAPI.Models;

public class Address
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Line { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public bool IsDefault { get; set; }
}
