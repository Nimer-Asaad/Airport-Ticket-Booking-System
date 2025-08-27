using AirportTicketBooking.Application.Contracts;
using AirportTicketBooking.Application.DTOs;
using AirportTicketBooking.Domain.Entities;
using AirportTicketBooking.Domain.Enums;

namespace AirportTicketBooking.Application.Services;

public sealed class FlightService
{
    private readonly IRepository<Flight, string> _repo;

    public FlightService(IRepository<Flight, string> repo) => _repo = repo;

    public IReadOnlyList<(Flight flight, decimal price)> Search(FlightSearchParams p)
    {
        var all = _repo.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(p.FromCountry))
            all = all.Where(f => f.DepartureCountry.Equals(p.FromCountry, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(p.ToCountry))
            all = all.Where(f => f.DestinationCountry.Equals(p.ToCountry, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(p.FromAirport))
            all = all.Where(f => f.DepartureAirport.Equals(p.FromAirport, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(p.ToAirport))
            all = all.Where(f => f.ArrivalAirport.Equals(p.ToAirport, StringComparison.OrdinalIgnoreCase));

        if (p.DepartureDateUtc.HasValue)
        {
            var day = p.DepartureDateUtc.Value.Date;
            all = all.Where(f => f.DepartureUtc.Date == day);
        }

        var cls = p.Class ?? SeatClass.Economy;

        static decimal PriceFor(Flight f, SeatClass c) => c switch
        {
            SeatClass.Economy => f.EconomyPrice,
            SeatClass.Business => f.BusinessPrice,
            SeatClass.First => f.FirstPrice,
            _ => f.EconomyPrice
        };

        var result = all
            .Select(f => (flight: f, price: PriceFor(f, cls)))
            .ToList();

        if (p.MaxPrice.HasValue)
            result = result.Where(x => x.price <= p.MaxPrice.Value).ToList();

        return result;
    }
}
