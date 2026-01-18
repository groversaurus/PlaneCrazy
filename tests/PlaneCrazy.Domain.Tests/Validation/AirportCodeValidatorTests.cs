using PlaneCrazy.Domain.Validation.Validators;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class AirportCodeValidatorTests
{
    private readonly AirportCodeValidator _validator = new();

    [Theory]
    [InlineData("KJFK", true)]
    [InlineData("EGLL", true)]
    [InlineData("LFPG", true)]
    [InlineData("EDDF", true)]
    [InlineData("YSSY", true)]
    public void ValidateIcao_ValidIcaoCode_ReturnsSuccess(string icaoCode, bool expected)
    {
        var result = _validator.ValidateIcao(icaoCode);
        Assert.Equal(expected, result.IsValid);
    }

    [Theory]
    [InlineData("JFK")] // Too short
    [InlineData("KJFKX")] // Too long
    [InlineData("K1FK")] // Contains digit
    [InlineData("kjfk")] // Lowercase
    [InlineData("")] // Empty
    [InlineData(null)] // Null
    public void ValidateIcao_InvalidIcaoCode_ReturnsFailure(string? icaoCode)
    {
        var result = _validator.ValidateIcao(icaoCode);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("JFK", true)]
    [InlineData("LHR", true)]
    [InlineData("CDG", true)]
    public void ValidateIata_ValidIataCode_ReturnsSuccess(string iataCode, bool expected)
    {
        var result = _validator.ValidateIata(iataCode);
        Assert.Equal(expected, result.IsValid);
    }

    [Theory]
    [InlineData("JF")] // Too short
    [InlineData("JFKK")] // Too long
    [InlineData("jfk")] // Lowercase
    public void ValidateIata_InvalidIataCode_ReturnsFailure(string iataCode)
    {
        var result = _validator.ValidateIata(iataCode);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("kjfk", "KJFK")]
    [InlineData("KJFK", "KJFK")]
    [InlineData("egll", "EGLL")]
    public void Normalize_ReturnsUppercase(string input, string expected)
    {
        var result = AirportCodeValidator.Normalize(input);
        Assert.Equal(expected, result);
    }
}
