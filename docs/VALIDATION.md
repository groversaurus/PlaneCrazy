# PlaneCrazy Validation Framework

This document describes the validation framework used in PlaneCrazy to ensure data integrity across commands and events.

## Table of Contents

1. [Overview](#overview)
2. [Validation Rules](#validation-rules)
3. [Using Validators](#using-validators)
4. [Error Messages](#error-messages)
5. [Examples](#examples)

## Overview

The PlaneCrazy validation framework provides a centralized, consistent approach to validating data before it enters the event stream. The framework consists of:

- **Core Framework**: `IValidator<T>`, `ValidationResult`, `ValidationException`
- **Specific Validators**: ICAO24, Registration, TypeCode, Airport Code, Text, Entity Type
- **Orchestrators**: `CommandValidator`, `EventValidator`

### Architecture

```
Command/Event
    ↓
CommandValidator/EventValidator (Orchestrator)
    ↓
Specific Validators (IcaoValidator, TypeCodeValidator, etc.)
    ↓
ValidationResult (Success or Failure with errors)
    ↓
ValidationException (if invalid)
```

## Validation Rules

### ICAO24 (Aircraft Identifier)

**Format**: Exactly 6 hexadecimal characters  
**Case**: Case-insensitive, normalized to uppercase  
**Required**: Yes (for aircraft-related commands)

**Valid Examples**:
- `A12345`
- `abc123` → normalized to `ABC123`
- `F0F0F0`
- `123ABC`

**Invalid Examples**:
- `12345` (too short)
- `ABCDEFG` (too long)
- `XYZ123` (contains non-hex character 'X')
- `` (empty)

### Aircraft Registration

**Format**: 1-10 alphanumeric characters with optional hyphens  
**Case**: Case-insensitive, normalized to uppercase  
**Required**: No (optional field)

**Valid Examples**:
- `N12345` (US N-number)
- `G-ABCD` (UK registration)
- `D-AIZY` (German registration)
- `N123AB`
- `VH-OQA` (Australian registration)

**Invalid Examples**:
- `ABCDEFGHIJK` (too long, >10 chars)
- `N@1234` (contains invalid character '@')

### Aircraft Type Code

**Format**: 2-10 alphanumeric characters  
**Case**: Case-insensitive, normalized to uppercase  
**Required**: Varies (required for FavouriteAircraftTypeCommand, optional elsewhere)

**Valid Examples**:
- `B738` (Boeing 737-800)
- `A320` (Airbus A320)
- `B77W` (Boeing 777-300ER)
- `PC12` (Pilatus PC-12)
- `C172` (Cessna 172)

**Invalid Examples**:
- `A` (too short)
- `ABCDEFGHIJK` (too long, >10 chars)
- `B737-800` (contains hyphen, use `B738`)

### Airport ICAO Code

**Format**: Exactly 4 uppercase letters  
**Case**: Case-sensitive (must be uppercase)  
**Required**: Yes (for airport-related commands)

**Valid Examples**:
- `KJFK` (New York JFK)
- `EGLL` (London Heathrow)
- `LFPG` (Paris Charles de Gaulle)
- `EDDF` (Frankfurt)
- `YSSY` (Sydney)

**Invalid Examples**:
- `JFK` (too short, use IATA for 3-letter codes)
- `KJFKX` (too long)
- `K1FK` (contains digit)
- `kjfk` (lowercase, must be uppercase)

### Airport IATA Code

**Format**: Exactly 3 uppercase letters  
**Case**: Case-sensitive (must be uppercase)  
**Required**: Context-dependent

**Valid Examples**:
- `JFK` (New York)
- `LHR` (London)
- `CDG` (Paris)
- `FRA` (Frankfurt)

**Invalid Examples**:
- `JF` (too short)
- `JFKK` (too long)
- `jfk` (lowercase)

### Comment Text

**Format**: Non-empty string  
**Max Length**: 5000 characters  
**Required**: Yes

**Valid Examples**:
- `Great aircraft!`
- `Spotted this at KJFK today`
- Any text between 1 and 5000 characters

**Invalid Examples**:
- `` (empty string)
- `x`.repeat(5001) (exceeds 5000 characters)

### Entity Type

**Format**: Exact string match (case-insensitive)  
**Allowed Values**: `"Aircraft"`, `"Type"`, `"Airport"`  
**Required**: Yes (for comment-related commands)

**Valid Examples**:
- `Aircraft`
- `Type`
- `Airport`
- `aircraft` (case-insensitive)

**Invalid Examples**:
- `Plane`
- `Airplane`
- `AircraftType`
- `` (empty)

### Optional Fields

The following fields have validation but are optional:

- **User** (max 100 characters)
- **TypeName** (max 200 characters)
- **Airport Name** (max 200 characters)
- **Reason** (max 500 characters)

## Using Validators

### Direct Validator Usage

```csharp
using PlaneCrazy.Domain.Validation.Validators;

// Validate an ICAO24
var icaoValidator = new IcaoValidator();
var result = icaoValidator.Validate("A12345");
if (!result.IsValid)
{
    Console.WriteLine($"Validation failed: {result.ErrorMessage}");
}

// Quick validation check
if (icaoValidator.IsValid("A12345"))
{
    Console.WriteLine("Valid ICAO24");
}

// Normalize to uppercase
var normalized = IcaoValidator.Normalize("abc123"); // Returns "ABC123"
```

### Command Validation

Commands automatically validate when `Validate()` is called:

```csharp
var command = new FavouriteAircraftCommand
{
    Icao24 = "A12345",
    Registration = "N12345",
    TypeCode = "B738"
};

try
{
    command.Validate(); // Throws ValidationException if invalid
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"  - {error}");
    }
}
```

### Using CommandValidator Directly

```csharp
using PlaneCrazy.Domain.Validation;

var command = new AddCommentCommand
{
    EntityType = "Aircraft",
    EntityId = "A12345",
    Text = "Great aircraft!"
};

var result = CommandValidator.ValidateAddComment(command);
if (!result.IsValid)
{
    Console.WriteLine($"Validation errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}
```

### Event Validation

Events are automatically validated before being persisted to the event store:

```csharp
var @event = new AircraftFavourited
{
    Icao24 = "A12345",
    Registration = "N12345",
    TypeCode = "B738"
};

// This will automatically validate before persistence
await eventStore.AppendEventAsync(@event);
```

If validation fails, a `ValidationException` is thrown with details about the errors.

## Error Messages

### Format

All validation error messages follow a consistent format:

- **Field identification**: The error message identifies which field failed validation
- **Specific issue**: Describes what is wrong (too long, too short, invalid format, etc.)
- **Context**: Includes the actual value when helpful (e.g., length found)

### Example Error Messages

**ICAO24 Errors**:
- `"ICAO24 cannot be empty"`
- `"ICAO24 must be exactly 6 characters (found 5)"`
- `"ICAO24 must contain only hexadecimal characters (0-9, A-F)"`

**Type Code Errors**:
- `"Type code cannot be empty"`
- `"Type code must be at least 2 characters (found 1)"`
- `"Type code cannot exceed 10 characters (found 12)"`
- `"Type code must contain only alphanumeric characters"`

**Entity Type Errors**:
- `"Entity type cannot be empty"`
- `"Entity type must be one of: Aircraft, Type, Airport (found 'Plane')"`

**Text Field Errors**:
- `"Comment text cannot be empty"`
- `"Comment text cannot exceed 5000 characters (found 5200)"`
- `"User name cannot exceed 100 characters (found 150)"`

**Airport Code Errors**:
- `"ICAO airport code must be exactly 4 characters (found 3)"`
- `"ICAO airport code must be 4 uppercase letters"`

### Multiple Errors

When multiple validation errors occur, they are combined:

```
Validation failed: ICAO24 must be exactly 6 characters (found 5); Type code cannot exceed 10 characters (found 12)
```

## Examples

### Example 1: Validating a Favourite Aircraft Command

```csharp
var command = new FavouriteAircraftCommand
{
    Icao24 = "A12345",      // Valid: 6 hex chars
    Registration = "N12345", // Valid: alphanumeric with hyphen allowed
    TypeCode = "B738"        // Valid: 2-10 alphanumeric
};

command.Validate(); // Success - no exception thrown
```

### Example 2: Invalid ICAO24

```csharp
var command = new FavouriteAircraftCommand
{
    Icao24 = "12345", // Invalid: only 5 characters
};

try
{
    command.Validate();
}
catch (ValidationException ex)
{
    // Output: "ICAO24 must be exactly 6 characters (found 5)"
    Console.WriteLine(ex.Message);
}
```

### Example 3: Multiple Validation Errors

```csharp
var command = new AddCommentCommand
{
    EntityType = "Airplane",  // Invalid: not in allowed list
    EntityId = "12345",       // Invalid: too short for ICAO24
    Text = ""                 // Invalid: empty
};

try
{
    command.Validate();
}
catch (ValidationException ex)
{
    // Output: Multiple errors combined
    Console.WriteLine(ex.Message);
    // "Entity type must be one of: Aircraft, Type, Airport (found 'Airplane'); 
    //  Invalid Aircraft ID (ICAO24): ICAO24 must be exactly 6 characters (found 5); 
    //  Comment text cannot be empty"
}
```

### Example 4: Optional Fields

```csharp
// Optional fields can be null or omitted
var command = new FavouriteAircraftCommand
{
    Icao24 = "A12345",
    // Registration is optional - can be null
    // TypeCode is optional - can be null
};

command.Validate(); // Success
```

### Example 5: Normalizing Input

```csharp
using PlaneCrazy.Domain.Validation.Validators;

// ICAO24 is case-insensitive and normalized to uppercase
var icao = IcaoValidator.Normalize("abc123"); // Returns "ABC123"

// Type codes are also normalized
var typeCode = TypeCodeValidator.Normalize("b738"); // Returns "B738"

// Airport codes are normalized
var airport = AirportCodeValidator.Normalize("kjfk"); // Returns "KJFK"
```

### Example 6: Custom Validation Logic

```csharp
using PlaneCrazy.Domain.Validation.Validators;

// Create a custom text validator
var customValidator = new TextValidator(
    minLength: 10,
    maxLength: 500,
    required: true,
    fieldName: "Description"
);

var result = customValidator.Validate("Short"); // Too short

if (!result.IsValid)
{
    // Output: "Description must be at least 10 characters (found 5)"
    Console.WriteLine(result.ErrorMessage);
}
```

## Best Practices

1. **Always validate commands** before processing them
2. **Use normalization** for case-insensitive fields (ICAO24, type codes, airport codes)
3. **Catch ValidationException** in command handlers for better error logging
4. **Provide meaningful error messages** to help users correct their input
5. **Validate early** to fail fast and provide quick feedback
6. **Use batch validation** to report all errors at once instead of one at a time
7. **Document validation rules** when creating new validators

## Extension Points

### Creating a Custom Validator

To create a new validator, implement `IValidator<T>`:

```csharp
using PlaneCrazy.Domain.Validation;

public class CustomValidator : IValidator<string?>
{
    public ValidationResult Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("Value cannot be empty");
        }
        
        // Add your validation logic here
        
        return ValidationResult.Success();
    }
    
    public bool IsValid(string? value)
    {
        return Validate(value).IsValid;
    }
}
```

### Thread Safety

All validators in the framework are thread-safe and can be shared across multiple threads. They use immutable regex patterns and stateless validation logic.

### Performance

Validation overhead is minimal, typically <10ms per command/event. Regex patterns are compiled for optimal performance.

## Support

For questions or issues with validation, please refer to:

- Source code: `src/PlaneCrazy.Domain/Validation/`
- Issue tracker: GitHub Issues
- Tests: `tests/PlaneCrazy.Domain.Tests/Validation/` (when created)
