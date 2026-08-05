using System.Text.Json;
using System.Text.Json.Serialization;

namespace NosTaleEmu.Core.Configuration;

public static class JsonConfigLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static T LoadOrCreate<T>(string path, T defaults) where T : class
    {
        if (!File.Exists(path))
        {
            string defaultJson = JsonSerializer.Serialize(defaults, Options);
            File.WriteAllText(path, defaultJson);

            Console.WriteLine($"[Config] No existía '{path}', se creó con valores por defecto. Revisalo antes de conectar clientes reales.");

            return defaults;
        }

        string json = File.ReadAllText(path);
        T? loaded = JsonSerializer.Deserialize<T>(json, Options);

        if (loaded is null)
        {
            throw new InvalidOperationException($"No se pudo leer la configuración de '{path}': el archivo está vacío o mal formado.");
        }

        return loaded;
    }
}
