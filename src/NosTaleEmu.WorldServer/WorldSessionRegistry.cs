using System.Collections.Concurrent;

namespace NosTaleEmu.WorldServer;

public static class WorldSessionRegistry
{
    private static readonly ConcurrentDictionary<string, WorldSession> Sessions = new(StringComparer.OrdinalIgnoreCase);

    public static void Register(string characterName, WorldSession session) =>
        Sessions[characterName] = session;

    public static void Unregister(string characterName) =>
        Sessions.TryRemove(characterName, out _);

    public static bool TryGet(string characterName, out WorldSession? session) =>
        Sessions.TryGetValue(characterName, out session);

    public static int ConnectedCount => Sessions.Count;
}
