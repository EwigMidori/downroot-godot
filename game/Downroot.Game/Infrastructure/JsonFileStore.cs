using System.Text.Json;

namespace Downroot.Game.Infrastructure;

public sealed class JsonFileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SavePathResolver _paths;

    public JsonFileStore(SavePathResolver paths)
    {
        _paths = paths;
    }

    public T? Read<T>(string godotPath)
    {
        var path = _paths.Globalize(godotPath);
        if (!File.Exists(path))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions);
    }

    public void Write<T>(string godotPath, T value)
    {
        var path = _paths.Globalize(godotPath);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(value, JsonOptions));
        File.Move(tempPath, path, overwrite: true);
    }
}
