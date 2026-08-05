using NosTaleEmu.WorldServer.Configuration;

namespace NosTaleEmu.WorldServer.Commands;

public sealed class ConsoleCommandContext
{
    public required WorldServerSettings Settings { get; init; }
    public required ConsoleCommandRegistry Registry { get; init; }
    public required CancellationTokenSource ShutdownSource { get; init; }
}

public interface IConsoleCommand
{
    string Name { get; }
    string Usage { get; }
    string Description { get; }
    Task ExecuteAsync(ConsoleCommandContext context, string[] args, CancellationToken cancellationToken);
}
