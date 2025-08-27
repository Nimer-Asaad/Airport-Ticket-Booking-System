using AirportTicketBooking.Application.DTOs;
using AirportTicketBooking.Application.Services;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Infrastructure.FileSystem;
using AirportTicketBooking.Infrastructure.Repositories;
using AirportTicketBooking.Shared.Helpers;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var settings = AppSettings.Load();
var paths = new DataPaths(settings);

DataSeeder.EnsureFilesExist(paths);

DataSeeder.SeedSampleData(paths);

var flightRepo = new JsonRepository<Flight, string>(paths.Flights);
var passengerRepo = new JsonRepository<Passenger, Guid>(paths.Passengers);
var bookingRepo = new JsonRepository<Booking, Guid>(paths.Bookings);

Console.WriteLine("== Storage smoke test ==");
Console.WriteLine($"Flights   : {flightRepo.GetAll().Count}");
Console.WriteLine($"Passengers: {passengerRepo.GetAll().Count}");
Console.WriteLine($"Bookings  : {bookingRepo.GetAll().Count}");


var flightService = new FlightService(flightRepo);

var matches = flightService.Search(new FlightSearchParams
{
    FromAirport = "AMM",
    Class = SeatClass.Economy
});

Console.WriteLine($"Search results: {matches.Count}");
foreach (var (f, price) in matches)
    Console.WriteLine($" - {f.Code} {f.DepartureAirport}->{f.ArrivalAirport} on {f.DepartureUtc:yyyy-MM-dd} | {price}$");

var bookingService = new BookingService(flightRepo, passengerRepo, bookingRepo);

var passenger = passengerRepo.GetAll().FirstOrDefault();
var flight = flightRepo.GetAll().FirstOrDefault();

if (passenger is not null && flight is not null)
{
    Console.WriteLine("\n== Booking smoke test ==");

    var booking = bookingService.Book(new BookingRequest
    {
        PassengerId = passenger.Id,
        FlightCode = flight.Code,
        Class = SeatClass.Economy,
        SeatCount = 2
    });
    Console.WriteLine($"Booked: {booking.Id} | {booking.FlightCode} | {booking.SeatClass} x{booking.SeatCount} → {booking.TotalPrice}$");

    booking = bookingService.Modify(new ModifyBookingRequest
    {
        BookingId = booking.Id,
        NewClass = SeatClass.Business
    });
    Console.WriteLine($"Modified: {booking.Id} | {booking.SeatClass} x{booking.SeatCount} → {booking.TotalPrice}$");

    var mine = bookingService.GetMyBookings(passenger.Id);
    Console.WriteLine($"My bookings: {mine.Count}");

    var cancelled = bookingService.Cancel(booking.Id);
    Console.WriteLine($"Cancelled? {cancelled}");
}
else
{
    Console.WriteLine("No passenger/flight found. Make sure seeding created initial data.");
}


// ===== Manager: Import flights from CSV =====
var importer = new CsvFlightImporter(flightRepo);
var csvPath = Path.Combine(AppContext.BaseDirectory, "sample_import.csv"); // عدّل المسار حسب مكان الملف
if (File.Exists(csvPath))
{
    var importResult = importer.Import(csvPath);
    Console.WriteLine($"\n== CSV Import ==");
    Console.WriteLine($"Inserted: {importResult.Inserted}");
    if (importResult.HasErrors)
    {
        Console.WriteLine("Errors:");
        foreach (var e in importResult.Errors)
            Console.WriteLine($"  Row {e.Row} | {e.Field}: {e.Message}");
    }
}
else
{
    Console.WriteLine("\n(sample_import.csv not found — skip import test)");
}

// ===== Manager: Dynamic validation details =====
Console.WriteLine("\n== Flight Model Validation Details ==");
var details = ManagerService.DescribeFlightModel();
foreach (var d in details)
{
    Console.WriteLine($"- {d.Field}");
    Console.WriteLine($"  Type: {d.Type}");
    Console.WriteLine($"  Constraints: {string.Join(", ", d.Constraints)}");
}