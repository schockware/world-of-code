namespace WorldOfCode.Weather;

/// <summary>Why an input was rejected. <see cref="Reason"/> is one of the codes defined by the weather specs.</summary>
public sealed record Rejection(string Reason);

/// <summary>Reason codes. Codes marked provisional are not yet backed by a spec requirement.</summary>
public static class Reasons
{
    public const string QtyValueMissing = "qty.value-missing";
    public const string QtyUnitMissing = "qty.unit-missing";
    public const string QtyValueNotNumeric = "qty.value-not-numeric";
    public const string QtyUnknownMember = "qty.unknown-member";
    public const string UnitNotUcum = "unit.not-ucum";
    public const string UnitNamespaced = "unit.namespaced";

    // Provisional: WX-OBS is not written yet, and WX-QTY does not say what a non-object Quantity is.
    public const string QtyNotObject = "qty.not-object";
    public const string ObsNotObject = "obs.not-object";
    public const string ObsStationMissing = "obs.station-missing";
    public const string ObsObservedAtMissing = "obs.observed-at-missing";
    public const string ObsObservedAtInvalid = "obs.observed-at-invalid";
    public const string ObsElementNull = "obs.element-null";
    public const string ObsUnknownMember = "obs.unknown-member";
}

/// <summary>Thrown by the <c>Read</c> methods for callers who prefer to fail fast.</summary>
public sealed class WeatherValidationException(Rejection rejection) : Exception(rejection.Reason)
{
    public Rejection Rejection { get; } = rejection;
}
