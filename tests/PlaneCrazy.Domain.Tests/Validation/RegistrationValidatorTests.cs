using PlaneCrazy.Domain.Validation.Validators;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class RegistrationValidatorTests
{
    private readonly RegistrationValidator _validator = new();

    [Theory]
    [InlineData("N12345", true)]
    [InlineData("G-ABCD", true)]
    [InlineData("D-AIZY", true)]
    [InlineData("VH-OQA", true)]
    [InlineData("N123AB", true)]
    [InlineData("A", true)] // Min 1 char
    [InlineData("ABCDEFGHIJ", true)] // Max 10 chars
    public void Validate_ValidRegistration_ReturnsSuccess(string registration, bool expected)
    {
        var result = _validator.Validate(registration);
        Assert.Equal(expected, result.IsValid);
    }

    [Theory]
    [InlineData("ABCDEFGHIJK")] // Too long (11 chars)
    [InlineData("N@1234")] // Contains invalid character
    [InlineData("N 1234")] // Contains space
    public void Validate_InvalidRegistration_ReturnsFailure(string registration)
    {
        var result = _validator.Validate(registration);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_NullOrEmpty_ReturnsSuccessForOptional()
    {
        // Registration is optional
        Assert.True(_validator.Validate(null).IsValid);
        Assert.True(_validator.Validate("").IsValid);
    }

    [Theory]
    [InlineData("n12345", "N12345")]
    [InlineData("g-abcd", "G-ABCD")]
    [InlineData("N12345", "N12345")]
    public void Normalize_ReturnsUppercase(string input, string expected)
    {
        var result = RegistrationValidator.Normalize(input);
        Assert.Equal(expected, result);
    }
}
