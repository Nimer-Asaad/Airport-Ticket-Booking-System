namespace AirportTicketBooking.Application.DTOs;

public sealed class FlightSearchParams
{
    public decimal? MaxPrice { get; init; }
    public string? FromCountry { get; init; }
    public string? ToCountry { get; init; }
    public string? FromAirport { get; init; }
    public string? ToAirport { get; init; }
    public DateTime? DepartureDateUtc { get; init; } 
    public AirportTicketBooking.Domain.Enums.SeatClass? Class { get; init; }
}
