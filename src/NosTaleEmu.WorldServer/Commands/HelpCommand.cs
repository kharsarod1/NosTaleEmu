using Serilog;

namespace NosTaleEmu.WorldServer.Commands;

public sealed class HelpCommand : IConsoleCommand
{
    public string Name => "help";
    public string Usage => "help";
    public string Description => "Muestra la lista de comandos disponibles";

    public Task ExecuteAsync(ConsoleCommandContext context, string[] args, CancellationToken cancellationToken)
    {
        Log.Information("Comandos disponibles:");

        foreach (IConsoleCommand command in context.Registry.All.OrderBy(c => c.Name))
        {
            Log.Information("  {Usage}  ->  {Description}", command.Usage, command.Description);
        }

        return Task.CompletedTask;
    }
}
