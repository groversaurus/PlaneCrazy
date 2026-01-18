namespace PlaneCrazy.Domain.Validation.Validators;

/// <summary>
/// Validator for entity types.
/// Valid entity types: "Aircraft", "Type", "Airport"
/// </summary>
public class EntityTypeValidator : IValidator<string?>
{
    private static readonly HashSet<string> ValidEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Aircraft",
        "Type",
        "Airport"
    };
    
    public ValidationResult Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("Entity type cannot be empty");
        }
        
        if (!ValidEntityTypes.Contains(value))
        {
            return ValidationResult.Failure($"Entity type must be one of: Aircraft, Type, Airport (found '{value}')");
        }
        
        return ValidationResult.Success();
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
    
    /// <summary>
    /// Gets the list of valid entity types.
    /// </summary>
    public static IEnumerable<string> GetValidTypes() => ValidEntityTypes;
}
