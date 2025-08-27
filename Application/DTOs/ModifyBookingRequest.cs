using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Application.DTOs;

public sealed class ModifyBookingRequest
{
    public Guid BookingId { get; init; }
    public SeatClass? NewClass { get; init; }
    public int? NewSeatCount { get; init; }
}
