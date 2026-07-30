namespace NosTaleEmu.Database.Entities.World;

// TODO: esto es un punto de partida mínimo; sumá los campos reales
// (clase, mapa, posición, stats, etc.) a medida que los necesites.
public sealed class CharacterEntity
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Level { get; set; } = 1;
    public long Gold { get; set; }
}
