using System.Net;
using System.Net.Sockets;
using NosTaleEmu.Core.Configuration;
using NosTaleEmu.Core.Logging;
using NosTaleEmu.Database;
using NosTaleEmu.WorldServer;
using NosTaleEmu.WorldServer.Commands;
using NosTaleEmu.WorldServer.Configuration;
using Serilog;

const string ConfigPath = "config.json";

WorldServerSettings settings = JsonConfigLoader.LoadOrCreate(ConfigPath, new WorldServerSettings());

GameLogger.Initialize("WorldServer", settings.DisplayLogs);

Log.Information("Verificando base de datos...");
WorldDbContextFactory.EnsureDatabaseReady(settings.MySqlConnectionString);
Log.Information("Base de datos lista.");
Log.Information("Rates -> Exp x{ExpRate} | Drop x{DropRate} | Gold x{GoldRate}", settings.Rates.ExpRate, settings.Rates.DropRate, settings.Rates.GoldRate);

if (!settings.DisplayLogs)
{
    Log.Information("DisplayLogs está en false: no vas a ver logs de conexiones ni paquetes. Poné 'DisplayLogs': true en config.json si los necesitás.");
}

var listener = new TcpListener(IPAddress.Any, settings.Port);
listener.Start();

Log.Information("Escuchando en el puerto {Port}", settings.Port);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var commandRegistry = new ConsoleCommandRegistry();
var commandContext = new ConsoleCommandContext
{
    Settings = settings,
    Registry = commandRegistry,
    ShutdownSource = cts
};

if (settings.EnableCommands)
{
    _ = RunCommandLoopAsync(commandRegistry, commandContext, cts.Token);
}

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

    GameLogger.Traffic.Information("Nueva conexión: {RemoteEndPoint}", client.Client.RemoteEndPoint);

    WorldDbContext sessionDbContext = WorldDbContextFactory.Create(settings.MySqlConnectionString);
    var session = new WorldSession(client, sessionDbContext);
    _ = session.RunAsync(cts.Token);
}

listener.Stop();
Log.Information("WorldServer detenido.");

static async Task RunCommandLoopAsync(ConsoleCommandRegistry registry, ConsoleCommandContext context, CancellationToken cancellationToken)
{
    Log.Information("Consola de comandos activa. Escribí 'help' para ver la lista.");

    while (!cancellationToken.IsCancellationRequested)
    {
        string? line;
        try
        {
            line = await Task.Run(Console.ReadLine, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandName = tokens[0];
        string[] commandArgs = tokens[1..];

        if (!registry.TryGet(commandName, out IConsoleCommand? command) || command is null)
        {
            Log.Warning("Comando desconocido: '{Command}'. Escribí 'help' para ver la lista.", commandName);
            continue;
        }

        try
        {
            await command.ExecuteAsync(context, commandArgs, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error ejecutando el comando '{Command}'", commandName);
        }
    }
}
