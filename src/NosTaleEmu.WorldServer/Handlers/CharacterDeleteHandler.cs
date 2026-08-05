using NosTaleEmu.Core.Logging;
using NosTaleEmu.Dto.Character;

namespace NosTaleEmu.WorldServer.Handlers;

public sealed class CharacterDeleteHandler : IWorldPacketHandler
{
    public string Header => "Char_DEL";

    public async Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 2 || !byte.TryParse(args[^2], out byte slot))
        {
            GameLogger.Traffic.Warning("CHAR_DEL con argumentos inválidos");
            return;
        }

        CharacterDto? character = await session.Characters.FindBySlotAsync(session.AccountId, slot, cancellationToken);

        if (character is null)
        {
            await session.SendAsync("msg 0 No tenés ningún personaje en ese slot.");
            return;
        }

        bool deleted = await session.Characters.DeleteCharacterAsync(character.Id, cancellationToken);

        if (!deleted)
        {
            await session.SendAsync("msg 0 No se pudo borrar el personaje.");
            return;
        }

        if (string.Equals(session.CharacterName, character.Name, StringComparison.OrdinalIgnoreCase))
        {
            session.CharacterName = null;
        }

        GameLogger.Traffic.Information("Personaje '{Character}' (slot {Slot}, accountId={AccountId}) eliminado", character.Name, slot, session.AccountId);

        await session.SendAsync("OK");
    }
}
