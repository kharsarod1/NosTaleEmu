using System.Net.Sockets;
using NosTaleEmu.Core.Cryptography;

namespace NosTaleEmu.Core.Networking;

public abstract class ClientSessionBase : IDisposable
{
    protected readonly TcpClient TcpClient;
    protected readonly NetworkStream Stream;
    protected readonly IPacketCipher Cipher;

    public int SessionId { get; protected set; }
    public bool IsConnected => TcpClient.Connected;

    protected ClientSessionBase(TcpClient tcpClient, IPacketCipher cipher, int sessionId = 0)
    {
        TcpClient = tcpClient;
        Stream = tcpClient.GetStream();
        Cipher = cipher;
        SessionId = sessionId;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[8192];

        try
        {
            while (!cancellationToken.IsCancellationRequested && TcpClient.Connected)
            {
                int read = await Stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read <= 0)
                {
                    break;
                }

                byte[] received = buffer[..read];
                string decrypted = DecryptIncoming(received);

                foreach (string packet in SplitPackets(decrypted))
                {
                    await OnPacketReceivedAsync(packet, cancellationToken);
                }
            }
        }
        catch (IOException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Dispose();
        }
    }

    protected abstract Task OnPacketReceivedAsync(string packet, CancellationToken cancellationToken);

    protected virtual string DecryptIncoming(byte[] raw) => Cipher.Decrypt(raw, SessionId);

    protected virtual IEnumerable<string> SplitPackets(string decrypted) =>
        decrypted.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public async Task SendAsync(string packet)
    {
        if (!TcpClient.Connected)
        {
            return;
        }

        byte[] payload = Cipher.Encrypt(packet);
        await Stream.WriteAsync(payload);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Stream.Dispose();
        TcpClient.Dispose();
    }
}
