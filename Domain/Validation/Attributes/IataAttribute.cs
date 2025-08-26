using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AirportTicketBooking.Domain.Validation.Attributes;

public sealed class IataAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is string s && Regex.IsMatch(s, "^[A-Z]{3}$");

    public override string FormatErrorMessage(string name)
        => $"{name} must be a 3-letter IATA code (e.g., AMM, DXB).";
}
