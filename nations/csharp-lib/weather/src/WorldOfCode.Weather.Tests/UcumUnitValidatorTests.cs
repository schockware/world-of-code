namespace WorldOfCode.Weather.Tests;

/// <summary>Checks the validator against the unit list itself and against a few compound forms from the UCUM spec.</summary>
public class UcumUnitValidatorTests
{
    private static readonly UcumUnitValidator Units = new();

    [Fact]
    public void EveryAtomInTheUnitListIsValid()
    {
        var failures = UcumUnitValidator.KnownAtomCodes.Where(code => !Units.IsValid(code)).ToList();
        Assert.True(failures.Count == 0, "Rejected atoms: " + string.Join(", ", failures));
    }

    [Theory]
    [InlineData("m/s")]
    [InlineData("/min")]
    [InlineData("km2")]
    [InlineData("s-1")]
    [InlineData("10*3/uL")]
    [InlineData("mm[Hg]")]
    [InlineData("kg.m/s2")]
    [InlineData("(m/s)")]
    [InlineData("{rbc}")]
    [InlineData("mL{total}")]
    [InlineData("hPa")]
    [InlineData("%")]
    [InlineData("[degF]")]
    public void AcceptsValidCodes(string code) => Assert.True(Units.IsValid(code));

    [Theory]
    [InlineData("")]
    [InlineData("degC")]
    [InlineData("celsius")]
    [InlineData("cel")]
    [InlineData("m/")]
    [InlineData("m..s")]
    [InlineData("m s")]
    [InlineData("(m")]
    [InlineData("(m/s)2")] // an exponent applies to a unit, not to a parenthesized term
    [InlineData("m{unclosed")]
    [InlineData("m-")]
    [InlineData("[degF")]
    [InlineData("kBtu_IT[")]
    public void RejectsInvalidCodes(string code) => Assert.False(Units.IsValid(code));
}
