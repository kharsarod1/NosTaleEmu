namespace NosTaleEmu.LoginServer.Configuration;

public sealed class LoginServerSettings
{
    public int Port { get; set; } = 4005;

    public string MySqlConnectionString { get; set; } =
        "Server=127.0.0.1;Port=3306;Database=login;Uid=nostaleemu;Pwd=changeme;";

    public List<ChannelSettings> Channels { get; set; } =
    [
        new ChannelSettings
        {
            Host = "127.0.0.1",
            Port = 4001,
            ColorId = 0,
            WorldId = 1,
            ChannelId = 1,
            Name = "NosEmu"
        }
    ];
}

public sealed class ChannelSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 4001;
    public int ColorId { get; set; }
    public int WorldId { get; set; } = 1;
    public int ChannelId { get; set; } = 1;
    public string Name { get; set; } = "NosEmu";
}
