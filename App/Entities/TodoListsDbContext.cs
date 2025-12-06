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
        
        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            // Configure table name to use snake_case
            entity.ToTable("refresh_tokens");
            
            // Configure primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            
            // Configure properties with snake_case column names
            entity.Property(e => e.Token).HasColumnName("token").IsRequired();
            entity.Property(e => e.Created).HasColumnName("created").IsRequired();
            entity.Property(e => e.Expires).HasColumnName("expires").IsRequired();
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SuperUserId).HasColumnName("super_user_id");
            
            // Configure unique index on Token
            entity.HasIndex(e => e.Token).IsUnique();
            
            // Configure foreign key relationships
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.SuperUser)
                  .WithMany()
                  .HasForeignKey(e => e.SuperUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public DbSet<SuperUser> SuperUsers { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
}
