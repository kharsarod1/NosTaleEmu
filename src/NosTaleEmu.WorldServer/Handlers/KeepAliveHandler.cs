namespace NosTaleEmu.WorldServer.Handlers;

public sealed class KeepAliveHandler : IWorldPacketHandler
{
    public string Header => "0";

    public Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
