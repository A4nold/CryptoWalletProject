using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("Users", "identity");

        entity.HasKey(u => u.Id);

        entity.HasIndex(u => u.Email)
            .IsUnique();

        entity.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        entity.Property(u => u.CreatedAt)
            .IsRequired();

        entity.Property(u => u.UpdatedAt);
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RefreshToken>();

        entity.ToTable("RefreshTokens", "identity");

        entity.HasKey(r => r.Id);

        entity.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(512);

        entity.HasIndex(r => r.UserId);

        entity
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
