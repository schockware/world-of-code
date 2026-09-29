using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace WorldOfCode.Weather;

/// <summary>
/// Reads an <see cref="Observation"/> from JSON. Only the parts the core specs cover are strict.
/// Reasons starting <c>obs.</c> are provisional until WX-OBS is written.
/// </summary>
public static class ObservationReader
{
    public static bool TryRead(
        JsonElement json,
        IUnitValidator units,
        [NotNullWhen(true)] out Observation? observation,
        [NotNullWhen(false)] out Rejection? rejection)
    {
        observation = null;
        rejection = null;

        if (json.ValueKind != JsonValueKind.Object)
        {
            rejection = new Rejection(Reasons.ObsNotObject);
            return false;
        }

        Station? station = null;
        DateTimeOffset? observedAt = null;
        var elements = new Dictionary<string, Quantity>();

        foreach (var member in json.EnumerateObject())
        {
            switch (member.Name)
            {
                case "station":
                    if (!TryReadStation(member.Value, out station))
                    {
                        rejection = new Rejection(Reasons.ObsStationMissing);
                        return false;
                    }

                    break;
                case "observedAt":
                    if (member.Value.ValueKind != JsonValueKind.String
                        || !DateTimeOffset.TryParse(member.Value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var at))
                    {
                        rejection = new Rejection(Reasons.ObsObservedAtInvalid);
                        return false;
                    }

                    observedAt = at;
                    break;
                default:
                    if (!Observation.ElementNames.Contains(member.Name))
                    {
                        rejection = new Rejection(Reasons.ObsUnknownMember);
                        return false;
                    }

                    // "Not reported" has exactly one form: the element is omitted. A JSON null is not allowed.
                    if (member.Value.ValueKind == JsonValueKind.Null)
                    {
                        rejection = new Rejection(Reasons.ObsElementNull);
                        return false;
                    }

                    if (!QuantityReader.TryRead(member.Value, units, out var quantity, out rejection))
                    {
                        return false;
                    }

                    elements[member.Name] = quantity;
                    break;
            }
        }

        if (station is null)
        {
            rejection = new Rejection(Reasons.ObsStationMissing);
            return false;
        }

        if (observedAt is null)
        {
            rejection = new Rejection(Reasons.ObsObservedAtMissing);
            return false;
        }

        observation = new Observation
        {
            Station = station,
            ObservedAt = observedAt.Value,
            Temperature = elements.GetValueOrDefault("temperature"),
            Dewpoint = elements.GetValueOrDefault("dewpoint"),
            RelativeHumidity = elements.GetValueOrDefault("relativeHumidity"),
            WindDirection = elements.GetValueOrDefault("windDirection"),
            WindSpeed = elements.GetValueOrDefault("windSpeed"),
            WindGust = elements.GetValueOrDefault("windGust"),
            BarometricPressure = elements.GetValueOrDefault("barometricPressure"),
            SeaLevelPressure = elements.GetValueOrDefault("seaLevelPressure"),
            Visibility = elements.GetValueOrDefault("visibility"),
        };
        return true;
    }

    public static Observation Read(JsonElement json, IUnitValidator units) =>
        TryRead(json, units, out var observation, out var rejection)
            ? observation
            : throw new WeatherValidationException(rejection);

    private static bool TryReadStation(JsonElement json, [NotNullWhen(true)] out Station? station)
    {
        station = null;
        if (json.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        string? id = null;
        string? name = null;
        foreach (var member in json.EnumerateObject())
        {
            if (member.Value.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            switch (member.Name)
            {
                case "id":
                    id = member.Value.GetString();
                    break;
                case "name":
                    name = member.Value.GetString();
                    break;
                default:
                    return false;
            }
        }

        if (id is null)
        {
            return false;
        }

        station = new Station(id, name);
        return true;
    }
}
