using PlaneCrazy.Domain.Validation.Validators;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class IcaoValidatorTests
{
    private readonly IcaoValidator _validator = new();

    [Theory]
    [InlineData("A12345", true)]
    [InlineData("ABC123", true)]
    [InlineData("123ABC", true)]
    [InlineData("FFFFFF", true)]
    [InlineData("000000", true)]
    [InlineData("abc123", true)] // Case insensitive
    public void Validate_ValidIcao24_ReturnsSuccess(string icao24, bool expected)
    {
        var result = _validator.Validate(icao24);
        Assert.Equal(expected, result.IsValid);
    }

    [Theory]
    [InlineData("12345")] // Too short
    [InlineData("ABCDEFG")] // Too long
    [InlineData("XYZ123")] // Invalid hex (X, Y, Z)
    [InlineData("G12345")] // Invalid hex (G)
    [InlineData("")] // Empty
    [InlineData(null)] // Null
    [InlineData("   ")] // Whitespace
    public void Validate_InvalidIcao24_ReturnsFailure(string? icao24)
    {
        var result = _validator.Validate(icao24);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("abc123", "ABC123")]
    [InlineData("ABC123", "ABC123")]
    [InlineData("a1b2c3", "A1B2C3")]
    public void Normalize_ReturnsUppercase(string input, string expected)
    {
        var result = IcaoValidator.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_ValidIcao24_ReturnsTrue()
    {
        Assert.True(_validator.IsValid("A12345"));
    }

    [Fact]
    public void IsValid_InvalidIcao24_ReturnsFalse()
    {
        Assert.False(_validator.IsValid("12345"));
    }
}
