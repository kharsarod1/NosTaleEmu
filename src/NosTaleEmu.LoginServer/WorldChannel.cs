namespace NosTaleEmu.LoginServer;

/// <summary>
/// Representa un canal (WorldServer) disponible que se ofrece al cliente
/// tras un login exitoso.
/// </summary>
/// <param name="Host">IP o dominio del WorldServer.</param>
/// <param name="Port">Puerto TCP del WorldServer.</param>
/// <param name="ColorId">Color del canal en la lista (0 = normal, etc.).</param>
/// <param name="WorldId">Id del mundo/servidor.</param>
/// <param name="ChannelId">Id del canal dentro de ese mundo.</param>
/// <param name="Name">Nombre mostrado en el selector de canales.</param>
public sealed record WorldChannel(
    string Host,
    int Port,
    int ColorId,
    int WorldId,
    int ChannelId,
    string Name)
{
    // Formato: ip:puerto:colorCanal:worldId.channelId.nombre
    // Ej: 127.0.0.1:7575:0:1.1.NosEmu
    public string ToProtocolString() => $"{Host}:{Port}:{ColorId}:{WorldId}.{ChannelId}.{Name}";
}
