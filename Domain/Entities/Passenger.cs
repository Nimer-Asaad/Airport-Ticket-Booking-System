using System.ComponentModel.DataAnnotations;

namespace AirportTicketBooking.Domain.Entities;

public class Passenger
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MinLength(2)]
    public string Name { get; set; } = default!;

    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    public string? NationalId { get; set; }
}
