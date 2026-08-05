namespace NosTaleEmu.Core.Cryptography;

public interface IPacketCipher
{
    byte[] Encrypt(string rawPacket);

    string Decrypt(byte[] rawBytes, int sessionId = 0);
}
