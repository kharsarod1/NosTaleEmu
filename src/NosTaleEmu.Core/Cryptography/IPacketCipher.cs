namespace NosTaleEmu.Core.Cryptography;

/// <summary>
/// Contrato común para los cifradores de paquetes (Login / World).
/// </summary>
public interface IPacketCipher
{
    byte[] Encrypt(string rawPacket);

    string Decrypt(byte[] rawBytes, int sessionId = 0);
}
