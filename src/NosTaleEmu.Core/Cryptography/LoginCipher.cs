using System.Text;

namespace NosTaleEmu.Core.Cryptography;

/// <summary>
/// Cifrado simétrico simple usado por el canal de autenticación.
/// Cada byte se desplaza +15 al cifrar; al descifrar se revierte el
/// desplazamiento y se aplica un XOR fijo (0xC3).
/// </summary>
public sealed class LoginCipher : IPacketCipher
{
    private const byte Shift = 15;
    private const byte XorMask = 0xC3;
    private const byte Terminator = 0x19;

    public byte[] Encrypt(string rawPacket)
    {
        try
        {
            string payload = rawPacket + " ";
            byte[] buffer = Encoding.Default.GetBytes(payload);

            for (int i = 0; i < buffer.Length - 1; i++)
            {
                buffer[i] = unchecked((byte)(buffer[i] + Shift));
            }

            buffer[^1] = Terminator;
            return buffer;
        }
        catch
        {
            return [];
        }
    }

    public string Decrypt(byte[] rawBytes, int sessionId = 0)
    {
        try
        {
            var builder = new StringBuilder(rawBytes.Length);

            foreach (byte b in rawBytes)
            {
                // (b - 15) mod 256, luego XOR 0xC3. Al trabajar en modulo 256
                // ambos casos (b > 14 y b <= 14) de la versión original colapsan
                // en una única expresión.
                byte shifted = unchecked((byte)(b - Shift));
                builder.Append((char)(shifted ^ XorMask));
            }

            return builder.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Extrae la contraseña en claro de la ofuscación hexadecimal reversible
    /// que usaban clientes NosTale antiguos. Los clientes actuales en cambio
    /// mandan directamente SHA-512(password) en hex (128 caracteres) — eso NO
    /// es reversible, así que si tu paquete trae un string de 128 hex chars,
    /// no uses este método: compará el hash contra el hash guardado en tu
    /// base de cuentas (ver LoginServer/Program.cs).
    /// </summary>
    public static string DecodePassword(string obfuscatedPassword)
    {
        int offset = obfuscatedPassword.Length % 2 == 0 ? 3 : 4;
        string hex = ExtractEvenChars(obfuscatedPassword.Remove(0, offset));

        if (hex.Length % 2 != 0)
        {
            hex = ExtractEvenChars(obfuscatedPassword.Remove(0, 2));
        }

        var sb = new StringBuilder(hex.Length / 2);
        for (int i = 0; i < hex.Length; i += 2)
        {
            sb.Append((char)Convert.ToUInt32(hex.Substring(i, 2), 16));
        }

        return sb.ToString();
    }

    private static string ExtractEvenChars(string source)
    {
        var sb = new StringBuilder(source.Length / 2 + 1);
        for (int i = 0; i < source.Length; i += 2)
        {
            sb.Append(source[i]);
        }
        return sb.ToString();
    }
}
