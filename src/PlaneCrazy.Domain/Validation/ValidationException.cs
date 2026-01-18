namespace PlaneCrazy.Domain.Validation;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors that caused this exception.
    /// </summary>
    public List<string> ValidationErrors { get; }
    
    /// <summary>
    /// Creates a new ValidationException with a single error message.
    /// </summary>
    public ValidationException(string message) : base(message)
    {
        ValidationErrors = new List<string> { message };
    }
    
    /// <summary>
    /// Creates a new ValidationException with multiple error messages.
    /// </summary>
    public ValidationException(List<string> errors) 
        : base(string.Join("; ", errors))
    {
        ValidationErrors = errors;
    }
    
    /// <summary>
    /// Creates a new ValidationException from a ValidationResult.
    /// </summary>
    public ValidationException(ValidationResult result) 
        : base(result.ErrorMessage ?? "Validation failed")
    {
        ValidationErrors = result.Errors;
    }
}
