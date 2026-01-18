using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Validation.Validators;

namespace PlaneCrazy.Domain.Validation;

/// <summary>
/// Orchestrator for command validation using specific validators.
/// </summary>
public static class CommandValidator
{
    private static readonly IcaoValidator _icaoValidator = new();
    private static readonly RegistrationValidator _registrationValidator = new();
    private static readonly TypeCodeValidator _typeCodeValidator = new();
    private static readonly AirportCodeValidator _airportCodeValidator = new();
    private static readonly EntityTypeValidator _entityTypeValidator = new();
    
    /// <summary>
    /// Validates an AddCommentCommand.
    /// </summary>
    public static ValidationResult ValidateAddComment(AddCommentCommand command)
    {
        var errors = new List<string>();
        
        // Validate EntityType
        var entityTypeResult = _entityTypeValidator.Validate(command.EntityType);
        if (!entityTypeResult.IsValid)
            errors.AddRange(entityTypeResult.Errors);
        
        // Validate EntityId
        if (string.IsNullOrWhiteSpace(command.EntityId))
            errors.Add("EntityId cannot be empty");
        else
            ValidateEntityId(command.EntityType, command.EntityId, errors);
        
        // Validate Text
        var textValidator = TextValidator.ForCommentText();
        var textResult = textValidator.Validate(command.Text);
        if (!textResult.IsValid)
            errors.AddRange(textResult.Errors);
        
        // Validate User (optional)
        if (!string.IsNullOrWhiteSpace(command.User))
        {
            var userValidator = TextValidator.ForUserName();
            var userResult = userValidator.Validate(command.User);
            if (!userResult.IsValid)
                errors.AddRange(userResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an EditCommentCommand.
    /// </summary>
    public static ValidationResult ValidateEditComment(EditCommentCommand command)
    {
        var errors = new List<string>();
        
        // Validate CommentId
        if (command.CommentId == Guid.Empty)
            errors.Add("CommentId cannot be empty");
        
        // Validate EntityType
        var entityTypeResult = _entityTypeValidator.Validate(command.EntityType);
        if (!entityTypeResult.IsValid)
            errors.AddRange(entityTypeResult.Errors);
        
        // Validate EntityId
        if (string.IsNullOrWhiteSpace(command.EntityId))
            errors.Add("EntityId cannot be empty");
        else
            ValidateEntityId(command.EntityType, command.EntityId, errors);
        
        // Validate NewText
        var textValidator = TextValidator.ForCommentText();
        var textResult = textValidator.Validate(command.NewText);
        if (!textResult.IsValid)
            errors.AddRange(textResult.Errors);
        
        // Validate User (optional)
        if (!string.IsNullOrWhiteSpace(command.User))
        {
            var userValidator = TextValidator.ForUserName();
            var userResult = userValidator.Validate(command.User);
            if (!userResult.IsValid)
                errors.AddRange(userResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a DeleteCommentCommand.
    /// </summary>
    public static ValidationResult ValidateDeleteComment(DeleteCommentCommand command)
    {
        var errors = new List<string>();
        
        // Validate CommentId
        if (command.CommentId == Guid.Empty)
            errors.Add("CommentId cannot be empty");
        
        // Validate EntityType
        var entityTypeResult = _entityTypeValidator.Validate(command.EntityType);
        if (!entityTypeResult.IsValid)
            errors.AddRange(entityTypeResult.Errors);
        
        // Validate EntityId
        if (string.IsNullOrWhiteSpace(command.EntityId))
            errors.Add("EntityId cannot be empty");
        else
            ValidateEntityId(command.EntityType, command.EntityId, errors);
        
        // Validate Reason (optional)
        if (!string.IsNullOrWhiteSpace(command.Reason))
        {
            var reasonValidator = TextValidator.ForReason();
            var reasonResult = reasonValidator.Validate(command.Reason);
            if (!reasonResult.IsValid)
                errors.AddRange(reasonResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a FavouriteAircraftCommand.
    /// </summary>
    public static ValidationResult ValidateFavouriteAircraft(FavouriteAircraftCommand command)
    {
        var errors = new List<string>();
        
        // Validate Icao24 (required)
        var icaoResult = _icaoValidator.Validate(command.Icao24);
        if (!icaoResult.IsValid)
            errors.AddRange(icaoResult.Errors);
        
        // Validate Registration (optional)
        if (!string.IsNullOrWhiteSpace(command.Registration))
        {
            var regResult = _registrationValidator.Validate(command.Registration);
            if (!regResult.IsValid)
                errors.AddRange(regResult.Errors);
        }
        
        // Validate TypeCode (optional)
        if (!string.IsNullOrWhiteSpace(command.TypeCode))
        {
            var typeResult = _typeCodeValidator.Validate(command.TypeCode);
            if (!typeResult.IsValid)
                errors.AddRange(typeResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a FavouriteAircraftTypeCommand.
    /// </summary>
    public static ValidationResult ValidateFavouriteAircraftType(FavouriteAircraftTypeCommand command)
    {
        var errors = new List<string>();
        
        // Validate TypeCode (required)
        var typeResult = _typeCodeValidator.ValidateRequired(command.TypeCode);
        if (!typeResult.IsValid)
            errors.AddRange(typeResult.Errors);
        
        // Validate TypeName (optional)
        if (!string.IsNullOrWhiteSpace(command.TypeName))
        {
            var nameValidator = TextValidator.ForTypeName();
            var nameResult = nameValidator.Validate(command.TypeName);
            if (!nameResult.IsValid)
                errors.AddRange(nameResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a FavouriteAirportCommand.
    /// </summary>
    public static ValidationResult ValidateFavouriteAirport(FavouriteAirportCommand command)
    {
        var errors = new List<string>();
        
        // Validate IcaoCode (required)
        var icaoResult = _airportCodeValidator.ValidateIcao(command.IcaoCode);
        if (!icaoResult.IsValid)
            errors.AddRange(icaoResult.Errors);
        
        // Validate Name (optional)
        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            var nameValidator = TextValidator.ForAirportName();
            var nameResult = nameValidator.Validate(command.Name);
            if (!nameResult.IsValid)
                errors.AddRange(nameResult.Errors);
        }
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an UnfavouriteAircraftCommand.
    /// </summary>
    public static ValidationResult ValidateUnfavouriteAircraft(UnfavouriteAircraftCommand command)
    {
        var errors = new List<string>();
        
        // Validate Icao24 (required)
        var icaoResult = _icaoValidator.Validate(command.Icao24);
        if (!icaoResult.IsValid)
            errors.AddRange(icaoResult.Errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an UnfavouriteAircraftTypeCommand.
    /// </summary>
    public static ValidationResult ValidateUnfavouriteAircraftType(UnfavouriteAircraftTypeCommand command)
    {
        var errors = new List<string>();
        
        // Validate TypeCode (required)
        var typeResult = _typeCodeValidator.ValidateRequired(command.TypeCode);
        if (!typeResult.IsValid)
            errors.AddRange(typeResult.Errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an UnfavouriteAirportCommand.
    /// </summary>
    public static ValidationResult ValidateUnfavouriteAirport(UnfavouriteAirportCommand command)
    {
        var errors = new List<string>();
        
        // Validate IcaoCode (required)
        var icaoResult = _airportCodeValidator.ValidateIcao(command.IcaoCode);
        if (!icaoResult.IsValid)
            errors.AddRange(icaoResult.Errors);
        
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates an entity ID based on its entity type.
    /// </summary>
    private static void ValidateEntityId(string entityType, string entityId, List<string> errors)
    {
        switch (entityType?.ToLowerInvariant())
        {
            case "aircraft":
                // For aircraft, EntityId should be ICAO24
                var icaoResult = _icaoValidator.Validate(entityId);
                if (!icaoResult.IsValid)
                    errors.Add($"Invalid Aircraft ID (ICAO24): {icaoResult.ErrorMessage}");
                break;
                
            case "type":
                // For type, EntityId should be TypeCode
                var typeResult = _typeCodeValidator.ValidateRequired(entityId);
                if (!typeResult.IsValid)
                    errors.Add($"Invalid Type ID (TypeCode): {typeResult.ErrorMessage}");
                break;
                
            case "airport":
                // For airport, EntityId should be ICAO code
                var airportResult = _airportCodeValidator.ValidateIcao(entityId);
                if (!airportResult.IsValid)
                    errors.Add($"Invalid Airport ID (ICAO): {airportResult.ErrorMessage}");
                break;
        }
    }
}
