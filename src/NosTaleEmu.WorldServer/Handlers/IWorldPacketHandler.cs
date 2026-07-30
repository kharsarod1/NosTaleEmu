namespace NosTaleEmu.WorldServer.Handlers;

/// <summary>
/// Maneja un paquete específico del World (identificado por su header, ej.
/// "c_list"). Para agregar soporte a un paquete nuevo: creá una clase que
/// implemente esta interfaz — se registra sola, no hace falta tocar
/// WorldSession ni ningún switch.
/// </summary>
public interface IWorldPacketHandler
{
    /// <summary>Header del paquete que maneja. Debe ser único.</summary>
    string Header { get; }

    /// <param name="args">Los campos del paquete después del header.</param>
    Task HandleAsync(WorldSession session, string[] args, CancellationToken cancellationToken);
}
