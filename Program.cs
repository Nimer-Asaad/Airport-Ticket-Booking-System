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
