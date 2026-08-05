using static System.Collections.Specialized.BitVector32;

namespace NosTaleEmu.WorldServer.Handlers;

public sealed class SelectHandler : IWorldPacketHandler
{
    public string Header => "select";

    public async Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken)
    {
        var slot = byte.Parse(args[0]);

        var character = await session.Characters.FindBySlotAsync(session.AccountId, slot, cancellationToken);
        if (character is null)
            return;

        byte rank = 3; // 3 = GM.

        await session.SendAsync("c_info_reset");
        await session.SendAsync($"c_map 0 {character.MapId} 0");

        await session.SendAsync($"c_info {character.Name} - -1 -1 - {character.Id} {rank}" +
            $" {(byte)character.Gender} {(byte)character.Hair} {(byte)character.HairColor} {(byte)character.Class} 1 0 0 0 0 0 0");

        await session.SendAsync($"at {character.Id} {character.MapId} {character.X} {character.Y} 2 0 1 -1");
    }
}
