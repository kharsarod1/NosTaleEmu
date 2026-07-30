using System.Text;

namespace NosTaleEmu.LoginServer;

/// <summary>
/// Arma el fragmento del paquete NsTeST que lista los canales disponibles.
/// Siempre termina con el marcador fijo "-1:-1:-1:10000.10000.1", que le
/// indica al cliente que ahí se acaba la lista.
/// </summary>
public static class WorldChannelListBuilder
{
    private const string EndOfListMarker = "-1:-1:-1:10000.10000.1";

    public static string Build(IEnumerable<WorldChannel> channels)
    {
        var builder = new StringBuilder();

        foreach (WorldChannel channel in channels)
        {
            builder.Append(channel.ToProtocolString());
            builder.Append(' ');
        }

        builder.Append(EndOfListMarker);
        return builder.ToString();
    }
}
