using Microsoft.EntityFrameworkCore;
using NosTaleEmu.Database.Entities.World;

namespace NosTaleEmu.Database;

public sealed class WorldDbContext : DbContext
{
    public DbSet<CharacterEntity> Characters => Set<CharacterEntity>();

    public WorldDbContext(DbContextOptions<WorldDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CharacterEntity>(entity =>
        {
            entity.ToTable("characters");

            entity.Property(c => c.Name)
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(c => c.Name)
                .IsUnique();

            entity.HasIndex(c => c.AccountId);
        });
    }
}
