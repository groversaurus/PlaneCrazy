using PlaneCrazy.Domain.Validation.Validators;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class TypeCodeValidatorTests
{
    private readonly TypeCodeValidator _validator = new();

    [Theory]
    [InlineData("B738", true)]
    [InlineData("A320", true)]
    [InlineData("B77W", true)]
    [InlineData("PC12", true)]
    [InlineData("C172", true)]
    [InlineData("AB", true)] // Min 2 chars
    [InlineData("ABCDEFGHIJ", true)] // Max 10 chars
    public void Validate_ValidTypeCode_ReturnsSuccess(string typeCode, bool expected)
    {
        var result = _validator.Validate(typeCode);
        Assert.Equal(expected, result.IsValid);
    }

    [Theory]
    [InlineData("A")] // Too short
    [InlineData("ABCDEFGHIJK")] // Too long (11 chars)
    [InlineData("B737-800")] // Contains hyphen
    [InlineData("A320 ")] // Contains space
    public void Validate_InvalidTypeCode_ReturnsFailure(string typeCode)
    {
        var result = _validator.Validate(typeCode);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_NullOrEmpty_ReturnsSuccessForOptional()
    {
        // Type code is optional in some contexts
        Assert.True(_validator.Validate(null).IsValid);
        Assert.True(_validator.Validate("").IsValid);
    }

    [Fact]
    public void ValidateRequired_NullOrEmpty_ReturnsFailure()
    {
        var result = _validator.ValidateRequired(null);
        Assert.False(result.IsValid);
        Assert.Contains("cannot be empty", result.ErrorMessage);
    }

    [Theory]
    [InlineData("b738", "B738")]
    [InlineData("a320", "A320")]
    [InlineData("PC12", "PC12")]
    public void Normalize_ReturnsUppercase(string input, string expected)
    {
        var result = TypeCodeValidator.Normalize(input);
        Assert.Equal(expected, result);
    }
}
