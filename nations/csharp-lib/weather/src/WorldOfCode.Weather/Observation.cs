namespace WorldOfCode.Weather;

public sealed record Station(string Id, string? Name = null);

/// <summary>Surface weather elements observed at one station at one instant. A null element was not reported.</summary>
public sealed record Observation
{
    public required Station Station { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public Quantity? Temperature { get; init; }
    public Quantity? Dewpoint { get; init; }
    public Quantity? RelativeHumidity { get; init; }
    public Quantity? WindDirection { get; init; }
    public Quantity? WindSpeed { get; init; }
    public Quantity? WindGust { get; init; }
    public Quantity? BarometricPressure { get; init; }
    public Quantity? SeaLevelPressure { get; init; }
    public Quantity? Visibility { get; init; }

    /// <summary>The element names, as they appear in the contract.</summary>
    public static IReadOnlyList<string> ElementNames { get; } =
    [
        "temperature", "dewpoint", "relativeHumidity", "windDirection", "windSpeed",
        "windGust", "barometricPressure", "seaLevelPressure", "visibility",
    ];

    public Quantity? Get(string element) => element switch
    {
        "temperature" => Temperature,
        "dewpoint" => Dewpoint,
        "relativeHumidity" => RelativeHumidity,
        "windDirection" => WindDirection,
        "windSpeed" => WindSpeed,
        "windGust" => WindGust,
        "barometricPressure" => BarometricPressure,
        "seaLevelPressure" => SeaLevelPressure,
        "visibility" => Visibility,
        _ => throw new ArgumentException($"Unknown element '{element}'.", nameof(element)),
    };

    /// <summary>Answers "what did the source say about this element?" (WX-QTY-001 to 005).</summary>
    public AnswerState Classify(string element) => Get(element) switch
    {
        null => AnswerState.NotReported,
        { Value: null } => AnswerState.ReportedWithoutValue,
        _ => AnswerState.Reported,
    };
}
