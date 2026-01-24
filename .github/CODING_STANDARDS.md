# C# Coding Standards for PlaneCrazy

## Development Workflow

### Branching Strategy
- **All new development must be done on feature branches**
- Branch naming convention: `feature/<descriptive-name>` or `copilot/<descriptive-name>`
- Merge to `main` periodically after code review
- Use squash merges to keep main history clean
- Delete branches after successful merge

### Commit Guidelines
- Write clear, descriptive commit messages
- Use present tense ("Add feature" not "Added feature")
- Reference issue numbers when applicable (#123)
- Keep commits focused on a single change

## Naming Conventions

### General Rules
- Use **PascalCase** for: Classes, Methods, Properties, Events, Namespaces
- Use **camelCase** for: Local variables, method parameters, private fields
- Use **_camelCase** (underscore prefix) for: Private fields (when used)
- Use **UPPER_CASE** for: Constants only when they are public

### Specific Naming Patterns

#### Classes
```csharp
public class AircraftAggregate { }
public class CommentQueryService { }
public interface IAircraftRepository { }
```

#### Interfaces
- Prefix with `I`
```csharp
public interface IEventStore { }
public interface ICommandHandler<TCommand> { }
```

#### Methods
- Use verb phrases
- Be descriptive and clear about intent
```csharp
public async Task<Aircraft> GetAircraftByIdAsync(string aircraftId)
public void HandleCommand(AddCommentCommand command)
public bool ValidateEventStream(IEnumerable<DomainEvent> events)
```

#### Properties
```csharp
public string AircraftId { get; set; }
public DateTime LastSeen { get; private set; }
public List<Comment> Comments { get; } = new();
```

#### Events
- Use past tense (what happened)
```csharp
public class AircraftFavourited : DomainEvent { }
public class CommentAdded : DomainEvent { }
```

#### Commands
- Use imperative verb (what to do)
```csharp
public class AddCommentCommand : Command { }
public class FavouriteAircraftCommand : Command { }
```

## Code Organization

### File Structure
- One class per file
- File name matches class name
- Organize related classes in appropriate folders:
  - `Aggregates/` - Aggregate roots
  - `Commands/` - Command definitions
  - `Events/` - Domain events
  - `Queries/` - Query definitions
  - `Services/` - Application services
  - `Repositories/` - Data access
  - `Models/` - DTOs and view models

### Namespace Organization
```csharp
namespace PlaneCrazy.Domain.Aggregates;
namespace PlaneCrazy.Domain.Commands;
namespace PlaneCrazy.Infrastructure.EventStore;
```

### Using Directives
- Place `using` statements at the top of the file
- Remove unused `using` statements
- Group by: System namespaces, then third-party, then project namespaces
- Use file-scoped namespaces (C# 10+)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlaneCrazy.Domain.Events;
```

## Formatting Standards

### Indentation and Spacing
- Use **4 spaces** for indentation (not tabs)
- One statement per line
- One declaration per line
- Add blank lines to separate logical blocks of code

### Braces
- Use braces for all control structures (even single-line)
- Opening brace on same line (K&R style)
```csharp
if (condition) {
    DoSomething();
}

public class MyClass {
    public void MyMethod() {
        // code
    }
}
```

### Line Length
- Aim for max 120 characters per line
- Break long method chains across multiple lines
```csharp
var result = collection
    .Where(x => x.IsActive)
    .OrderBy(x => x.CreatedDate)
    .Select(x => x.ToDto())
    .ToList();
```

## Language Features

### Modern C# Features
- Use **nullable reference types** (enabled in project)
- Use **file-scoped namespaces** (C# 10)
- Use **record types** for immutable DTOs
- Use **pattern matching** where it improves readability
- Use **expression-bodied members** for simple properties/methods

```csharp
// File-scoped namespace
namespace PlaneCrazy.Domain.Events;

// Record type for immutable data
public record AircraftPosition(double Latitude, double Longitude, double Altitude);

// Expression-bodied member
public bool IsValid => !string.IsNullOrEmpty(AircraftId);

// Pattern matching
public string GetStatusDescription() => Status switch {
    AircraftStatus.Airborne => "In flight",
    AircraftStatus.Grounded => "On ground",
    _ => "Unknown"
};
```

### Async/Await
- Use `async`/`await` for I/O-bound operations
- Suffix async methods with `Async`
- Avoid `async void` (except event handlers)
- Use `ConfigureAwait(false)` in library code

```csharp
public async Task<Aircraft> GetAircraftAsync(string id) {
    return await _repository.FindByIdAsync(id).ConfigureAwait(false);
}
```

### LINQ
- Use method syntax (fluent) over query syntax
- Chain operations for readability
- Use meaningful variable names in lambdas

```csharp
var activeAircraft = aircraft
    .Where(a => a.IsActive)
    .OrderBy(a => a.Callsign)
    .ToList();
```

## Architecture Patterns

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Mutate state, return void or simple results
- **Queries**: Read data, return view models, never mutate
- Keep commands and queries strictly separated

```csharp
// Command - changes state
public class AddCommentCommand : Command {
    public string AggregateId { get; init; }
    public string CommentText { get; init; }
    public string UserId { get; init; }
}

// Query - reads data
public class GetAircraftDetailsQuery {
    public string AircraftId { get; init; }
}
```

### Event Sourcing
- Events are **immutable** - use `init` or read-only properties
- Events represent facts that happened in the past
- Never modify or delete events
- Use aggregate roots to ensure consistency

```csharp
public class AircraftFavourited : DomainEvent {
    public string AircraftId { get; init; }
    public string UserId { get; init; }
    public DateTime FavouritedAt { get; init; }
}
```

### Dependency Injection
- Use constructor injection
- Inject interfaces, not concrete types
- Use `ILogger<T>` for logging
- Register services in `DependencyInjection` folder

```csharp
public class AircraftQueryService : IAircraftQueryService {
    private readonly IAircraftRepository _repository;
    private readonly ILogger<AircraftQueryService> _logger;

    public AircraftQueryService(
        IAircraftRepository repository,
        ILogger<AircraftQueryService> logger) {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

## Error Handling

### Exceptions
- Use specific exception types
- Include meaningful error messages
- Don't catch exceptions you can't handle
- Log exceptions with appropriate context

```csharp
public async Task<Aircraft> GetAircraftAsync(string id) {
    if (string.IsNullOrWhiteSpace(id)) {
        throw new ArgumentException("Aircraft ID cannot be null or empty", nameof(id));
    }

    try {
        return await _repository.FindByIdAsync(id);
    }
    catch (RepositoryException ex) {
        _logger.LogError(ex, "Failed to retrieve aircraft {AircraftId}", id);
        throw;
    }
}
```

### Validation
- Validate at boundaries (API, commands)
- Use FluentValidation for complex validation
- Fail fast - validate early
- Return meaningful validation messages

## Logging

### Structured Logging
- Use structured logging with `ILogger<T>`
- Use log levels appropriately:
  - **Trace**: Very detailed, typically only enabled in development
  - **Debug**: Debugging information
  - **Information**: General informational messages
  - **Warning**: Warning but application continues
  - **Error**: Error occurred but application can continue
  - **Critical**: Application cannot continue

```csharp
_logger.LogInformation(
    "Aircraft {AircraftId} position updated to {Latitude}, {Longitude}",
    aircraft.Id,
    position.Latitude,
    position.Longitude
);

_logger.LogError(
    ex,
    "Failed to process command {CommandType} for aggregate {AggregateId}",
    command.GetType().Name,
    command.AggregateId
);
```

## Testing Standards

### Unit Tests
- Follow AAA pattern: Arrange, Act, Assert
- One assertion per test (generally)
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Use xUnit framework
- Mock dependencies with interfaces

```csharp
[Fact]
public void AddComment_ValidCommand_AddsCommentToAggregate() {
    // Arrange
    var aggregate = new CommentAggregate();
    var command = new AddCommentCommand {
        AggregateId = "test-id",
        CommentText = "Test comment",
        UserId = "user-1"
    };

    // Act
    aggregate.Handle(command);

    // Assert
    Assert.Single(aggregate.GetUncommittedEvents());
    Assert.IsType<CommentAdded>(aggregate.GetUncommittedEvents().First());
}
```

### Test Organization
- Mirror source project structure in test projects
- Group related tests in classes
- Use `[Theory]` for parameterized tests

## Documentation

### XML Documentation
- Document public APIs with XML comments
- Include `<summary>`, `<param>`, `<returns>`, `<exception>`
- Be concise but clear

```csharp
/// <summary>
/// Retrieves an aircraft by its unique identifier.
/// </summary>
/// <param name="aircraftId">The unique identifier of the aircraft.</param>
/// <returns>The aircraft if found; otherwise, null.</returns>
/// <exception cref="ArgumentNullException">Thrown when aircraftId is null.</exception>
public async Task<Aircraft?> GetAircraftByIdAsync(string aircraftId) {
    // implementation
}
```

### Code Comments
- Write self-documenting code (good names over comments)
- Comment **why**, not **what**
- Explain complex algorithms or business rules
- Keep comments up to date with code changes

## Performance Considerations

### General Guidelines
- Avoid premature optimization
- Profile before optimizing
- Use `Span<T>` and `Memory<T>` for performance-critical code
- Reuse collections when possible
- Use `StringBuilder` for string concatenation in loops

### Async Best Practices
- Don't block on async code (`Task.Wait()` or `Task.Result`)
- Use `ValueTask<T>` for frequently called methods that often complete synchronously
- Avoid unnecessary async state machines

### Memory Management
- Dispose of `IDisposable` resources properly
- Use `using` statements or declarations
- Be mindful of closures and captured variables
- Avoid large object allocations in tight loops

```csharp
// Using declaration (C# 8+)
using var stream = File.OpenRead(filePath);
var content = await ReadStreamAsync(stream);
```

## Security

### Input Validation
- Never trust user input
- Validate all input at boundaries
- Sanitize data before storage
- Use parameterized queries (not applicable here, but generally)

### Sensitive Data
- Never log sensitive information (passwords, tokens, PII)
- Use configuration for secrets, not hardcoded values
- Be careful with exception messages containing sensitive data

## Code Review Checklist

Before submitting a PR, ensure:
- [ ] Code follows naming conventions
- [ ] All tests pass
- [ ] New features have tests
- [ ] XML documentation for public APIs
- [ ] No compiler warnings
- [ ] Consistent formatting
- [ ] No commented-out code (remove or justify)
- [ ] Error handling is appropriate
- [ ] Logging is meaningful and structured
- [ ] Dependencies are injected, not newed up
- [ ] Branch is up to date with main
- [ ] Commit messages are clear

## Tools and Configuration

### Recommended Extensions
- EditorConfig for consistent formatting
- StyleCop or Roslynator for code analysis
- SonarLint for code quality

### .editorconfig
Consider adding an `.editorconfig` file to enforce formatting standards automatically.

---

**Note**: These standards are living guidelines. As the project evolves, update this document to reflect new patterns and practices. Discuss significant deviations in code reviews.
