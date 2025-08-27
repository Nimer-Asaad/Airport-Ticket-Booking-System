using System.Text.Json;

namespace AirportTicketBooking.Shared.Helpers;

public static class JsonFile
{
    private static readonly object _lock = new();

    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true   
    };

    public static List<T> ReadList<T>(string path)
    {
        if (!File.Exists(path)) return new List<T>();
        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json)) return new List<T>();
        return JsonSerializer.Deserialize<List<T>>(json, _opts) ?? new List<T>();
    }

    public static void WriteList<T>(string path, IEnumerable<T> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + ".tmp";

        lock (_lock)
        {
            File.WriteAllText(tmp, JsonSerializer.Serialize(items, _opts));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
    }
}
