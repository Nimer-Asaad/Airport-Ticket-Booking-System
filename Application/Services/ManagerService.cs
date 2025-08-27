using System.Linq;
using AirportTicketBooking.Domain.Validation;

namespace AirportTicketBooking.Application.Services
{
    public sealed class ManagerService
    {
        public static IReadOnlyList<(string Field, string Type, IReadOnlyList<string> Constraints)>
            DescribeFlightModel()
        {
            var desc = ValidationInspector.Describe<AirportTicketBooking.Domain.Entities.Flight>();

            return desc
                .Select(d => (d.Field, d.Type, (IReadOnlyList<string>)d.Constraints))
                .ToList();
        }
    }
}
