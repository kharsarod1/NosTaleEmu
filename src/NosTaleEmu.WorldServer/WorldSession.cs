using System.Net.Sockets;
using NosTaleEmu.Core.Cryptography;
using NosTaleEmu.Core.Networking;
using NosTaleEmu.WorldServer.Handlers;

namespace NosTaleEmu.WorldServer;

public sealed class WorldSession : ClientSessionBase
{
    // Un solo registro compartido por todas las sesiones: los handlers son
    // sin estado, no hace falta uno por conexión.
    private static readonly WorldPacketHandlerRegistry HandlerRegistry = new();

    private readonly WorldCipher _worldCipher;

    /// <summary>
    /// El primer paquete que manda el cliente va cifrado con el esquema de
    /// "parámetro especial" (DecryptCustomParameter), no con el Decrypt
    /// normal. Trae el sessionId real asignado por el LoginServer; recién
    /// ahí arranca el resto del protocolo.
    /// </summary>
    private bool _handshakeCompleted;

    private int _lastKeepAliveId = -1;

    public string? CharacterName { get; private set; }

    public WorldSession(TcpClient tcpClient) : base(tcpClient, new WorldCipher(), sessionId: 0)
    {
        _worldCipher = (WorldCipher)Cipher;
    }

    protected override string DecryptIncoming(byte[] raw) =>
        _handshakeCompleted
            ? _worldCipher.Decrypt(raw, SessionId)
            : _worldCipher.DecryptCustomParameter(raw);

    protected override IEnumerable<string> SplitPackets(string decrypted) =>
        _handshakeCompleted
            ? decrypted.Split((char)0xFF, StringSplitOptions.RemoveEmptyEntries)
            : [decrypted];

    protected override async Task OnPacketReceivedAsync(string packet, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet))
        {
            return;
        }

        if (!_handshakeCompleted)
        {
            await CompleteHandshakeAsync(packet);
            return;
        }

        // Formato: "<keepAliveId> <header> <arg1> <arg2>..."
        // '^' representa un espacio dentro de un argumento (ej: mensajes de chat).
        string readable = packet.Replace('^', ' ');
        string[] parts = readable.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || !int.TryParse(parts[0], out int keepAliveId))
        {
            return;
        }

        _lastKeepAliveId = keepAliveId;

        // Mensajería rápida (/, :, ;) usa un solo carácter como header.
        string header = parts[1] is [var shortcut, ..] && shortcut is '/' or ':' or ';'
            ? shortcut.ToString()
            : parts[1].Replace("#", "");

        Console.WriteLine($"[World] << {readable}");

        if (HandlerRegistry.TryGetHandler(header, out IWorldPacketHandler? handler))
        {
            await handler!.HandleAsync(this, parts[2..], cancellationToken);
        }
        else
        {
            Console.WriteLine($"[World] Header no manejado: {header}");
        }
    }

    private async Task CompleteHandshakeAsync(string customParameter)
    {
        // Formato esperado tras DecryptCustomParameter: "<keepAliveId> <sessionId>[\...resto]"
        string[] parts = customParameter.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int keepAliveId = 0;
        int sessionId = 0;

        bool ok = parts.Length >= 2
            && int.TryParse(parts[0], out keepAliveId)
            && int.TryParse(parts[1].Split('\\')[0], out sessionId);

        if (!ok)
        {
            Console.WriteLine("[World] Handshake inválido, cerrando conexión.");
            Dispose();
            return;
        }

        _lastKeepAliveId = keepAliveId;
        SessionId = sessionId;
        _handshakeCompleted = true;

        Console.WriteLine($"[World] Handshake OK, sessionId={SessionId}");

        await SendAsync("OK");
    }
}
