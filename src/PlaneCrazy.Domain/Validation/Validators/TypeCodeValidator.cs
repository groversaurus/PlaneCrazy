using System.Text.RegularExpressions;

namespace PlaneCrazy.Domain.Validation.Validators;

/// <summary>
/// Validator for aircraft type codes.
/// Format: 2-10 alphanumeric characters, case-insensitive, normalized to uppercase.
/// Examples: "B738", "A320", "B77W", "PC12"
/// </summary>
public class TypeCodeValidator : IValidator<string?>
{
    private static readonly Regex TypeCodeRegex = new("^[A-Za-z0-9]{2,10}$", RegexOptions.Compiled);
    
    public ValidationResult Validate(string? value)
    {
        // Type code is optional in some cases
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Success();
        }
        
        if (value.Length < 2)
        {
            return ValidationResult.Failure($"Type code must be at least 2 characters (found {value.Length})");
        }
        
        if (value.Length > 10)
        {
            return ValidationResult.Failure($"Type code cannot exceed 10 characters (found {value.Length})");
        }
        
        if (!TypeCodeRegex.IsMatch(value))
        {
            return ValidationResult.Failure("Type code must contain only alphanumeric characters");
        }
        
        return ValidationResult.Success();
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
    
    /// <summary>
    /// Validates a required type code (non-null/empty).
    /// </summary>
    public ValidationResult ValidateRequired(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("Type code cannot be empty");
        }
        
        return Validate(value);
    }
    
    /// <summary>
    /// Normalizes a type code to uppercase format.
    /// </summary>
    public static string? Normalize(string? value)
    {
        return value?.ToUpperInvariant();
    }
}
