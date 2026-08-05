namespace NosTaleEmu.WorldServer.Handlers;

public interface IWorldPacketHandler
{
    string Header { get; }

    Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken);
}
