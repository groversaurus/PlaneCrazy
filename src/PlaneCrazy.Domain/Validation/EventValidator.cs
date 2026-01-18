using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Validation.Validators;

namespace PlaneCrazy.Domain.Validation;

/// <summary>
/// Orchestrator for event validation before persistence.
/// </summary>
public static class EventValidator
{
    private static readonly IcaoValidator _icaoValidator = new();
    private static readonly RegistrationValidator _registrationValidator = new();
    private static readonly TypeCodeValidator _typeCodeValidator = new();
    private static readonly AirportCodeValidator _airportCodeValidator = new();
    private static readonly EntityTypeValidator _entityTypeValidator = new();
    
    /// <summary>
    /// Minimum allowed timestamp for events (year 2000).
    /// </summary>
    public static readonly DateTime MinTimestamp = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    /// <summary>
    /// Clock skew buffer in minutes to allow for slightly future timestamps.
    /// </summary>
    public const int ClockSkewBufferMinutes = 5;
    
    /// <summary>
    /// Validates a domain event before persistence.
    /// </summary>
    public static ValidationResult Validate(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            CommentAdded e => ValidateCommentAdded(e),
            CommentEdited e => ValidateCommentEdited(e),
            CommentDeleted e => ValidateCommentDeleted(e),
            AircraftFavourited e => ValidateAircraftFavourited(e),
            TypeFavourited e => ValidateTypeFavourited(e),
            AirportFavourited e => ValidateAirportFavourited(e),
            // Other event types are valid by default
            _ => ValidationResult.Success()
        };
    }
    
    /// <summary>
    /// Validates a CommentAdded event.
    /// </summary>
    private static ValidationResult ValidateCommentAdded(CommentAdded @event)
    {
        var errors = new List<string>();
        
        // Validate CommentId
        if (@event.CommentId == Guid.Empty)
            errors.Add("CommentId cannot be empty");
        
        // Validate EntityType
        var entityTypeResult = _entityTypeValidator.Validate(@event.EntityType);
        if (!entityTypeResult.IsValid)
            errors.AddRange(entityTypeResult.Errors);
        
        // Validate EntityId
        if (string.IsNullOrWhiteSpace(@event.EntityId))
            errors.Add("EntityId cannot be empty");
        
        // Validate Text
        var textValidator = TextValidator.ForCommentText();
        var textResult = textValidator.Validate(@event.Text);
        if (!textResult.IsValid)
            errors.AddRange(textResult.Errors);
        
        // Validate User (optional)
        if (!string.IsNullOrWhiteSpace(@event.User))
        {
            var userValidator = TextValidator.ForUserName();
            var userResult = userValidator.Validate(@event.User);
            if (!userResult.IsValid)
                errors.AddRange(userResult.Errors);
        }
        
        // Validate Timestamp is reasonable (not too far in future, not before 2000)
        ValidateTimestamp(@event.Timestamp, errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a CommentEdited event.
    /// </summary>
    private static ValidationResult ValidateCommentEdited(CommentEdited @event)
    {
        var errors = new List<string>();
        
        // Validate CommentId
        if (@event.CommentId == Guid.Empty)
            errors.Add("CommentId cannot be empty");
        
        // Validate EntityType
        var entityTypeResult = _entityTypeValidator.Validate(@event.EntityType);
        if (!entityTypeResult.IsValid)
            errors.AddRange(entityTypeResult.Errors);
        
        // Validate EntityId
        if (string.IsNullOrWhiteSpace(@event.EntityId))
            errors.Add("EntityId cannot be empty");
        
        // Validate Text
        var textValidator = TextValidator.ForCommentText();
        var textResult = textValidator.Validate(@event.Text);
        if (!textResult.IsValid)
            errors.AddRange(textResult.Errors);
        
        // Validate Timestamp
        ValidateTimestamp(@event.Timestamp, errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a CommentDeleted event.
    /// </summary>
    private static ValidationResult ValidateCommentDeleted(CommentDeleted @event)
    {
        var errors = new List<string>();
        
        // Validate CommentId
        if (@event.CommentId == Guid.Empty)
            errors.Add("CommentId cannot be empty");
        
        // Validate EntityType
        var entityTypeResult = _entityTypeValidator.Validate(@event.EntityType);
        if (!entityTypeResult.IsValid)
            errors.AddRange(entityTypeResult.Errors);
        
        // Validate EntityId
        if (string.IsNullOrWhiteSpace(@event.EntityId))
            errors.Add("EntityId cannot be empty");
        
        // Validate Reason (optional)
        if (!string.IsNullOrWhiteSpace(@event.Reason))
        {
            var reasonValidator = TextValidator.ForReason();
            var reasonResult = reasonValidator.Validate(@event.Reason);
            if (!reasonResult.IsValid)
                errors.AddRange(reasonResult.Errors);
        }
        
        // Validate Timestamp
        ValidateTimestamp(@event.Timestamp, errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an AircraftFavourited event.
    /// </summary>
    private static ValidationResult ValidateAircraftFavourited(AircraftFavourited @event)
    {
        var errors = new List<string>();
        
        // Validate Icao24 (required)
        var icaoResult = _icaoValidator.Validate(@event.Icao24);
        if (!icaoResult.IsValid)
            errors.AddRange(icaoResult.Errors);
        
        // Validate Registration (optional)
        if (!string.IsNullOrWhiteSpace(@event.Registration))
        {
            var regResult = _registrationValidator.Validate(@event.Registration);
            if (!regResult.IsValid)
                errors.AddRange(regResult.Errors);
        }
        
        // Validate TypeCode (optional)
        if (!string.IsNullOrWhiteSpace(@event.TypeCode))
        {
            var typeResult = _typeCodeValidator.Validate(@event.TypeCode);
            if (!typeResult.IsValid)
                errors.AddRange(typeResult.Errors);
        }
        
        // Validate OccurredAt
        ValidateTimestamp(@event.OccurredAt, errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a TypeFavourited event.
    /// </summary>
    private static ValidationResult ValidateTypeFavourited(TypeFavourited @event)
    {
        var errors = new List<string>();
        
        // Validate TypeCode (required)
        var typeResult = _typeCodeValidator.ValidateRequired(@event.TypeCode);
        if (!typeResult.IsValid)
            errors.AddRange(typeResult.Errors);
        
        // Validate TypeName (optional)
        if (!string.IsNullOrWhiteSpace(@event.TypeName))
        {
            var nameValidator = TextValidator.ForTypeName();
            var nameResult = nameValidator.Validate(@event.TypeName);
            if (!nameResult.IsValid)
                errors.AddRange(nameResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an AirportFavourited event.
    /// </summary>
    private static ValidationResult ValidateAirportFavourited(AirportFavourited @event)
    {
        var errors = new List<string>();
        
        // Validate IcaoCode (required)
        var icaoResult = _airportCodeValidator.ValidateIcao(@event.IcaoCode);
        if (!icaoResult.IsValid)
            errors.AddRange(icaoResult.Errors);
        
        // Validate Name (optional)
        if (!string.IsNullOrWhiteSpace(@event.Name))
        {
            var nameValidator = TextValidator.ForAirportName();
            var nameResult = nameValidator.Validate(@event.Name);
            if (!nameResult.IsValid)
                errors.AddRange(nameResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates that a timestamp is reasonable (not in future, not before minimum date).
    /// </summary>
    private static void ValidateTimestamp(DateTime timestamp, List<string> errors)
    {
        var maxDate = DateTime.UtcNow.AddMinutes(ClockSkewBufferMinutes);
        
        if (timestamp < MinTimestamp)
        {
            errors.Add($"Timestamp cannot be before {MinTimestamp:yyyy-MM-dd} (found {timestamp:yyyy-MM-dd})");
        }
        
        if (timestamp > maxDate)
        {
            errors.Add($"Timestamp cannot be in the future (found {timestamp:yyyy-MM-dd HH:mm:ss})");
        }
    }
}
