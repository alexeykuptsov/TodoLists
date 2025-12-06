using Microsoft.EntityFrameworkCore;

namespace TodoLists.App.Entities;

[Index(nameof(Token), IsUnique = true)]
public class RefreshToken
{
    public long Id { get; set; }
    public required string Token { get; set; }
    public required DateTime Created { get; set; }
    public required DateTime Expires { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    
    // Foreign key relationships - either UserId OR SuperUserId should be set, not both
    public long? UserId { get; set; }
    public User? User { get; set; }
    
    public long? SuperUserId { get; set; }
    public SuperUser? SuperUser { get; set; }
}