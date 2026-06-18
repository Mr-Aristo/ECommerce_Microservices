namespace UsersAPI.Models;

public class FavoriteItem
{
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
