using PlaneCrazy.Domain.Validation.Validators;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class TextValidatorTests
{
    [Fact]
    public void Validate_ValidText_ReturnsSuccess()
    {
        var validator = new TextValidator(minLength: 1, maxLength: 100);
        var result = validator.Validate("Valid text");
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_TextTooShort_ReturnsFailure()
    {
        var validator = new TextValidator(minLength: 10, maxLength: 100);
        var result = validator.Validate("Short");
        
        Assert.False(result.IsValid);
        Assert.Contains("at least 10 characters", result.ErrorMessage);
    }

    [Fact]
    public void Validate_TextTooLong_ReturnsFailure()
    {
        var validator = new TextValidator(minLength: 1, maxLength: 10);
        var result = validator.Validate("This text is too long");
        
        Assert.False(result.IsValid);
        Assert.Contains("cannot exceed 10 characters", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EmptyRequired_ReturnsFailure()
    {
        var validator = new TextValidator(required: true);
        var result = validator.Validate("");
        
        Assert.False(result.IsValid);
        Assert.Contains("cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EmptyOptional_ReturnsSuccess()
    {
        var validator = new TextValidator(required: false);
        var result = validator.Validate("");
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ForCommentText_ValidatesCorrectly()
    {
        var validator = TextValidator.ForCommentText();
        
        // Valid comment
        Assert.True(validator.Validate("This is a comment").IsValid);
        
        // Empty comment
        Assert.False(validator.Validate("").IsValid);
        
        // Too long comment
        var longText = new string('x', 5001);
        Assert.False(validator.Validate(longText).IsValid);
    }

    [Fact]
    public void ForUserName_ValidatesCorrectly()
    {
        var validator = TextValidator.ForUserName();
        
        // Valid user name
        Assert.True(validator.Validate("John Doe").IsValid);
        
        // Empty is OK (optional)
        Assert.True(validator.Validate("").IsValid);
        
        // Too long
        var longName = new string('x', 101);
        Assert.False(validator.Validate(longName).IsValid);
    }

    [Fact]
    public void ForReason_ValidatesCorrectly()
    {
        var validator = TextValidator.ForReason();
        
        // Valid reason
        Assert.True(validator.Validate("Spam content").IsValid);
        
        // Empty is OK (optional)
        Assert.True(validator.Validate("").IsValid);
        
        // Too long
        var longReason = new string('x', 501);
        Assert.False(validator.Validate(longReason).IsValid);
    }
}
