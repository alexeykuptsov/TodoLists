using Microsoft.EntityFrameworkCore;

namespace TodoLists.App.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSerialColumns();
    }

    public DbSet<SuperUser> SuperUsers { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
}
