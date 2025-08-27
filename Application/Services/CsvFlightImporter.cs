using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AirportTicketBooking.Application.Contracts;
using AirportTicketBooking.Application.DTOs;
using AirportTicketBooking.Domain.Entities;

namespace AirportTicketBooking.Application.Services;

public sealed class CsvFlightImporter
{
    private readonly IRepository<Flight, string> _flights;

    // صيغة CSV المتوقعة (رأس الملف بالضبط):
    // Code,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureUtc,ArrivalUtc,EconomyPrice,BusinessPrice,FirstPrice,EconomySeats,BusinessSeats,FirstSeats

    public CsvFlightImporter(IRepository<Flight, string> flights)
    {
        _flights = flights;
    }

    public ImportResult Import(string csvPath)
    {
        var result = new ImportResult();

        if (!File.Exists(csvPath))
        {
            result.Errors.Add(new ImportError { Row = 0, Field = "File", Message = "CSV file not found." });
            return result;
        }

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length <= 1)
        {
            result.Errors.Add(new ImportError { Row = 0, Field = "File", Message = "CSV is empty or missing header." });
            return result;
        }

        var existingCodes = _flights.GetAll().Select(f => f.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var batchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // helper
        static DateTime ParseUtc(string val) =>
            DateTime.Parse(val, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        for (int i = 1; i < lines.Length; i++)
        {
            var rowIndex = i + 1; 
            var raw = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var cells = SplitCsvLine(raw);
            if (cells.Count < 13)
            {
                result.Errors.Add(new ImportError { Row = rowIndex, Field = "", Message = "Not enough columns (expected 13)." });
                continue;
            }

            try
            {
                var f = new Flight
                {
                    Code = cells[0].Trim(),
                    DepartureCountry = cells[1].Trim(),
                    DestinationCountry = cells[2].Trim(),
                    DepartureAirport = cells[3].Trim(),
                    ArrivalAirport = cells[4].Trim(),
                    DepartureUtc = ParseUtc(cells[5].Trim()),
                    ArrivalUtc = ParseUtc(cells[6].Trim()),
                    EconomyPrice = decimal.Parse(cells[7].Trim(), CultureInfo.InvariantCulture),
                    BusinessPrice = decimal.Parse(cells[8].Trim(), CultureInfo.InvariantCulture),
                    FirstPrice = decimal.Parse(cells[9].Trim(), CultureInfo.InvariantCulture),
                    EconomySeats = int.Parse(cells[10].Trim(), CultureInfo.InvariantCulture),
                    BusinessSeats = int.Parse(cells[11].Trim(), CultureInfo.InvariantCulture),
                    FirstSeats = int.Parse(cells[12].Trim(), CultureInfo.InvariantCulture),
                };

                var ctx = new ValidationContext(f);
                Validator.ValidateObject(f, ctx, validateAllProperties: true);

                if (f.ArrivalUtc <= f.DepartureUtc)
                    throw new ValidationException("ArrivalUtc must be after DepartureUtc.");

                if (existingCodes.Contains(f.Code) || batchCodes.Contains(f.Code))
                    throw new ValidationException($"Duplicate flight code '{f.Code}'.");

                if (f.EconomyPrice <= 0 || f.BusinessPrice <= 0 || f.FirstPrice <= 0)
                    throw new ValidationException("Prices must be positive.");

                if (f.EconomySeats < 0 || f.BusinessSeats < 0 || f.FirstSeats < 0)
                    throw new ValidationException("Seat counts must be non-negative.");

                _flights.Upsert(f, x => x.Code);
                batchCodes.Add(f.Code);
                result.Inserted++;
            }
            catch (FormatException ex)
            {
                result.Errors.Add(new ImportError { Row = rowIndex, Field = "Parse", Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                result.Errors.Add(new ImportError { Row = rowIndex, Field = "Validation", Message = ex.Message });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportError { Row = rowIndex, Field = "", Message = ex.Message });
            }
        }

        return result;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new System.Text.StringBuilder();
        bool inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '\"') { inQuotes = !inQuotes; continue; }
            if (ch == ',' && !inQuotes) { result.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(ch);
        }
        result.Add(sb.ToString());
        return result;
    }
}
