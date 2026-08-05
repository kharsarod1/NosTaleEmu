using System.Net.Sockets;
using NosTaleEmu.Core.Cryptography;
using NosTaleEmu.Core.Networking;
using NosTaleEmu.Database;
using NosTaleEmu.Services.Account;
using Serilog;

namespace NosTaleEmu.LoginServer;

public sealed class LoginSession : ClientSessionBase
{
    private readonly IReadOnlyList<WorldChannel> _channels;
    private readonly AccountService _accountService;
    private readonly LoginDbContext _dbContext;

    public LoginSession(
        TcpClient tcpClient,
        IReadOnlyList<WorldChannel> channels,
        LoginDbContext dbContext)
        : base(tcpClient, new LoginCipher())
    {
        _channels = channels;
        _dbContext = dbContext;
        _accountService = new AccountService(dbContext);
    }

    protected override async Task OnPacketReceivedAsync(string packet, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(packet))
        {
            return;
        }

        Log.Debug("<< {Packet}", packet);

        string[] parts = packet.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4 || parts[0] != "NoS0575")
        {
            await SendAsync("failc 3");
            return;
        }

        string username = parts[2];
        string passwordHash = parts[3];

        bool valid;
        try
        {
            valid = await _accountService.ValidateCredentialsAsync(username, passwordHash, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error consultando la base de datos al autenticar '{Username}'", username);
            await SendAsync("failc 2");
            return;
        }

        if (!valid)
        {
            Log.Warning("Login fallido para '{Username}' (credenciales inválidas o cuenta baneada)", username);
            await SendAsync("failc 1");
            return;
        }

        int sessionId = Random.Shared.Next(1, ushort.MaxValue);
        string channelList = WorldChannelListBuilder.Build(_channels);

        await SendAsync($"NsTeST {username} {sessionId} {channelList}");

        Log.Information("'{Username}' autenticado (sessionId={SessionId})", username, sessionId);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Dispose();
        }

        base.Dispose(disposing);
    }
}
