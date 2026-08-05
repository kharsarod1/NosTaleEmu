using System.Net.Sockets;
using NosTaleEmu.Core.Cryptography;
using NosTaleEmu.Core.Networking;
using NosTaleEmu.Database;
using NosTaleEmu.Services.Account;

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

        Console.WriteLine($"[Login] << {packet}");

        // Formato esperado del cliente: NoS0575 <clientVersion> <username> <passwordHash> <extra...>
        string[] parts = packet.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4 || parts[0] != "NoS0575")
        {
            await SendAsync("failc 3"); // paquete no reconocido
            return;
        }

        string username = parts[2];
        string passwordHash = parts[3]; // SHA-512(password) en hex, tal cual lo manda el cliente

        bool valid;
        try
        {
            valid = await _accountService.ValidateCredentialsAsync(username, passwordHash, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Login] Error consultando la base de datos: {ex.Message}");
            await SendAsync("failc 2"); // error del servidor
            return;
        }

        if (!valid)
        {
            await SendAsync("failc 1"); // credenciales inválidas o cuenta baneada
            return;
        }

        int sessionId = Random.Shared.Next(1, ushort.MaxValue);
        string channelList = WorldChannelListBuilder.Build(_channels);

        // Respuesta de éxito: NsTeST <username> <sessionId> <lista de canales> -1:-1:-1:10000.10000.1
        await SendAsync($"NsTeST {username} {sessionId} {channelList}");

        Console.WriteLine(channelList);

        Console.WriteLine($"[Login] {username} autenticado, sessionId={sessionId}");
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
