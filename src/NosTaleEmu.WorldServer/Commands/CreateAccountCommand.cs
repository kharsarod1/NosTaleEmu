using System.Security.Cryptography;
using System.Text;
using NosTaleEmu.Database;
using NosTaleEmu.Services.Account;
using Serilog;

namespace NosTaleEmu.WorldServer.Commands;

public sealed class CreateAccountCommand : IConsoleCommand
{
    public string Name => "create-account";
    public string Usage => "create-account <usuario> <contraseña>";
    public string Description => "Crea una cuenta nueva en la base de login";

    public async Task ExecuteAsync(ConsoleCommandContext context, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 2)
        {
            Log.Warning("Uso: {Usage}", Usage);
            return;
        }

        string username = args[0];
        string plainPassword = args[1];

        using LoginDbContext dbContext = LoginDbContextFactory.Create(context.Settings.LoginMySqlConnectionString);
        var accountService = new AccountService(dbContext);

        string hash = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(plainPassword)));
        bool created = await accountService.CreateAccountAsync(username, hash, cancellationToken);

        if (created)
        {
            Log.Information("Cuenta '{Username}' creada correctamente.", username);
        }
        else
        {
            Log.Warning("Ya existe una cuenta con el usuario '{Username}'.", username);
        }
    }
}
