namespace PlaneCrazy.Domain.Validation;

/// <summary>
/// Base validator interface for validating objects of type T.
/// </summary>
/// <typeparam name="T">The type to validate</typeparam>
public interface IValidator<T>
{
    /// <summary>
    /// Validates the given value and returns a detailed validation result.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A ValidationResult containing validation status and any errors</returns>
    ValidationResult Validate(T value);
    
    /// <summary>
    /// Quick check if a value is valid without detailed error messages.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValid(T value);
}
