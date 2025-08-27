using AirportTicketBooking.Application.Contracts;
using AirportTicketBooking.Application.DTOs;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Application.Services;

public sealed class ManagerBookingService
{
    private readonly IRepository<Booking, Guid> _bookings;
    private readonly IRepository<Flight, string> _flights;
    private readonly IRepository<Passenger, Guid> _passengers;

    public ManagerBookingService(
        IRepository<Booking, Guid> bookings,
        IRepository<Flight, string> flights,
        IRepository<Passenger, Guid> passengers)
    {
        _bookings = bookings;
        _flights = flights;
        _passengers = passengers;
    }

    public IReadOnlyList<(Booking booking, Flight flight, Passenger passenger)>
        Filter(BookingFilterParams p)
    {
        var bookings = _bookings.GetAll();
        var flights = _flights.GetAll().ToDictionary(f => f.Code, f => f);
        var passengers = _passengers.GetAll().ToDictionary(p => p.Id, p => p);

        var q = bookings.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(p.FlightCode))
            q = q.Where(b => b.FlightCode.Equals(p.FlightCode, StringComparison.OrdinalIgnoreCase));

        if (p.PassengerId.HasValue)
            q = q.Where(b => b.PassengerId == p.PassengerId);

        if (p.Class.HasValue)
            q = q.Where(b => b.SeatClass == p.Class);

        if (p.MinPrice.HasValue)
            q = q.Where(b => b.TotalPrice >= p.MinPrice.Value);

        if (p.MaxPrice.HasValue)
            q = q.Where(b => b.TotalPrice <= p.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(p.FromCountry))
            q = q.Where(b => flights.TryGetValue(b.FlightCode, out var f) &&
                             f.DepartureCountry.Equals(p.FromCountry, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(p.ToCountry))
            q = q.Where(b => flights.TryGetValue(b.FlightCode, out var f) &&
                             f.DestinationCountry.Equals(p.ToCountry, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(p.FromAirport))
            q = q.Where(b => flights.TryGetValue(b.FlightCode, out var f) &&
                             f.DepartureAirport.Equals(p.FromAirport, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(p.ToAirport))
            q = q.Where(b => flights.TryGetValue(b.FlightCode, out var f) &&
                             f.ArrivalAirport.Equals(p.ToAirport, StringComparison.OrdinalIgnoreCase));

        if (p.DepartureDateUtc.HasValue)
        {
            var d = p.DepartureDateUtc.Value.Date;
            q = q.Where(b => flights.TryGetValue(b.FlightCode, out var f) &&
                             f.DepartureUtc.Date == d);
        }

        return q
            .Select(b => (b,
                flights.TryGetValue(b.FlightCode, out var f) ? f : null!,
                passengers.TryGetValue(b.PassengerId, out var psg) ? psg : null!))
            .ToList();
    }
}
