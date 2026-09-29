namespace WorldOfCode.Weather;

/// <summary>
/// A measured value and the unit it was reported in (WX-QTY).
/// A null <see cref="Value"/> means "reported without a value". "Not reported" is a null
/// <see cref="Quantity"/> on the <see cref="Observation"/>, so this type is deliberately a class.
/// </summary>
public sealed record Quantity(decimal? Value, string Unit);

public enum AnswerState
{
    NotReported,
    ReportedWithoutValue,
    Reported,
}
