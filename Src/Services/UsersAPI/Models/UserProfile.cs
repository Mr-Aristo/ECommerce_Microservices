namespace UsersAPI.Models;

// User-centric business data. Id = Keycloak 'sub' (credentials/roles stay in Keycloak).
public class UserProfile
{
    public string Id { get; set; } = default!;          // Keycloak sub
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public List<Address> Addresses { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
