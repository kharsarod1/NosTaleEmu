using Microsoft.EntityFrameworkCore;

namespace NosTaleEmu.Database;

public static class LoginDbContextFactory
{
    public static LoginDbContext Create(string connectionString)
    {
        DbContextOptions<LoginDbContext> options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return new LoginDbContext(options);
    }

    public static void EnsureDatabaseReady(string connectionString)
    {
        using LoginDbContext context = Create(connectionString);
        context.Database.EnsureCreated();
    }
}
