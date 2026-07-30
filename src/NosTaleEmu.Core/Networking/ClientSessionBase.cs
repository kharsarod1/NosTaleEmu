using System.Net.Sockets;
using NosTaleEmu.Core.Cryptography;

namespace NosTaleEmu.Core.Networking;

/// <summary>
/// Maneja la lectura/escritura cruda de un socket y delega el
/// procesamiento de cada paquete ya descifrado al servidor concreto.
/// </summary>
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
            // Conexión cerrada por el cliente.
        }
        catch (OperationCanceledException)
        {
            // Servidor deteniéndose.
        }
        finally
        {
            Dispose();
        }
    }

    protected abstract Task OnPacketReceivedAsync(string packet, CancellationToken cancellationToken);

    /// <summary>
    /// Convierte los bytes crudos leídos del socket en texto descifrado.
    /// Por defecto usa <see cref="Cipher"/> con el <see cref="SessionId"/>
    /// actual, pero se puede sobreescribir (ej: WorldServer necesita usar el
    /// descifrado de "parámetro especial" para el primer paquete, antes de
    /// que el handshake establezca el sessionId real).
    /// </summary>
    protected virtual string DecryptIncoming(byte[] raw) => Cipher.Decrypt(raw, SessionId);

    /// <summary>
    /// Separa el texto ya descifrado en paquetes individuales. Un solo
    /// receive de socket puede traer varios paquetes pegados (ej: World los
    /// separa con 0xFF); por defecto se asume uno por línea.
    /// </summary>
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

    /// <summary>
    /// Sobreescribí esto para liberar recursos propios de la subclase (ej:
    /// LoginSession libera su DbContext acá). Llamá siempre a base.Dispose(disposing).
    /// </summary>
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
