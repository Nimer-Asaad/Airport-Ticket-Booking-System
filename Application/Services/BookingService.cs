using AirportTicketBooking.Application.Contracts;
using AirportTicketBooking.Application.DTOs;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Application.Services;

public sealed class BookingService
{
    private readonly IRepository<Flight, string> _flights;
    private readonly IRepository<Passenger, Guid> _passengers;
    private readonly IRepository<Booking, Guid> _bookings;

    public BookingService(
        IRepository<Flight, string> flights,
        IRepository<Passenger, Guid> passengers,
        IRepository<Booking, Guid> bookings)
    {
        _flights = flights;
        _passengers = passengers;
        _bookings = bookings;
    }

    // ============ Helpers ============
    private static decimal PriceFor(Flight f, SeatClass c) => c switch
    {
        SeatClass.Economy => f.EconomyPrice,
        SeatClass.Business => f.BusinessPrice,
        SeatClass.First => f.FirstPrice,
        _ => f.EconomyPrice
    };

    private static int GetAvailable(Flight f, SeatClass c) => c switch
    {
        SeatClass.Economy => f.EconomySeats,
        SeatClass.Business => f.BusinessSeats,
        SeatClass.First => f.FirstSeats,
        _ => f.EconomySeats
    };

    private static void SetAvailable(Flight f, SeatClass c, int value)
    {
        switch (c)
        {
            case SeatClass.Economy: f.EconomySeats = value; break;
            case SeatClass.Business: f.BusinessSeats = value; break;
            case SeatClass.First: f.FirstSeats = value; break;
        }
    }

    private static void EnsureNotDeparted(Flight f)
    {
        if (DateTime.UtcNow >= f.DepartureUtc)
            throw new InvalidOperationException("Cannot modify bookings for a departed flight.");
    }

    // ============ Public API ============

    public Booking Book(BookingRequest req)
    {
        if (req.SeatCount <= 0) throw new ArgumentOutOfRangeException(nameof(req.SeatCount));

        var flight = _flights.GetById(req.FlightCode)
                     ?? throw new InvalidOperationException("Flight not found.");
        var passenger = _passengers.GetById(req.PassengerId)
                        ?? throw new InvalidOperationException("Passenger not found.");

        EnsureNotDeparted(flight);

        var available = GetAvailable(flight, req.Class);
        if (available < req.SeatCount)
            throw new InvalidOperationException("Not enough seats available for the selected class.");

        SetAvailable(flight, req.Class, available - req.SeatCount);
        _flights.Upsert(flight, x => x.Code);

        var unitPrice = PriceFor(flight, req.Class);
        var total = unitPrice * req.SeatCount;

        var booking = new Booking
        {
            PassengerId = passenger.Id,
            FlightCode = flight.Code,
            SeatClass = req.Class,          // <== مهم
            SeatCount = req.SeatCount,
            TotalPrice = total,
            Status = BookingStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow     // <== مهم
        };

        _bookings.Upsert(booking, b => b.Id);
        return booking;
    }

    public bool Cancel(Guid bookingId)
    {
        var booking = _bookings.GetById(bookingId);
        if (booking is null) return false;
        if (booking.Status == BookingStatus.Cancelled) return true;

        var flight = _flights.GetById(booking.FlightCode)
                     ?? throw new InvalidOperationException("Flight not found for this booking.");

        EnsureNotDeparted(flight);

        var back = GetAvailable(flight, booking.SeatClass) + booking.SeatCount;
        SetAvailable(flight, booking.SeatClass, back);
        _flights.Upsert(flight, x => x.Code);

        booking.Status = BookingStatus.Cancelled;
        _bookings.Upsert(booking, b => b.Id);
        return true;
    }

    public Booking Modify(ModifyBookingRequest req)
    {
        var booking = _bookings.GetById(req.BookingId)
                      ?? throw new InvalidOperationException("Booking not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a cancelled booking.");

        var flight = _flights.GetById(booking.FlightCode)
                     ?? throw new InvalidOperationException("Flight not found for this booking.");

        EnsureNotDeparted(flight);

        var currentClass = booking.SeatClass;
        var currentSeats = booking.SeatCount;

        var newClass = req.NewClass ?? currentClass;
        var newSeats = req.NewSeatCount ?? currentSeats;

        if (newSeats <= 0) throw new ArgumentOutOfRangeException(nameof(req.NewSeatCount));

        SetAvailable(flight, currentClass, GetAvailable(flight, currentClass) + currentSeats);

        var avail = GetAvailable(flight, newClass);
        if (avail < newSeats)
        {
            SetAvailable(flight, currentClass, GetAvailable(flight, currentClass) - currentSeats);
            throw new InvalidOperationException("Not enough seats for the new class/seat count.");
        }
        SetAvailable(flight, newClass, avail - newSeats);
        _flights.Upsert(flight, x => x.Code);

        booking.SeatClass = newClass;
        booking.SeatCount = newSeats;
        booking.TotalPrice = PriceFor(flight, newClass) * newSeats;

        _bookings.Upsert(booking, b => b.Id);
        return booking;
    }

    public IReadOnlyList<Booking> GetMyBookings(Guid passengerId)
        => _bookings.GetAll()
                    .Where(b => b.PassengerId == passengerId)
                    .OrderByDescending(b => b.CreatedAtUtc)  // <== مهم
                    .ToList();
}
