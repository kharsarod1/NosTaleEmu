using Microsoft.EntityFrameworkCore;

namespace NosTaleEmu.Database;

public static class WorldDbContextFactory
{
    public static WorldDbContext Create(string connectionString)
    {
        DbContextOptions<WorldDbContext> options = new DbContextOptionsBuilder<WorldDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return new WorldDbContext(options);
    }

    public static void EnsureDatabaseReady(string connectionString)
    {
        using WorldDbContext context = Create(connectionString);
        context.Database.EnsureCreated();
    }
}
