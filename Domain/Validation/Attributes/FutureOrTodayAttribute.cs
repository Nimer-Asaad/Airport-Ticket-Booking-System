using System.ComponentModel.DataAnnotations;

namespace AirportTicketBooking.Domain.Validation.Attributes;

public sealed class FutureOrTodayAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is DateTime dt && dt.Date >= DateTime.Today;

    public override string FormatErrorMessage(string name)
        => $"{name} must be today or in the future.";
}
