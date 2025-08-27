using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Shared.Helpers;

namespace AirportTicketBooking.Infrastructure.FileSystem;

public static class DataSeeder
{
    public static void EnsureFilesExist(DataPaths paths)
    {
        if (!File.Exists(paths.Flights)) JsonFile.WriteList<Flight>(paths.Flights, new List<Flight>());
        if (!File.Exists(paths.Bookings)) JsonFile.WriteList<Booking>(paths.Bookings, new List<Booking>());
        if (!File.Exists(paths.Passengers)) JsonFile.WriteList<Passenger>(paths.Passengers, new List<Passenger>());
    }

    public static void SeedSampleData(DataPaths paths)
    {
        var flights = JsonFile.ReadList<Flight>(paths.Flights);
        if (flights.Count == 0)
        {
            var today = DateTime.UtcNow.Date;

            flights = new List<Flight>
            {
                new Flight
                {
                    Code = "RJ101",
                    DepartureCountry = "Jordan",  DestinationCountry = "UAE",
                    DepartureAirport = "AMM",      ArrivalAirport = "DXB",
                    DepartureUtc = today.AddDays(7).AddHours(9),
                    ArrivalUtc   = today.AddDays(7).AddHours(12),
                    EconomyPrice = 180, BusinessPrice = 540, FirstPrice = 980,
                    EconomySeats = 120, BusinessSeats = 18, FirstSeats = 8
                },
                new Flight
                {
                    Code = "TK900",
                    DepartureCountry = "Jordan",  DestinationCountry = "Turkey",
                    DepartureAirport = "AMM",      ArrivalAirport = "IST",
                    DepartureUtc = today.AddDays(14).AddHours(13),
                    ArrivalUtc   = today.AddDays(14).AddHours(16),
                    EconomyPrice = 220, BusinessPrice = 360, FirstPrice = 520,
                    EconomySeats = 140, BusinessSeats = 24, FirstSeats = 8
                }
            };

            JsonFile.WriteList(paths.Flights, flights);
        }

        var passengers = JsonFile.ReadList<Passenger>(paths.Passengers);
        if (passengers.Count == 0)
        {
            passengers = new List<Passenger>
            {
                new Passenger { Name = "Test User",   Email = "test@example.com" },
                new Passenger { Name = "Nimer Asaad", Email = "nimer@example.com" }
            };

            JsonFile.WriteList(paths.Passengers, passengers);
        }

        var bookings = JsonFile.ReadList<Booking>(paths.Bookings);
        if (bookings.Count == 0)
            JsonFile.WriteList(paths.Bookings, bookings);
    }
}
