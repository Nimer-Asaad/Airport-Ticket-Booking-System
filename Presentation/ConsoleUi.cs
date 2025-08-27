using System.Globalization;
using AirportTicketBooking.Application.Contracts;
using AirportTicketBooking.Application.DTOs;
using AirportTicketBooking.Application.Services;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;
using AirportTicketBooking.Infrastructure.FileSystem;

namespace AirportTicketBooking.Presentation
{
    public static class ConsoleUi
    {
        public static void Run(
            IRepository<Flight, string> flightRepo,
            IRepository<Passenger, Guid> passengerRepo,
            IRepository<Booking, Guid> bookingRepo,
            DataPaths paths)
        {
            var flightService = new FlightService(flightRepo);
            var bookingService = new BookingService(flightRepo, passengerRepo, bookingRepo);
            var managerFilter = new ManagerBookingService(bookingRepo, flightRepo, passengerRepo);
            var importer = new CsvFlightImporter(flightRepo);

            while (true)
            {
                Console.WriteLine("\n=== Airport Ticket Booking ===");
                Console.WriteLine("1) Passenger");
                Console.WriteLine("2) Manager");
                Console.WriteLine("0) Exit");
                Console.Write("Choose: ");
                var ch = Console.ReadLine();

                if (ch == "1") PassengerMenu(flightService, bookingService, passengerRepo);
                else if (ch == "2") ManagerMenu(managerFilter, importer);
                else if (ch == "0") break;
                else Console.WriteLine("Invalid choice.");
            }
        }

