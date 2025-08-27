namespace AirportTicketBooking.Application.DTOs;

public sealed class ImportResult
{
    public int Inserted { get; set; }
    public List<ImportError> Errors { get; } = new();
    public bool HasErrors => Errors.Count > 0;
}
