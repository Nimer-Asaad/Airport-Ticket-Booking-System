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




ConsoleUi.Run(flightRepo, passengerRepo, bookingRepo, paths);
