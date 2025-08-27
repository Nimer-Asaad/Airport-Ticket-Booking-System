using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Application.DTOs;

public sealed class BookingRequest
{
    public Guid PassengerId { get; init; }
    public string FlightCode { get; init; } = string.Empty;
    public SeatClass Class { get; init; } = SeatClass.Economy;
    public int SeatCount { get; init; } = 1;
}
