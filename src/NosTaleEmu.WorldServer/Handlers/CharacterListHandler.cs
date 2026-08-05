using NosTaleEmu.Dto.Character;

namespace NosTaleEmu.WorldServer.Handlers;

public sealed class CharacterListHandler : IWorldPacketHandler
{
    public string Header => "game_start";

    public async Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken)
    {
        List<CharacterDto> characters = await session.Characters.FindByAccountIdAsync(session.AccountId, cancellationToken);

        await session.SendAsync("clist_start 0");

        foreach (CharacterDto character in characters)
        {
            await session.SendAsync($"clist {character.Slot} {character.Name} 0 {(byte)character.Gender}" +
                $" {(byte)character.Hair} {(byte)character.HairColor} 0 {(byte)character.Class} {character.Level} {character.HeroLevel} 0.0.0.0.0.0.0.0 0 0 -1 -1 -1");
        }

        await session.SendAsync("clist_end");
    }
}
