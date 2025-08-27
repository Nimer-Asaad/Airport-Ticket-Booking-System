using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Application.DTOs;

public sealed class BookingFilterParams
{
    public string? FlightCode { get; init; }
    public Guid? PassengerId { get; init; }
    public SeatClass? Class { get; init; }

    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }

    public string? FromCountry { get; init; }
    public string? ToCountry { get; init; }
    public string? FromAirport { get; init; }
    public string? ToAirport { get; init; }
    public DateTime? DepartureDateUtc { get; init; }
}
