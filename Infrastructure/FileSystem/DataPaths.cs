using AirportTicketBooking.Shared.Helpers;

namespace AirportTicketBooking.Infrastructure.FileSystem;

public sealed class DataPaths
{
    public string Flights { get; }
    public string Bookings { get; }
    public string Passengers { get; }

    public DataPaths(AppSettings settings, string? baseDir = null)
    {
        baseDir ??= AppContext.BaseDirectory;

        Flights = Path.Combine(baseDir, settings.DataFiles.Flights);
        Bookings = Path.Combine(baseDir, settings.DataFiles.Bookings);
        Passengers = Path.Combine(baseDir, settings.DataFiles.Passengers);
    }
}
