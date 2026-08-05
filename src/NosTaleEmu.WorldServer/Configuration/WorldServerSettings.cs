namespace NosTaleEmu.WorldServer.Configuration;

public sealed class WorldServerSettings
{
    public int Port { get; set; } = 4001;

    public string MySqlConnectionString { get; set; } =
        "Server=127.0.0.1;Port=3306;Database=world;Uid=root;Pwd=segundo123;";

    public string LoginMySqlConnectionString { get; set; } =
        "Server=127.0.0.1;Port=3306;Database=login;Uid=root;Pwd=segundo123;";

    public bool DisplayLogs { get; set; } = false;

    public bool EnableCommands { get; set; } = true;

    public RateSettings Rates { get; set; } = new();
}

public sealed class RateSettings
{
    public double ExpRate { get; set; } = 1;
    public double DropRate { get; set; } = 1;
    public double GoldRate { get; set; } = 1;
    public double GoldDropRate { get; set; } = 1;
    public double ReputationRate { get; set; } = 1;
    public double FairyXpRate { get; set; } = 1;
}
