using System.Text.RegularExpressions;

namespace PlaneCrazy.Domain.Validation.Validators;

/// <summary>
/// Validator for ICAO24 aircraft identifiers.
/// Format: 6 hexadecimal characters (case-insensitive, normalized to uppercase).
/// Examples: "A12345", "abc123" -> "ABC123"
/// </summary>
public class IcaoValidator : IValidator<string?>
{
    private static readonly Regex IcaoRegex = new("^[A-Fa-f0-9]{6}$", RegexOptions.Compiled);
    
    public ValidationResult Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("ICAO24 cannot be empty");
        }
        
        if (value.Length != 6)
        {
            return ValidationResult.Failure($"ICAO24 must be exactly 6 characters (found {value.Length})");
        }
        
        if (!IcaoRegex.IsMatch(value))
        {
            return ValidationResult.Failure("ICAO24 must contain only hexadecimal characters (0-9, A-F)");
        }
        
        return ValidationResult.Success();
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
    
    /// <summary>
    /// Normalizes an ICAO24 to uppercase format.
    /// </summary>
    public static string? Normalize(string? value)
    {
        return value?.ToUpperInvariant();
    }
}
