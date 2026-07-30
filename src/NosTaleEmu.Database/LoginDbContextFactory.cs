using Microsoft.EntityFrameworkCore;

namespace NosTaleEmu.Database;

/// <summary>
/// Punto único donde se sabe que estamos usando EF Core + Pomelo para MySQL.
/// El resto del proyecto (Program.cs, etc.) solo necesita un connection
/// string, sin tener que armar DbContextOptionsBuilder a mano.
/// </summary>
public static class LoginDbContextFactory
{
    public static LoginDbContext Create(string connectionString)
    {
        DbContextOptions<LoginDbContext> options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return new LoginDbContext(options);
    }

    /// <summary>
    /// Crea la tabla "accounts" si todavía no existe. Pensado para que
    /// alguien nuevo pueda arrancar el servidor sin correr migraciones a
    /// mano; en un entorno productivo real conviene migrar con
    /// `dotnet ef database update` en su lugar.
    /// </summary>
    public static void EnsureDatabaseReady(string connectionString)
    {
        using LoginDbContext context = Create(connectionString);
        context.Database.EnsureCreated();
    }
}
