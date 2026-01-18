using PlaneCrazy.Domain.Validation.Validators;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class EntityTypeValidatorTests
{
    private readonly EntityTypeValidator _validator = new();

    [Theory]
    [InlineData("Aircraft", true)]
    [InlineData("Type", true)]
    [InlineData("Airport", true)]
    [InlineData("aircraft", true)] // Case insensitive
    [InlineData("AIRCRAFT", true)]
    [InlineData("type", true)]
    public void Validate_ValidEntityType_ReturnsSuccess(string entityType, bool expected)
    {
        var result = _validator.Validate(entityType);
        Assert.Equal(expected, result.IsValid);
    }

    [Theory]
    [InlineData("Plane")]
    [InlineData("Airplane")]
    [InlineData("AircraftType")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_InvalidEntityType_ReturnsFailure(string? entityType)
    {
        var result = _validator.Validate(entityType);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void GetValidTypes_ReturnsExpectedTypes()
    {
        var validTypes = EntityTypeValidator.GetValidTypes().ToList();
        
        Assert.Contains("Aircraft", validTypes, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Type", validTypes, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Airport", validTypes, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(3, validTypes.Count);
    }
}
