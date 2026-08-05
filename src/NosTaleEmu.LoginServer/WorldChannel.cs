namespace NosTaleEmu.LoginServer;

public sealed record WorldChannel(
    string Host,
    int Port,
    int ColorId,
    int WorldId,
    int ChannelId,
    string Name)
{
    public string ToProtocolString() => $"{Host}:{Port}:{ColorId}:{WorldId}.{ChannelId}.{Name}";
}
