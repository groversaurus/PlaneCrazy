namespace PlaneCrazy.Domain.Validation.Validators;

/// <summary>
/// Validator for text fields with length constraints.
/// </summary>
public class TextValidator : IValidator<string?>
{
    /// <summary>
    /// Maximum length for comment text.
    /// </summary>
    public const int CommentTextMaxLength = 5000;
    
    /// <summary>
    /// Maximum length for user names.
    /// </summary>
    public const int UserNameMaxLength = 100;
    
    /// <summary>
    /// Maximum length for deletion/edit reasons.
    /// </summary>
    public const int ReasonMaxLength = 500;
    
    /// <summary>
    /// Maximum length for type and airport names.
    /// </summary>
    public const int NameMaxLength = 200;
    
    private readonly int _minLength;
    private readonly int _maxLength;
    private readonly bool _required;
    private readonly string _fieldName;
    
    /// <summary>
    /// Creates a new TextValidator with specified constraints.
    /// </summary>
    /// <param name="minLength">Minimum allowed length (default: 1)</param>
    /// <param name="maxLength">Maximum allowed length (default: int.MaxValue)</param>
    /// <param name="required">Whether the field is required (default: true)</param>
    /// <param name="fieldName">Name of the field for error messages (default: "Text")</param>
    public TextValidator(int minLength = 1, int maxLength = int.MaxValue, bool required = true, string fieldName = "Text")
    {
        _minLength = minLength;
        _maxLength = maxLength;
        _required = required;
        _fieldName = fieldName;
    }
    
    public ValidationResult Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (_required)
            {
                return ValidationResult.Failure($"{_fieldName} cannot be empty");
            }
            return ValidationResult.Success();
        }
        
        if (value.Length < _minLength)
        {
            return ValidationResult.Failure($"{_fieldName} must be at least {_minLength} characters (found {value.Length})");
        }
        
        if (value.Length > _maxLength)
        {
            return ValidationResult.Failure($"{_fieldName} cannot exceed {_maxLength} characters (found {value.Length})");
        }
        
        return ValidationResult.Success();
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
    
    /// <summary>
    /// Creates a validator for comment text (max 5000 characters).
    /// </summary>
    public static TextValidator ForCommentText() => new(minLength: 1, maxLength: CommentTextMaxLength, required: true, fieldName: "Comment text");
    
    /// <summary>
    /// Creates a validator for user names (max 100 characters, optional).
    /// </summary>
    public static TextValidator ForUserName() => new(minLength: 1, maxLength: UserNameMaxLength, required: false, fieldName: "User name");
    
    /// <summary>
    /// Creates a validator for reasons (max 500 characters, optional).
    /// </summary>
    public static TextValidator ForReason() => new(minLength: 1, maxLength: ReasonMaxLength, required: false, fieldName: "Reason");
    
    /// <summary>
    /// Creates a validator for type names (max 200 characters, optional).
    /// </summary>
    public static TextValidator ForTypeName() => new(minLength: 1, maxLength: NameMaxLength, required: false, fieldName: "Type name");
    
    /// <summary>
    /// Creates a validator for airport names (max 200 characters, optional).
    /// </summary>
    public static TextValidator ForAirportName() => new(minLength: 1, maxLength: NameMaxLength, required: false, fieldName: "Airport name");
}
