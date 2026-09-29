namespace WorldOfCode.Weather;

public interface IUnitValidator
{
    /// <summary>True when <paramref name="unit"/> is a valid UCUM code in the case-sensitive form (WX-UNIT-001).</summary>
    bool IsValid(string unit);
}
