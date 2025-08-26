using System.ComponentModel.DataAnnotations;
using AirportTicketBooking.Domain.Validation.Attributes;

namespace AirportTicketBooking.Domain.Entities;

public class Flight
{
    [Required, RegularExpression(@"^[A-Z0-9]{2,6}$")]
    public string Code { get; set; } = default!;

    [Required, Iata] public string DepartureAirport { get; set; } = default!;
    [Required, Iata] public string ArrivalAirport { get; set; } = default!;

    [Required] public string DepartureCountry { get; set; } = default!;
    [Required] public string DestinationCountry { get; set; } = default!;

    [Required, FutureOrToday] public DateTime DepartureUtc { get; set; }
    [Required] public DateTime ArrivalUtc { get; set; }

    [Range(0, 50_000)] public decimal EconomyPrice { get; set; }
    [Range(0, 50_000)] public decimal BusinessPrice { get; set; }
    [Range(0, 50_000)] public decimal FirstPrice { get; set; }

    [Range(0, 1000)] public int EconomySeats { get; set; }
    [Range(0, 1000)] public int BusinessSeats { get; set; }
    [Range(0, 1000)] public int FirstSeats { get; set; }
}
