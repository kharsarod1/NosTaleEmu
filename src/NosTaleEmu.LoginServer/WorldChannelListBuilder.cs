using System.Text;

namespace NosTaleEmu.LoginServer;

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
