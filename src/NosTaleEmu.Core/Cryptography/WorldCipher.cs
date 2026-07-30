using System.Text;

namespace NosTaleEmu.Core.Cryptography;

/// <summary>
/// Cifrado usado por el canal de juego. Combina un desplazamiento dependiente
/// de la sesión con una codificación tipo RLE por segmentos (separados por 0xFF).
/// </summary>
public sealed class WorldCipher : IPacketCipher
{
    private const int ChunkSize = 0x7E;
    private const byte SegmentEnd = 0xFF;

    // Tabla de símbolos usada para des-empaquetar los "nibbles" de los
    // segmentos numéricos (fechas, cantidades, coordenadas, etc.).
    private static readonly char[] NibbleSymbols =
    {
        ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n'
    };

    public byte[] Encrypt(string rawPacket)
    {
        byte[] payload = Encoding.Default.GetBytes(rawPacket);
        int chunkCount = (int)Math.Ceiling(payload.Length / (double)ChunkSize);
        byte[] output = new byte[payload.Length + chunkCount + 1];

        int outIndex = 0;
        for (int i = 0; i < payload.Length; i++)
        {
            if (i % ChunkSize == 0)
            {
                int remaining = payload.Length - i;
                output[outIndex++] = (byte)Math.Min(remaining, ChunkSize);
            }

            output[outIndex++] = unchecked((byte)~payload[i]);
        }

        output[^1] = SegmentEnd;
        return output;
    }

    /// <summary>
    /// Descifra el buffer recibido del socket. El resultado puede contener
    /// varios paquetes del juego pegados en un mismo string, separados por
    /// (char)0xFF — el llamador (ver <see cref="Networking.ClientSessionBase"/>)
    /// es responsable de partirlos antes de procesarlos uno por uno.
    /// </summary>
    public string Decrypt(byte[] rawBytes, int sessionId = 0)
    {
        string shifted = ApplySessionShift(rawBytes, sessionId);
        return DecodeSegments(shifted);
    }

    /// <summary>
    /// Aplica el desplazamiento (suma/resta + XOR opcional) determinado por
    /// los bits altos del identificador de sesión.
    /// </summary>
    private static string ApplySessionShift(byte[] rawBytes, int sessionId)
    {
        byte key = unchecked((byte)((sessionId & 0xFF) + 0x40));
        int mode = ((byte)(sessionId >> 6)) & 0x03;

        var builder = new StringBuilder(rawBytes.Length);
        foreach (byte b in rawBytes)
        {
            byte shifted = mode switch
            {
                0 => unchecked((byte)(b - key)),
                1 => unchecked((byte)(b + key)),
                2 => unchecked((byte)((b - key) ^ 0xC3)),
                3 => unchecked((byte)((b + key) ^ 0xC3)),
                _ => 0x0F
            };
            builder.Append((char)shifted);
        }

        return builder.ToString();
    }

    private static string DecodeSegments(string shifted)
    {
        string[] segments = shifted.Split((char)SegmentEnd);
        var result = new StringBuilder();

        for (int i = 0; i < segments.Length; i++)
        {
            result.Append(DecryptSegment(segments[i]));

            if (i < segments.Length - 2)
            {
                result.Append((char)SegmentEnd);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Decodifica un único segmento crudo (sin el desplazamiento de sesión),
    /// es decir, la parte de la codificación tipo RLE que empaqueta texto
    /// literal y símbolos numéricos por nibble. Útil cuando ya tenés el
    /// segmento aislado (por ejemplo, un parámetro suelto de un paquete) y
    /// no necesitás pasar por <see cref="Decrypt"/> completo.
    /// </summary>
    public static string DecryptSegment(string segment)
    {
        var decoded = new List<byte>(segment.Length);
        int pos = 0;

        while (pos < segment.Length)
        {
            char marker = segment[pos];

            if (marker <= 0x7A)
            {
                // Bloque "literal": los siguientes <marker> bytes van complementados con XOR 0xFF.
                int literalLength = marker;
                for (int i = 0; i < literalLength; i++)
                {
                    pos++;
                    byte value = pos < segment.Length ? (byte)(segment[pos] ^ 0xFF) : (byte)255;
                    decoded.Add(value);
                }
            }
            else
            {
                // Bloque "empaquetado": cada byte contiene dos símbolos (nibble alto/bajo).
                int symbolsToDecode = marker & 0x7F;
                int symbolsDecoded = 0;

                while (symbolsDecoded < symbolsToDecode)
                {
                    pos++;
                    int raw = pos < segment.Length ? segment[pos] : 0;
                    int high = (raw & 0xF0) >> 4;
                    int low = raw & 0x0F;

                    if (high != 0x0 && high != 0xF)
                    {
                        decoded.Add((byte)NibbleSymbols[high - 1]);
                        symbolsDecoded++;
                    }

                    if (low != 0x0 && low != 0xF)
                    {
                        decoded.Add((byte)NibbleSymbols[low - 1]);
                    }

                    symbolsDecoded++;
                }
            }

            pos++;
        }

        byte[] rawBytes = decoded.ToArray();
        return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, rawBytes));
    }

    public string DecryptCustomParameter(byte[] data)
    {
        try
        {
            var builder = new StringBuilder();

            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] == 0x0E)
                {
                    return builder.ToString();
                }

                int value = data[i] - 0x0F;
                int high = (value & 0xF0) >> 4;
                int low = value & 0x0F;

                builder.Append(MapNibbleToChar(high));
                builder.Append(MapNibbleToChar(low));
            }

            return builder.ToString();
        }
        catch (OverflowException)
        {
            return string.Empty;
        }
    }

    private static char MapNibbleToChar(int nibble) => nibble switch
    {
        0 or 1 => ' ',
        2 => '-',
        3 => '.',
        _ => (char)(nibble + 0x2C)
    };
}
