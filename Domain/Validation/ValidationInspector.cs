using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AirportTicketBooking.Domain.Validation;

public record FieldRule(string Field, string Type, IReadOnlyList<string> Constraints);

public static class ValidationInspector
{
    public static IEnumerable<FieldRule> Describe<T>()
    {
        foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var constraints = new List<string>();
            var typeName = SimplifyType(p.PropertyType);

            foreach (var a in p.GetCustomAttributes(true))
            {
                switch (a)
                {
                    case RequiredAttribute:
                        constraints.Add("Required"); break;
                    case RangeAttribute r:
                        constraints.Add($"Range({r.Minimum}..{r.Maximum})"); break;
                    case RegularExpressionAttribute reg:
                        constraints.Add($"Regex({reg.Pattern})"); break;
                    case EmailAddressAttribute:
                        constraints.Add("Email format"); break;
                    case Validation.Attributes.FutureOrTodayAttribute:
                        constraints.Add("Allowed: today → future"); break;
                    case Validation.Attributes.IataAttribute:
                        constraints.Add("IATA(AAA)"); break;
                }
            }

            yield return new FieldRule(p.Name, typeName, constraints);
        }
    }

    private static string SimplifyType(Type t)
    {
        if (t == typeof(string)) return "Text";
        if (t == typeof(int)) return "Number(Int32)";
        if (t == typeof(decimal)) return "Number(Decimal)";
        if (t == typeof(DateTime)) return "DateTime";
        if (t.IsEnum) return $"Enum({t.Name})";
        if (Nullable.GetUnderlyingType(t) is { } u) return $"{SimplifyType(u)}?";
        return t.Name;
    }
}
