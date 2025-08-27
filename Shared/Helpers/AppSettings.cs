using System.Text.Json;

namespace AirportTicketBooking.Shared.Helpers;

public sealed class AppSettings
{
    public DataFilesSection DataFiles { get; init; } = new();

    public sealed class DataFilesSection
    {
        public string Flights { get; init; } = "Data/flights.json";
        public string Bookings { get; init; } = "Data/bookings.json";
        public string Passengers { get; init; } = "Data/passengers.json";
    }

    public static AppSettings Load(string? baseDir = null)
    {
        baseDir ??= AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "appSettings.json");

        if (!File.Exists(path))
            path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "appSettings.json"));

        if (!File.Exists(path))
            throw new FileNotFoundException("appSettings.json was not found. Make sure it is copied to output.", path);

        var json = File.ReadAllText(path);
        var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return settings ?? new AppSettings();
    }
}
