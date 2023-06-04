using Microsoft.EntityFrameworkCore;

namespace TodoLists.App.Entities;

[Index(nameof(ProfileId), nameof(UsernameLowerCase), IsUnique = true)]
public class User
{
    public long Id { get; set; }
    public long ProfileId { get; set; } public Profile Profile { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string UsernameLowerCase { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
}