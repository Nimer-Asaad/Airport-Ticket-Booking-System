using AirportTicketBooking.Shared.Helpers;
using AirportTicketBooking.Infrastructure.FileSystem;
using AirportTicketBooking.Infrastructure.Repositories;
using AirportTicketBooking.Domain.Entities;

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
