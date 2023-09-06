using Microsoft.EntityFrameworkCore;

namespace TodoLists.App.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Profile
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long CreatedAt { get; set; }
}