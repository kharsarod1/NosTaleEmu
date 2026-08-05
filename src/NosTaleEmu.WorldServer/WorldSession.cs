using System.Net.Sockets;
using NosTaleEmu.Core.Cryptography;
using NosTaleEmu.Core.Logging;
using NosTaleEmu.Core.Networking;
using NosTaleEmu.Database;
using NosTaleEmu.Services.Character;
using NosTaleEmu.WorldServer.Handlers;

namespace NosTaleEmu.WorldServer;

public sealed class WorldSession : ClientSessionBase
{
    private static readonly WorldPacketHandlerRegistry HandlerRegistry = new();

    private readonly WorldCipher _worldCipher;
    private readonly WorldDbContext _dbContext;

    private bool _handshakeCompleted;
    private int _lastKeepAliveId = -1;
    private string? _characterName;

    public CharacterService Characters { get; }

    public long AccountId { get; set; }

    public string? CharacterName
    {
        get => _characterName;
        set
        {
            if (_characterName is not null)
            {
                WorldSessionRegistry.Unregister(_characterName);
            }

            _characterName = value;

            if (_characterName is not null)
            {
                WorldSessionRegistry.Register(_characterName, this);
            }
        }
    }

    public WorldSession(TcpClient tcpClient, WorldDbContext dbContext)
        : base(tcpClient, new WorldCipher(), sessionId: 0)
    {
        _worldCipher = (WorldCipher)Cipher;
        _dbContext = dbContext;
        Characters = new CharacterService(dbContext);
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

        string readable = packet.Replace('^', ' ');
        string[] parts = readable.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || !int.TryParse(parts[0], out int keepAliveId))
        {
            return;
        }

        _lastKeepAliveId = keepAliveId;

        string header = parts[1] is [var shortcut, ..] && shortcut is '/' or ':' or ';'
            ? shortcut.ToString()
            : parts[1].Replace("#", "");

        GameLogger.Traffic.Debug("<< {Packet}", readable);

        if (HandlerRegistry.TryGetHandler(header, out IWorldPacketHandler? handler))
        {
            await handler!.HandleAsync(this, parts[2..], cancellationToken);
        }
        else
        {
            GameLogger.Traffic.Debug("Header no manejado: {Header}", header);
        }
    }

    private async Task CompleteHandshakeAsync(string customParameter)
    {
        string[] parts = customParameter.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int keepAliveId = 0;
        int sessionId = 0;

        bool ok = parts.Length >= 2
            && int.TryParse(parts[0], out keepAliveId)
            && int.TryParse(parts[1].Split('\\')[0], out sessionId);

        if (!ok)
        {
            GameLogger.Traffic.Warning("Handshake inválido, cerrando conexión");
            Dispose();
            return;
        }

        _lastKeepAliveId = keepAliveId;
        SessionId = sessionId;
        _handshakeCompleted = true;

        GameLogger.Traffic.Information("Handshake OK (sessionId={SessionId})", SessionId);

        await SendAsync("OK");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_characterName is not null)
            {
                WorldSessionRegistry.Unregister(_characterName);
            }

            _dbContext.Dispose();
        }

        base.Dispose(disposing);
    }
}
