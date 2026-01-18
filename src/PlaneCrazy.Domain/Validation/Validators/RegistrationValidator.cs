using System.Text.RegularExpressions;

namespace PlaneCrazy.Domain.Validation.Validators;

/// <summary>
/// Validator for aircraft registration numbers.
/// Common formats: N-number (US), G-prefix (UK), etc.
/// Format: Alphanumeric with optional hyphens, max 10 characters.
/// Examples: "N12345", "G-ABCD", "D-AIZY"
/// </summary>
public class RegistrationValidator : IValidator<string?>
{
    private static readonly Regex RegistrationRegex = new("^[A-Za-z0-9-]{1,10}$", RegexOptions.Compiled);
    
    public ValidationResult Validate(string? value)
    {
        // Registration is optional in most cases, so null/empty is considered valid
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Success();
        }
        
        if (value.Length > 10)
        {
            return ValidationResult.Failure($"Registration cannot exceed 10 characters (found {value.Length})");
        }
        
        if (value.Length < 1)
        {
            return ValidationResult.Failure("Registration must be at least 1 character");
        }
        
        if (!RegistrationRegex.IsMatch(value))
        {
            return ValidationResult.Failure("Registration must contain only alphanumeric characters and hyphens");
        }
        
        return ValidationResult.Success();
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
    
    /// <summary>
    /// Normalizes a registration to uppercase format.
    /// </summary>
    public static string? Normalize(string? value)
    {
        return value?.ToUpperInvariant();
    }
}
