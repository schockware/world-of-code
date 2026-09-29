using System.Globalization;
using System.Text;
using System.Text.Json;

namespace WorldOfCode.Weather;

/// <summary>Writes the model back out as contract JSON, without rounding or reshaping (WX-QTY-006, WX-UNIT-002).</summary>
public static class WeatherJsonWriter
{
    public static void Write(Utf8JsonWriter writer, Quantity quantity)
    {
        writer.WriteStartObject();
        if (quantity.Value is { } value)
        {
            writer.WriteNumber("value", value);
        }
        else
        {
            writer.WriteNull("value");
        }

        writer.WriteString("unit", quantity.Unit);
        writer.WriteEndObject();
    }

    public static void Write(Utf8JsonWriter writer, Observation observation)
    {
        writer.WriteStartObject();

        writer.WriteStartObject("station");
        writer.WriteString("id", observation.Station.Id);
        if (observation.Station.Name is { } name)
        {
            writer.WriteString("name", name);
        }

        writer.WriteEndObject();

        // RFC 3339, using "Z" for UTC.
        var observedAt = observation.ObservedAt.Offset == TimeSpan.Zero
            ? observation.ObservedAt.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture)
            : observation.ObservedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
        writer.WriteString("observedAt", observedAt);

        foreach (var element in Observation.ElementNames)
        {
            if (observation.Get(element) is { } quantity)
            {
                writer.WritePropertyName(element);
                Write(writer, quantity);
            }
        }

        writer.WriteEndObject();
    }

    public static string ToJson(Quantity quantity) => Render(w => Write(w, quantity));

    public static string ToJson(Observation observation) => Render(w => Write(w, observation));

    private static string Render(Action<Utf8JsonWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            write(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