        // ================= PASSENGER =================
        private static void PassengerMenu(
            FlightService flightService,
            BookingService bookingService,
            IRepository<Passenger, Guid> passengerRepo)
        {
            var passenger = SelectPassenger(passengerRepo);
            if (passenger is null) return;

            while (true)
            {
                Console.WriteLine($"\n=== Passenger: {passenger.Name} ===");
                Console.WriteLine("1) Search & Book Flight");
                Console.WriteLine("2) View My Bookings");
                Console.WriteLine("3) Modify Booking");
                Console.WriteLine("4) Cancel Booking");
                Console.WriteLine("0) Back");
                Console.Write("Choose: ");
                var ch = Console.ReadLine();

                if (ch == "1")
                {
                    var p = ReadFlightSearchParams();
                    var results = flightService.Search(p);
                    if (results.Count == 0) { Console.WriteLine("No flights found."); continue; }
                    PrintFlights(results);

                    Console.Write("Enter flight code to book: ");
                    var code = Console.ReadLine() ?? "";
                    var sel = results.FirstOrDefault(x => x.flight.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                    if (sel.flight is null) { Console.WriteLine("Invalid code."); continue; }

                    var cls = ReadSeatClass(p.Class ?? SeatClass.Economy);
                    var count = ReadInt("Seat count", 1, 20);

                    try
                    {
                        var b = bookingService.Book(new BookingRequest
                        {
                            PassengerId = passenger.Id,
                            FlightCode = sel.flight.Code,
                            Class = cls,
                            SeatCount = count
                        });
                        Console.WriteLine($"Booked ✅ {b.Id} | {b.FlightCode} | {b.SeatClass} x{b.SeatCount} → {b.TotalPrice}$");
                    }
                    catch (Exception ex) { Console.WriteLine("❌ " + ex.Message); }
                }
                else if (ch == "2")
                {
                    var mine = bookingService.GetMyBookings(passenger.Id);
                    PrintBookings(mine);
                }
                else if (ch == "3")
                {
                    var mine = bookingService.GetMyBookings(passenger.Id);
                    if (mine.Count == 0) { Console.WriteLine("No bookings."); continue; }
                    PrintBookings(mine);

                    var id = ReadGuid("Booking Id");
                    var newCls = ReadSeatClassNullable();
                    int? newCnt = AskYesNo("Change seat count? (y/n)") ? ReadInt("New seat count", 1, 20) : null;

                    try
                    {
                        var updated = bookingService.Modify(new ModifyBookingRequest
                        {
                            BookingId = id,
                            NewClass = newCls,
                            NewSeatCount = newCnt
                        });
                        Console.WriteLine($"Updated ✅ {updated.Id} | {updated.SeatClass} x{updated.SeatCount} → {updated.TotalPrice}$");
                    }
                    catch (Exception ex) { Console.WriteLine("❌ " + ex.Message); }
                }
                else if (ch == "4")
                {
                    var id = ReadGuid("Booking Id");
                    Console.WriteLine(bookingService.Cancel(id) ? "Cancelled ✅" : "Not found.");
                }
                else if (ch == "0") break;
            }
        }

        private static Passenger? SelectPassenger(IRepository<Passenger, Guid> repo)
        {
            var all = repo.GetAll();
            if (all.Count == 0) { Console.WriteLine("No passengers."); return null; }
            Console.WriteLine("\nPassengers:");
            foreach (var p in all) Console.WriteLine($"{p.Id} | {p.Name} | {p.Email}");
            return repo.GetById(ReadGuid("Passenger Id"));
        }

        // ================ MANAGER ================
        private static void ManagerMenu(
            ManagerBookingService managerFilter,
            CsvFlightImporter importer)
        {
            while (true)
            {
                Console.WriteLine("\n=== Manager ===");
                Console.WriteLine("1) Filter Bookings");
                Console.WriteLine("2) Import Flights from CSV");
                Console.WriteLine("3) Show Flight Model Validation Details");
                Console.WriteLine("0) Back");
                Console.Write("Choose: ");
                var ch = Console.ReadLine();

                if (ch == "1")
                {
                    var p = ReadBookingFilterParams();
                    var items = managerFilter.Filter(p);
                    if (items.Count == 0) { Console.WriteLine("No results."); continue; }
                    foreach (var (b, f, u) in items)
                        Console.WriteLine($"Booking {b.Id} | Flight {f.Code} {f.DepartureAirport}->{f.ArrivalAirport} | Passenger: {u.Name} | {b.SeatClass} x{b.SeatCount} | {b.TotalPrice}$ | {b.Status}");
                }
                else if (ch == "2")
                {
                    Console.Write("CSV path (default: sample_import.csv): ");
                    var path = Console.ReadLine();
                    var csv = string.IsNullOrWhiteSpace(path)
                        ? Path.Combine(AppContext.BaseDirectory, "sample_import.csv")
                        : path!;

                    var res = importer.Import(csv);
                    Console.WriteLine($"Inserted: {res.Inserted}");
                    if (res.HasErrors)
                    {
                        Console.WriteLine("Errors:");
                        foreach (var e in res.Errors)
                            Console.WriteLine($"  Row {e.Row} | {e.Field}: {e.Message}");
                    }
                }
                else if (ch == "3")
                {
                    Console.WriteLine("\n== Flight Model Validation Details ==");
                    var details = ManagerService.DescribeFlightModel();
                    foreach (var d in details)
                    {
                        Console.WriteLine($"- {d.Field}");
                        Console.WriteLine($"  Type: {d.Type}");
                        Console.WriteLine($"  Constraints: {string.Join(", ", d.Constraints)}");
                    }
                }
                else if (ch == "0") break;
            }
        }

        // =============== Helpers ===============
        private static void PrintFlights(IReadOnlyList<(Flight flight, decimal price)> items)
        {
            foreach (var (f, price) in items)
                Console.WriteLine($"{f.Code} {f.DepartureAirport}->{f.ArrivalAirport} {f.DepartureUtc:yyyy-MM-dd} | {price}$");
        }

        private static void PrintBookings(IReadOnlyList<Booking> list)
        {
            if (list.Count == 0) { Console.WriteLine("No bookings."); return; }
            foreach (var b in list)
                Console.WriteLine($"{b.Id} | {b.FlightCode} | {b.SeatClass} x{b.SeatCount} | {b.TotalPrice}$ | {b.Status}");
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

        private static FlightSearchParams ReadFlightSearchParams()
        {
            Console.Write("From Country: "); var fc = Console.ReadLine();
            Console.Write("To Country: "); var tc = Console.ReadLine();
            Console.Write("From Airport: "); var fa = Console.ReadLine();
            Console.Write("To Airport: "); var ta = Console.ReadLine();
            Console.Write("Departure Date (yyyy-MM-dd): "); var d = Console.ReadLine();
            Console.Write("Max Price: "); var mp = Console.ReadLine();
            var cls = ReadSeatClassNullable();

            DateTime? date = (!string.IsNullOrWhiteSpace(d) && DateTime.TryParse(d, out var dd)) ? dd.Date : null;
            decimal? maxP = (!string.IsNullOrWhiteSpace(mp) && decimal.TryParse(mp, NumberStyles.Any, CultureInfo.InvariantCulture, out var m)) ? m : null;

            return new FlightSearchParams
            {
                FromCountry = NullIfEmpty(fc),
                ToCountry = NullIfEmpty(tc),
                FromAirport = NullIfEmpty(fa),
                ToAirport = NullIfEmpty(ta),
                DepartureDateUtc = date,
                MaxPrice = maxP,
                Class = cls
            };
        }

        private static BookingFilterParams ReadBookingFilterParams()
        {
            Console.Write("Flight Code: "); var fc = Console.ReadLine();
            Console.Write("Passenger Id (blank to skip): "); var pid = Console.ReadLine();
            Guid? guid = Guid.TryParse(pid, out var g) ? g : null;

            return new BookingFilterParams
            {
                FlightCode = NullIfEmpty(fc),
                PassengerId = guid
            };
        }

        private static Guid ReadGuid(string label)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                var s = Console.ReadLine();
                if (Guid.TryParse(s, out var g)) return g;
                Console.WriteLine("Invalid GUID.");
            }
        }

        private static int ReadInt(string label, int min, int max)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out var i) && i >= min && i <= max) return i;
                Console.WriteLine($"Enter number between {min} and {max}.");
            }
        }

        private static bool AskYesNo(string label)
        {
            Console.Write(label + " ");
            var s = Console.ReadLine();
            return s != null && s.Trim().ToLower().StartsWith("y");
        }

        private static SeatClass ReadSeatClass(SeatClass def)
        {
            Console.Write("Seat Class (E=Economy, B=Business, F=First) [default E]: ");
            var s = Console.ReadLine()?.Trim().ToUpperInvariant();
            return s switch
            {
                "B" => SeatClass.Business,
                "F" => SeatClass.First,
                _ => def
            };
        }

        private static SeatClass? ReadSeatClassNullable()
        {
            Console.Write("Seat Class (E=Economy, B=Business, F=First, empty=skip): ");
            var s = Console.ReadLine()?.Trim().ToUpperInvariant();
            return s switch
            {
                "E" => SeatClass.Economy,
                "B" => SeatClass.Business,
                "F" => SeatClass.First,
                "" => (SeatClass?)null,
                null => (SeatClass?)null,
                _ => (SeatClass?)null
            };
        }
    }
}
