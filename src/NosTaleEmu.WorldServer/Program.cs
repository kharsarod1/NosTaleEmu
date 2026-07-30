using System.Net;
using System.Net.Sockets;
using NosTaleEmu.Core.Configuration;
using NosTaleEmu.Database;
using NosTaleEmu.WorldServer;
using NosTaleEmu.WorldServer.Configuration;

const string ConfigPath = "config.json";

WorldServerSettings settings = JsonConfigLoader.LoadOrCreate(ConfigPath, new WorldServerSettings());

Console.WriteLine("[WorldServer] Verificando base de datos...");
WorldDbContextFactory.EnsureDatabaseReady(settings.MySqlConnectionString);
Console.WriteLine("[WorldServer] Base de datos lista.");
Console.WriteLine($"[WorldServer] Rates -> Exp x{settings.Rates.ExpRate} | Drop x{settings.Rates.DropRate} | Gold x{settings.Rates.GoldRate}");

var listener = new TcpListener(IPAddress.Any, settings.Port);
listener.Start();

Console.WriteLine($"[WorldServer] Escuchando en el puerto {settings.Port}...");

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

    Console.WriteLine($"[WorldServer] Nueva conexión: {client.Client.RemoteEndPoint}");

    var session = new WorldSession(client);
    _ = session.RunAsync(cts.Token);
}

listener.Stop();
Console.WriteLine("[WorldServer] Detenido.");
