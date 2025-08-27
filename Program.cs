//using AirportTicketBooking.Application.DTOs;
//using AirportTicketBooking.Application.Services;
using AirportTicketBooking.Domain.Entities;
//using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Infrastructure.FileSystem;
using AirportTicketBooking.Infrastructure.Repositories;
using AirportTicketBooking.Shared.Helpers;
using AirportTicketBooking.Presentation;


Console.OutputEncoding = System.Text.Encoding.UTF8;

var settings = AppSettings.Load();
var paths = new DataPaths(settings);

DataSeeder.EnsureFilesExist(paths);

DataSeeder.SeedSampleData(paths);

var flightRepo = new JsonRepository<Flight, string>(paths.Flights);
var passengerRepo = new JsonRepository<Passenger, Guid>(paths.Passengers);
var bookingRepo = new JsonRepository<Booking, Guid>(paths.Bookings);



// Program.cs (أضف بعد تهيئة الـ repos مباشرة)
if (args.Contains("--smoke"))
{
    // 1) Storage counts
    var flights = flightRepo.GetAll();
    var passengers = passengerRepo.GetAll();
    var bookings = bookingRepo.GetAll();
    Console.WriteLine("== Storage smoke test ==");
    Console.WriteLine($"Flights   : {flights.Count}");
    Console.WriteLine($"Passengers: {passengers.Count}");
    Console.WriteLine($"Bookings  : {bookings.Count}");

    // 2) ابحث عن RJ101 Economy
    var rj = flights.FirstOrDefault(f => f.Code == "RJ101");
    if (rj != null)
    {
        Console.WriteLine("Search results: 1");
        Console.WriteLine($" - {rj.Code} {rj.DepartureAirport}->{rj.ArrivalAirport} on {rj.DepartureUtc:yyyy-MM-dd} | {rj.EconomyPrice}$");
    }
    else
    {
        Console.WriteLine("Search results: 0");
    }

    // 3) احجز لراكب أول اثنين مقاعد Economy
    var passenger = passengers.First();
    var bookingSvc = new AirportTicketBooking.Application.Services.BookingService(flightRepo, passengerRepo, bookingRepo);
    var book = bookingSvc.Book(new AirportTicketBooking.Application.DTOs.BookingRequest
    {
        PassengerId = passenger.Id,
        FlightCode = "RJ101",
        Class = AirportTicketBooking.Domain.Enums.SeatClass.Economy,
        SeatCount = 2
    });
    Console.WriteLine("\n== Booking smoke test ==");
    Console.WriteLine($"Booked: {book.Id} | {book.FlightCode} | {book.SeatClass} x{book.SeatCount} → {book.TotalPrice}$");

    // 4) عدّل لـ Business
    var upd = bookingSvc.Modify(new AirportTicketBooking.Application.DTOs.ModifyBookingRequest
    {
        BookingId = book.Id,
        NewClass = AirportTicketBooking.Domain.Enums.SeatClass.Business
    });
    Console.WriteLine($"Modified: {upd.Id} | {upd.SeatClass} x{upd.SeatCount} → {upd.TotalPrice}$");

    // 5) اطبع حجوزاتي
    var mine = bookingSvc.GetMyBookings(passenger.Id);
    Console.WriteLine("My bookings:");
    foreach (var b in mine)
        Console.WriteLine($" - {b.Id} {b.FlightCode} {b.SeatClass} x{b.SeatCount} → {b.TotalPrice}$");

    return;
}



ConsoleUi.Run(flightRepo, passengerRepo, bookingRepo, paths);
