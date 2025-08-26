using System.ComponentModel.DataAnnotations;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public string FlightCode { get; set; } = default!;
    [Required] public Guid PassengerId { get; set; }

    [Required] public SeatClass SeatClass { get; set; }

    [Range(1, 9)]
    public int SeatCount { get; set; }

    [Range(0, 1_000_000)]
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
