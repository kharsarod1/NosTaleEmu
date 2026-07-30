namespace NosTaleEmu.WorldServer.Configuration;

public sealed class WorldServerSettings
{
    public int Port { get; set; } = 4001;

    public string MySqlConnectionString { get; set; } =
        "Server=127.0.0.1;Port=3306;Database=world;Uid=nostaleemu;Pwd=changeme;";

    public RateSettings Rates { get; set; } = new();
}

/// <summary>
/// Multiplicadores del servidor. 1 = rate normal/oficial. Se pueden tocar
/// libremente en config.json sin recompilar nada.
/// </summary>
public sealed class RateSettings
{
    public double ExpRate { get; set; } = 1;
    public double DropRate { get; set; } = 1;
    public double GoldRate { get; set; } = 1;
    public double GoldDropRate { get; set; } = 1;
    public double ReputationRate { get; set; } = 1;
    public double FairyXpRate { get; set; } = 1;
}
