using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using NosTaleEmu.Core.Configuration;
using NosTaleEmu.Database;
using NosTaleEmu.LoginServer;
using NosTaleEmu.LoginServer.Configuration;
using NosTaleEmu.Services.Account;

const string ConfigPath = "config.json";

LoginServerSettings settings = JsonConfigLoader.LoadOrCreate(ConfigPath, new LoginServerSettings());

Console.WriteLine("[LoginServer] Verificando base de datos...");
LoginDbContextFactory.EnsureDatabaseReady(settings.MySqlConnectionString);
Console.WriteLine("[LoginServer] Base de datos lista.");

// Modo CLI: "dotnet run -- create-account <usuario> <contraseña>"
// Pensado para crear cuentas de prueba sin escribir SQL a mano. Calcula el
// mismo hash SHA-512 que manda el cliente real y lo guarda en la base.
if (args is ["create-account", string user, string plainPassword])
{
    using LoginDbContext dbContext = LoginDbContextFactory.Create(settings.MySqlConnectionString);
    var accountService = new AccountService(dbContext);

    string hash = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(plainPassword)));
    bool created = await accountService.CreateAccountAsync(user, hash);

    Console.WriteLine(created
        ? $"Cuenta '{user}' creada correctamente."
        : $"Ya existe una cuenta con el usuario '{user}'.");

    return;
}

var channels = settings.Channels
    .Select(c => new WorldChannel(c.Host, c.Port, c.ColorId, c.WorldId, c.ChannelId, c.Name))
    .ToList();

var listener = new TcpListener(IPAddress.Any, settings.Port);
listener.Start();

Console.WriteLine($"[LoginServer] Escuchando en el puerto {settings.Port}...");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

while (!cts.IsCancellationRequested)
{
    TcpClient client;
    try
    {
        client = await listener.AcceptTcpClientAsync(cts.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }

    Console.WriteLine($"[LoginServer] Nueva conexión: {client.Client.RemoteEndPoint}");

    // Un DbContext por sesión: EF Core no es thread-safe para compartir uno
    // solo entre conexiones concurrentes. LoginSession se hace cargo de
    // liberarlo cuando la conexión se cierra.
    LoginDbContext sessionDbContext = LoginDbContextFactory.Create(settings.MySqlConnectionString);
    var session = new LoginSession(client, channels, sessionDbContext);
    _ = session.RunAsync(cts.Token);
}

listener.Stop();
Console.WriteLine("[LoginServer] Detenido.");
