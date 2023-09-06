using Microsoft.EntityFrameworkCore;

namespace TodoLists.App.Entities;

public class TodoListsDbContext : DbContext
{
    public TodoListsDbContext(DbContextOptions<TodoListsDbContext> options)
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
    public DbSet<Project> Projects { get; set; } = null!;
}
