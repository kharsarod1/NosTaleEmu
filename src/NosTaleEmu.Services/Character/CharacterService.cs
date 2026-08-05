using Microsoft.EntityFrameworkCore;
using NosTaleEmu.Database;
using NosTaleEmu.Database.Entities.World;
using NosTaleEmu.Dto.Character;

namespace NosTaleEmu.Services.Character;

public sealed class CharacterService
{
    private readonly WorldDbContext _context;

    public CharacterService(WorldDbContext context)
    {
        _context = context;
    }

    public async Task<CharacterDto?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        CharacterEntity? entity = await _context.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<CharacterDto?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        CharacterEntity? entity = await _context.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<List<CharacterDto>> FindByAccountIdAsync(long accountId, CancellationToken cancellationToken = default)
    {
        List<CharacterEntity> entities = await _context.Characters
            .AsNoTracking()
            .Where(c => c.AccountId == accountId)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDto).ToList();
    }

    // FindBySlotAsync es un método que busca un personaje por su ID de cuenta y el número de slot. Devuelve el personaje encontrado o null si no existe.
    public async Task<CharacterDto?> FindBySlotAsync(long accountId, byte slot, CancellationToken cancellationToken = default)
    {
        CharacterEntity? entity = await _context.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.AccountId == accountId && c.Slot == slot, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    /// <returns>El personaje creado, o null si ya existía uno con ese nombre.</returns>
    public async Task<CharacterDto?> CreateCharacterAsync(CharacterDto character, CancellationToken cancellationToken = default)
    {
        bool nameTaken = await _context.Characters.AnyAsync(c => c.Name == character.Name, cancellationToken);
        if (nameTaken)
        {
            return null;
        }

        var entity = new CharacterEntity
        {
            AccountId = character.AccountId,
            Name = character.Name,
            Level = character.Level == 0 ? (byte)1 : character.Level,
            JobLevel = character.JobLevel,
            HeroLevel = character.HeroLevel,
            Experience = character.Experience,
            JobExperience = character.JobExperience,
            HeroExperience = character.HeroExperience,
            Gold = character.Gold,
            Class = character.Class,
            Gender = character.Gender,
            Hair = character.Hair,
            HairColor = character.HairColor,
            Faction = character.Faction,
            Health = character.Health,
            Mana = character.Mana,
            Dignity = character.Dignity,
            Reputation = character.Reputation,
            Compliments = character.Compliments,
            MapId = character.MapId,
            X = character.X,
            Y = character.Y,
            Slot = character.Slot
        };

        _context.Characters.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    /// <returns>true si se borró; false si no existía.</returns>
    public async Task<bool> DeleteCharacterAsync(long id, CancellationToken cancellationToken = default)
    {
        CharacterEntity? entity = await _context.Characters
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.Characters.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static CharacterDto ToDto(CharacterEntity entity) => new()
    {
        Id = entity.Id,
        AccountId = entity.AccountId,
        Name = entity.Name,
        Level = entity.Level,
        JobLevel = entity.JobLevel,
        HeroLevel = entity.HeroLevel,
        Experience = entity.Experience,
        JobExperience = entity.JobExperience,
        HeroExperience = entity.HeroExperience,
        Gold = entity.Gold,
        Class = entity.Class,
        Gender = entity.Gender,
        Hair = entity.Hair,
        HairColor = entity.HairColor,
        Faction = entity.Faction,
        Health = entity.Health,
        Mana = entity.Mana,
        Dignity = entity.Dignity,
        Reputation = entity.Reputation,
        Compliments = entity.Compliments,
        MapId = entity.MapId,
        X = entity.X,
        Y = entity.Y,
        Slot = entity.Slot
    };
}
