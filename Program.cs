using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Validation;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("== Flight model validation ==");
foreach (var r in ValidationInspector.Describe<Flight>())
{
    Console.WriteLine($"- {r.Field} : {r.Type} | {string.Join(", ", r.Constraints)}");
}
