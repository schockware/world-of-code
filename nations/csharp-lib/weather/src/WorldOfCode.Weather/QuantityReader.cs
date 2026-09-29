using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace WorldOfCode.Weather;

/// <summary>Reads a <see cref="Quantity"/> from JSON, applying WX-QTY and WX-UNIT.</summary>
public static class QuantityReader
{
    private static readonly string[] NamespacePrefixes = ["wmo:", "wmoUnit:", "nwsUnit:", "uc:"];

    public static bool TryRead(
        JsonElement json,
        IUnitValidator units,
        [NotNullWhen(true)] out Quantity? quantity,
        [NotNullWhen(false)] out Rejection? rejection)
    {
        quantity = null;
        rejection = null;

        if (json.ValueKind != JsonValueKind.Object)
        {
            rejection = new Rejection(Reasons.QtyNotObject);
            return false;
        }

        // Inspect the members directly. Deserializing straight to the record would treat a missing
        // "value" the same as "value": null, which WX-QTY-007 forbids.
        JsonElement? value = null;
        JsonElement? unit = null;
        foreach (var member in json.EnumerateObject())
        {
            switch (member.Name)
            {
                case "value":
                    value = member.Value;
                    break;
                case "unit":
                    unit = member.Value;
                    break;
                default:
                    rejection = new Rejection(Reasons.QtyUnknownMember);
                    return false;
            }
        }

        if (value is null)
        {
            rejection = new Rejection(Reasons.QtyValueMissing);
            return false;
        }

        if (unit is null)
        {
            rejection = new Rejection(Reasons.QtyUnitMissing);
            return false;
        }

        decimal? number;
        switch (value.Value.ValueKind)
        {
            case JsonValueKind.Null:
                number = null;
                break;
            case JsonValueKind.Number when value.Value.TryGetDecimal(out var d):
                number = d;
                break;
            default:
                // Not a number, not null, or a number that does not fit decimal.
                rejection = new Rejection(Reasons.QtyValueNotNumeric);
                return false;
        }

        if (unit.Value.ValueKind != JsonValueKind.String)
        {
            rejection = new Rejection(Reasons.UnitNotUcum);
            return false;
        }

        var code = unit.Value.GetString()!;
        if (code.Length == 0)
        {
            rejection = new Rejection(Reasons.UnitNotUcum);
            return false;
        }

        // WX-UNIT-003: a namespaced unit is reported as namespaced even though it is not valid UCUM either.
        if (NamespacePrefixes.Any(p => code.StartsWith(p, StringComparison.Ordinal)))
        {
            rejection = new Rejection(Reasons.UnitNamespaced);
            return false;
        }

        if (!units.IsValid(code))
        {
            rejection = new Rejection(Reasons.UnitNotUcum);
            return false;
        }

        quantity = new Quantity(number, code);
        return true;
    }

    public static Quantity Read(JsonElement json, IUnitValidator units) =>
        TryRead(json, units, out var quantity, out var rejection)
            ? quantity
            : throw new WeatherValidationException(rejection);
}
