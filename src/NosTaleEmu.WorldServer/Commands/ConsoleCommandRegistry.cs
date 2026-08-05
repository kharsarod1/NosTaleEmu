namespace NosTaleEmu.WorldServer.Commands;

public sealed class ConsoleCommandRegistry
{
    private readonly Dictionary<string, IConsoleCommand> _commands;

    public ConsoleCommandRegistry()
    {
        _commands = Discover();
    }

    public IReadOnlyCollection<IConsoleCommand> All => _commands.Values;

    public bool TryGet(string name, out IConsoleCommand? command) =>
        _commands.TryGetValue(name, out command);

    private static Dictionary<string, IConsoleCommand> Discover()
    {
        var commands = new Dictionary<string, IConsoleCommand>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Type> types = typeof(ConsoleCommandRegistry).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && typeof(IConsoleCommand).IsAssignableFrom(t)
                && t.GetConstructor(Type.EmptyTypes) is not null);

        foreach (Type type in types)
        {
            if (Activator.CreateInstance(type) is IConsoleCommand command)
            {
                commands.TryAdd(command.Name, command);
            }
        }

        return commands;
    }
}
