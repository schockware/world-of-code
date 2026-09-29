using System.Text.Json;

namespace WorldOfCode.Weather.Tests;

/// <summary>Runs every vector in domains/weather/specs/core/conformance. Passing them is the scoreboard.</summary>
public class ConformanceTests
{
    private static readonly IUnitValidator Units = new UcumUnitValidator();

    public static TheoryData<string, string> Vectors()
    {
        var data = new TheoryData<string, string>();
        var dir = Path.Combine(AppContext.BaseDirectory, "conformance");
        foreach (var file in Directory.GetFiles(dir, "*.json").Order())
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(file));
            foreach (var vector in doc.RootElement.GetProperty("vectors").EnumerateArray())
            {
                data.Add(Path.GetFileName(file), vector.GetProperty("id").GetString()!);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Vector(string file, string id)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "conformance", file)));
        var vector = doc.RootElement.GetProperty("vectors").EnumerateArray()
            .Single(v => v.GetProperty("id").GetString() == id);

        var input = vector.GetProperty("input");
        var expect = vector.GetProperty("expect");

        switch (vector.GetProperty("operation").GetString())
        {
            case "validate":
                var ok = QuantityReader.TryRead(input.GetProperty("quantity"), Units, out _, out var rejection);
                AssertOutcome(expect, ok, rejection);
                break;

            case "classify":
                var element = input.GetProperty("element").GetString()!;
                Assert.True(ObservationReader.TryRead(input.GetProperty("observation"), Units, out var observation, out var why), why?.Reason);
                Assert.Equal(expect.GetProperty("state").GetString(), Kebab(observation.Classify(element)));
                break;

            case "roundtrip":
                var written = input.TryGetProperty("quantity", out var q)
                    ? WriteQuantity(q)
                    : WriteObservation(input.GetProperty("observation"));
                using (var actual = JsonDocument.Parse(written))
                {
                    Assert.True(JsonEquals(expect.GetProperty("result"), actual.RootElement),
                        $"expected {expect.GetProperty("result")} but wrote {written}");
                }

                break;

            case var other:
                Assert.Fail($"Unknown operation '{other}'.");
                break;
        }
    }

    private static void AssertOutcome(JsonElement expect, bool ok, Rejection? rejection)
    {
        if (expect.GetProperty("outcome").GetString() == "ok")
        {
            Assert.True(ok, $"expected ok but rejected with {rejection?.Reason}");
        }
        else
        {
            Assert.False(ok, "expected a rejection but the input was accepted");
            Assert.Equal(expect.GetProperty("reason").GetString(), rejection!.Reason);
        }
    }

    private static string WriteQuantity(JsonElement json) =>
        WeatherJsonWriter.ToJson(QuantityReader.Read(json, Units));

    private static string WriteObservation(JsonElement json) =>
        WeatherJsonWriter.ToJson(ObservationReader.Read(json, Units));

    private static string Kebab(AnswerState state) => state switch
    {
        AnswerState.NotReported => "not-reported",
        AnswerState.ReportedWithoutValue => "reported-without-value",
        AnswerState.Reported => "reported",
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };

    /// <summary>JSON equality: object members in any order, numbers by numeric value.</summary>
    private static bool JsonEquals(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind)
        {
            return false;
        }

        switch (a.ValueKind)
        {
            case JsonValueKind.Object:
                var left = a.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var right = b.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                return left.Count == right.Count
                    && left.All(p => right.TryGetValue(p.Key, out var other) && JsonEquals(p.Value, other));
            case JsonValueKind.Array:
                var l = a.EnumerateArray().ToList();
                var r = b.EnumerateArray().ToList();
                return l.Count == r.Count && l.Zip(r).All(t => JsonEquals(t.First, t.Second));
            case JsonValueKind.Number:
                return a.GetDecimal() == b.GetDecimal();
            case JsonValueKind.String:
                return a.GetString() == b.GetString();
            default:
                return true;
        }
    }
}
