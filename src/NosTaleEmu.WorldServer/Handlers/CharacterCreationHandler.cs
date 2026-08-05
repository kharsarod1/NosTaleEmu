using NosTaleEmu.Core.Enums.Characters;
using NosTaleEmu.Core.Logging;
using NosTaleEmu.Dto.Character;

namespace NosTaleEmu.WorldServer.Handlers;

public sealed class CharacterCreationHandler : IWorldPacketHandler
{
    public string Header => "CHAR_NEW";

    public async Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 5)
        {
            GameLogger.Traffic.Warning("CHAR_NEW con muy pocos argumentos");
            return;
        }

        string name = args[0];

        byte slot = 0, genderRaw = 0, hairStyleRaw = 0, hairColorRaw = 0;

        bool parsedOk = byte.TryParse(args[1], out slot)
            && byte.TryParse(args[2], out genderRaw)
            && byte.TryParse(args[3], out hairStyleRaw)
            && byte.TryParse(args[4], out hairColorRaw);

        if (!parsedOk || string.IsNullOrWhiteSpace(name))
        {
            GameLogger.Traffic.Warning("CHAR_NEW con argumentos inválidos");
            return;
        }

        var newCharacter = new CharacterDto
        {
            AccountId = session.AccountId,
            Name = name,
            Slot = slot,
            Gender = (GenderType)genderRaw,
            Hair = (HairStyleType)hairStyleRaw,
            HairColor = (HairColorType)hairColorRaw,
            Level = 1,
            MapId = 1,
            X = 79,
            Y = 117
        };

        CharacterDto? created = await session.Characters.CreateCharacterAsync(newCharacter, cancellationToken);

        if (created is null)
        {
            await session.SendAsync("info Ese nombre ya está en uso.");
            return;
        }

        session.CharacterName = created.Name;

        GameLogger.Traffic.Information("Personaje '{Character}' creado (id={Id}, accountId={AccountId})", created.Name, created.Id, created.AccountId);

        await session.SendAsync("OK");
    }
}
