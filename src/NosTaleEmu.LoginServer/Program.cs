using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using NosTaleEmu.Core.Configuration;
using NosTaleEmu.Core.Logging;
using NosTaleEmu.Database;
using NosTaleEmu.LoginServer;
using NosTaleEmu.LoginServer.Configuration;
using NosTaleEmu.Services.Account;
using Serilog;

GameLogger.Initialize("LoginServer");

const string ConfigPath = "config.json";

LoginServerSettings settings = JsonConfigLoader.LoadOrCreate(ConfigPath, new LoginServerSettings());

Log.Information("Verificando base de datos...");
LoginDbContextFactory.EnsureDatabaseReady(settings.MySqlConnectionString);
Log.Information("Base de datos lista.");

if (args is ["create-account", string user, string plainPassword])
{
    using LoginDbContext dbContext = LoginDbContextFactory.Create(settings.MySqlConnectionString);
    var accountService = new AccountService(dbContext);

    string hash = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(plainPassword)));
    bool created = await accountService.CreateAccountAsync(user, hash);

    if (created)
    {
        Log.Information("Cuenta '{Username}' creada correctamente.", user);
    }
    else
    {
        Log.Warning("Ya existe una cuenta con el usuario '{Username}'.", user);
    }

    return;
}

var channels = settings.Channels
    .Select(c => new WorldChannel(c.Host, c.Port, c.ColorId, c.WorldId, c.ChannelId, c.Name))
    .ToList();

var listener = new TcpListener(IPAddress.Any, settings.Port);
listener.Start();

Log.Information("Escuchando en el puerto {Port}", settings.Port);
Log.Information("Canales configurados: {ChannelCount}", channels.Count);

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

    Log.Debug("Nueva conexión: {RemoteEndPoint}", client.Client.RemoteEndPoint);

    LoginDbContext sessionDbContext = LoginDbContextFactory.Create(settings.MySqlConnectionString);
    var session = new LoginSession(client, channels, sessionDbContext);
    _ = session.RunAsync(cts.Token);
}

listener.Stop();
Log.Information("LoginServer detenido.");
