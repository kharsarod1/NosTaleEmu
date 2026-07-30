using System.Reflection;

namespace NosTaleEmu.WorldServer.Handlers;

/// <summary>
/// Escanea el ensamblado en busca de clases que implementen
/// <see cref="IWorldPacketHandler"/> y arma el mapa header -> handler.
/// Un handler nuevo se detecta solo con crear la clase, sin registrarlo a
/// mano en ningún lado.
/// </summary>
public sealed class WorldPacketHandlerRegistry
{
    private readonly Dictionary<string, IWorldPacketHandler> _handlers;

    public WorldPacketHandlerRegistry()
    {
        _handlers = DiscoverHandlers();
    }

    public bool TryGetHandler(string header, out IWorldPacketHandler? handler) =>
        _handlers.TryGetValue(header, out handler);

    private static Dictionary<string, IWorldPacketHandler> DiscoverHandlers()
    {
        var handlers = new Dictionary<string, IWorldPacketHandler>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Type> handlerTypes = typeof(WorldPacketHandlerRegistry).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && typeof(IWorldPacketHandler).IsAssignableFrom(t)
                && t.GetConstructor(Type.EmptyTypes) is not null);

        foreach (Type type in handlerTypes)
        {
            if (Activator.CreateInstance(type) is not IWorldPacketHandler handler)
            {
                continue;
            }

            if (!handlers.TryAdd(handler.Header, handler))
            {
                throw new InvalidOperationException(
                    $"Dos handlers registran el mismo header '{handler.Header}' ({type.Name}). Cada header debe ser único.");
            }
        }

        return handlers;
    }
}
