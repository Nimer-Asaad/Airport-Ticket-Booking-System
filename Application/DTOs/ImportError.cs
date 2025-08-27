namespace AirportTicketBooking.Application.DTOs;

public sealed class ImportError
{
    public int Row { get; init; }                 
    public string Field { get; init; } = "";       
    public string Message { get; init; } = "";    
}
