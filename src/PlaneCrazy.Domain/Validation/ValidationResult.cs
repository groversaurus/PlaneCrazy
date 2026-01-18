namespace PlaneCrazy.Domain.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation passed.
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets the list of validation error messages.
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Gets a concatenated error message from all errors, or null if no errors.
    /// </summary>
    public string? ErrorMessage => Errors.Any() ? string.Join("; ", Errors) : null;
    
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };
    
    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static ValidationResult Failure(string error) => new() 
    { 
        IsValid = false, 
        Errors = new List<string> { error } 
    };
    
    /// <summary>
    /// Creates a failed validation result with multiple errors.
    /// </summary>
    public static ValidationResult Failure(List<string> errors) => new() 
    { 
        IsValid = false, 
        Errors = errors 
    };
}
