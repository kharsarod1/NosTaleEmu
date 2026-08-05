using Microsoft.EntityFrameworkCore;
using NosTaleEmu.Database.Entities;

namespace NosTaleEmu.Database;

public sealed class LoginDbContext : DbContext
{
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    public LoginDbContext(DbContextOptions<LoginDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.ToTable("accounts");

            entity.Property(a => a.Username)
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(a => a.Username)
                .IsUnique();

            entity.Property(a => a.Password)
                .HasColumnName("password")
                .HasMaxLength(128) // SHA-512 en hex: 64 bytes -> 128 caracteres
                .IsRequired();
        });
    }
}
