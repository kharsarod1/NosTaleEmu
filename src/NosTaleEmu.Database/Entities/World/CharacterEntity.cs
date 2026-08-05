using NosTaleEmu.Core.Enums.Characters;
using NosTaleEmu.Core.Enums.World;

namespace NosTaleEmu.Database.Entities.World;

public sealed class CharacterEntity
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Level { get; set; }
    public byte JobLevel { get; set; }
    public byte HeroLevel { get; set; }
    public long Experience { get; set; }
    public long JobExperience { get; set; }
    public long HeroExperience { get; set; }
    public long Gold { get; set; }
    public ClassType Class { get; set; }
    public GenderType Gender { get; set; }
    public HairStyleType Hair { get; set; }
    public HairColorType HairColor { get; set; }
    public FactionType Faction { get; set; }
    public long Health { get; set; }
    public long Mana { get; set; }
    public short Dignity { get; set; }
    public int Reputation { get; set; }
    public int Compliments { get; set; }
    public int MapId { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public byte Slot { get; set; }
}
