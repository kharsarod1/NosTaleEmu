using Serilog;

namespace NosTaleEmu.WorldServer.Commands;

public sealed class ShutdownCommand : IConsoleCommand
{
    public string Name => "shutdown";
    public string Usage => "shutdown";
    public string Description => "Detiene el WorldServer de forma prolija";

    public Task ExecuteAsync(ConsoleCommandContext context, string[] args, CancellationToken cancellationToken)
    {
        Log.Information("Deteniendo el servidor...");
        context.ShutdownSource.Cancel();
        return Task.CompletedTask;
    }
}
