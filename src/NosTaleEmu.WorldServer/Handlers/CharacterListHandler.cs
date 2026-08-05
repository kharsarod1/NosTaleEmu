namespace NosTaleEmu.WorldServer.Handlers;

public sealed class CharacterListHandler : IWorldPacketHandler
{
    public string Header => "game_start";

    public async Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken)
    {
        // TODO: reemplazar por la lista real de personajes de la cuenta
        // (consultar WorldDbContext.Characters filtrando por AccountId).

        var characters = await session.Characters.FindByAccountIdAsync(session.AccountId);

        await session.SendAsync("clist_start 0");
        foreach (var character in characters)
        {
            await session.SendAsync($"clist {character.Slot} {character.Name} 0 {(byte)character.Gender} {(byte)character.Hair} {(byte)character.HairColor} 0 {(byte)character.Class} {character.Level} {character.HeroLevel} 0.0.0.0.0.0.0.0 0 0 -1 -1 -1");
        }

        await session.SendAsync("clist_end");
    }
}
