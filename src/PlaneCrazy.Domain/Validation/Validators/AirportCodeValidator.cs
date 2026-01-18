using System.Text.RegularExpressions;

namespace PlaneCrazy.Domain.Validation.Validators;

/// <summary>
/// Validator for airport codes (ICAO and IATA formats).
/// </summary>
public class AirportCodeValidator : IValidator<string?>
{
    private static readonly Regex IcaoCodeRegex = new("^[A-Z]{4}$", RegexOptions.Compiled);
    private static readonly Regex IataCodeRegex = new("^[A-Z]{3}$", RegexOptions.Compiled);
    
    public ValidationResult Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("Airport code cannot be empty");
        }
        
        // Check for ICAO format (4 uppercase letters)
        if (value.Length == 4)
        {
            if (!IcaoCodeRegex.IsMatch(value))
            {
                return ValidationResult.Failure("ICAO airport code must be exactly 4 uppercase letters");
            }
            return ValidationResult.Success();
        }
        
        // Check for IATA format (3 uppercase letters)
        if (value.Length == 3)
        {
            if (!IataCodeRegex.IsMatch(value))
            {
                return ValidationResult.Failure("IATA airport code must be exactly 3 uppercase letters");
            }
            return ValidationResult.Success();
        }
        
        return ValidationResult.Failure($"Airport code must be either 3 (IATA) or 4 (ICAO) letters (found {value.Length})");
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
    
    /// <summary>
    /// Validates specifically ICAO format (4 uppercase letters).
    /// </summary>
    public ValidationResult ValidateIcao(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("ICAO airport code cannot be empty");
        }
        
        if (value.Length != 4)
        {
            return ValidationResult.Failure($"ICAO airport code must be exactly 4 characters (found {value.Length})");
        }
        
        if (!IcaoCodeRegex.IsMatch(value))
        {
            return ValidationResult.Failure("ICAO airport code must be 4 uppercase letters");
        }
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates specifically IATA format (3 uppercase letters).
    /// </summary>
    public ValidationResult ValidateIata(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("IATA airport code cannot be empty");
        }
        
        if (value.Length != 3)
        {
            return ValidationResult.Failure($"IATA airport code must be exactly 3 characters (found {value.Length})");
        }
        
        if (!IataCodeRegex.IsMatch(value))
        {
            return ValidationResult.Failure("IATA airport code must be 3 uppercase letters");
        }
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Normalizes an airport code to uppercase format.
    /// </summary>
    public static string? Normalize(string? value)
    {
        return value?.ToUpperInvariant();
    }
}
