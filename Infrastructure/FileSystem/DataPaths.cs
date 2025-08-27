using AirportTicketBooking.Shared.Helpers;

namespace AirportTicketBooking.Infrastructure.FileSystem;

public sealed class DataPaths
{
    public string Flights { get; }
    public string Bookings { get; }
    public string Passengers { get; }

    public DataPaths(AppSettings settings, string? baseDir = null)
    {
        // ✅ خليك دايمًا على مجلد المشروع (…/Airport-Ticket-Booking-System/)
        // إذا وجد Data جنب السوليوشن استعمله، وإلا ارجع لـ BaseDirectory
        var projectRoot = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        var preferProjectData = System.IO.Directory.Exists(
            System.IO.Path.Combine(projectRoot, "Data"));

        var root = preferProjectData ? projectRoot : AppContext.BaseDirectory;

        Flights = System.IO.Path.Combine(root, settings.DataFiles.Flights);
        Bookings = System.IO.Path.Combine(root, settings.DataFiles.Bookings);
        Passengers = System.IO.Path.Combine(root, settings.DataFiles.Passengers);
    }
}
