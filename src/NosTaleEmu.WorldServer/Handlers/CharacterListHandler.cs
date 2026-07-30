namespace NosTaleEmu.WorldServer.Handlers;

public sealed class CharacterListHandler : IWorldPacketHandler
{
    public string Header => "game_start";

    public async Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken)
    {
        await session.SendAsync("clist_start 0");
        await session.SendAsync("clist_end");
    }
}
