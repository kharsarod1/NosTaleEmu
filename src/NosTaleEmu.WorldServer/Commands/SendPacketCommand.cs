using Serilog;

namespace NosTaleEmu.WorldServer.Commands;

public sealed class SendPacketCommand : IConsoleCommand
{
    public string Name => "sendpacket";
    public string Usage => "sendpacket <jugador> <paquete> [parámetros...]";
    public string Description => "Manda un paquete crudo a un jugador conectado. Ej: sendpacket jugador shout hola mundo";

    public async Task ExecuteAsync(ConsoleCommandContext context, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 2)
        {
            Log.Warning("Uso: {Usage}", Usage);
            return;
        }

        string playerName = args[0];
        string packetHeader = args[1];
        string packetArgs = string.Join(' ', args.Skip(2));

        if (!WorldSessionRegistry.TryGet(playerName, out WorldSession? session) || session is null)
        {
            Log.Warning("No hay ningún jugador conectado con el nombre '{Player}'.", playerName);
            return;
        }

        string packet = string.IsNullOrWhiteSpace(packetArgs) ? packetHeader : $"{packetHeader} {packetArgs}";

        await session.SendAsync(packet);

        Log.Information("Paquete enviado a '{Player}': {Packet}", playerName, packet);
    }
}
