using Microsoft.EntityFrameworkCore;

namespace TodoLists.App.Models;

[Index(nameof(UsernameLowerCase), IsUnique = true)]
public class SuperUser
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string UsernameLowerCase { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
}