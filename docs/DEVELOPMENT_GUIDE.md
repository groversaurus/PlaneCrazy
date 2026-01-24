# PlaneCrazy Development Guide

## Welcome

This guide will help you get started contributing to PlaneCrazy, understand the codebase structure, and follow our development practices.

## Prerequisites

### Required Software
- **.NET 10.0 SDK** or later
- **Visual Studio 2022**, **VS Code**, or **Rider**
- **Git** for version control
- **Windows**, **Linux**, or **macOS**

### Recommended Tools
- **Visual Studio Code Extensions**:
  - C# Dev Kit
  - .NET Extension Pack
  - GitHub Copilot (optional)
- **Visual Studio Extensions**:
  - ReSharper (optional)
  - CodeMaid (optional)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/planecrazy.git
cd planecrazy
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Run the Application

```bash
cd src/PlaneCrazy.Console
dotnet run
```

## Project Structure

```
planecrazy/
├── docs/                          # Documentation
│   ├── ARCHITECTURE.md
│   ├── BACKGROUND_SERVICES.md
│   ├── COMMAND_HANDLERS.md
│   └── ...
├── src/
│   ├── PlaneCrazy.Console/        # Console UI layer
│   │   ├── Program.cs
│   │   └── PlaneCrazy.Console.csproj
│   ├── PlaneCrazy.Domain/         # Core domain layer
│   │   ├── Aggregates/
│   │   ├── Commands/
│   │   ├── Entities/
│   │   ├── Events/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   ├── Queries/
│   │   └── Validation/
│   ├── PlaneCrazy.Infrastructure/ # Infrastructure layer
│   │   ├── CommandHandlers/
│   │   ├── DependencyInjection/
│   │   ├── EventDispatcher/
│   │   ├── EventStore/
│   │   ├── Http/
│   │   ├── Models/
│   │   ├── Projections/
│   │   ├── QueryServices/
│   │   ├── Repositories/
│   │   └── Services/
│   └── PlaneCrazy.Tests/          # Test project
├── tests/
│   └── PlaneCrazy.Domain.Tests/   # Domain unit tests
├── LICENSE
├── README.md
├── ROADMAP.md
└── PlaneCrazy.sln
```

## Architecture Overview

### Layer Dependencies

```
Console Layer
    ↓ (depends on)
Infrastructure Layer
    ↓ (depends on)
Domain Layer (no dependencies)
```

**Rule**: Domain layer must not depend on any other layers.

### Key Patterns

1. **Event Sourcing**: All state changes stored as events
2. **CQRS**: Separate command (write) and query (read) models
3. **Clean Architecture**: Clear separation of concerns
4. **Dependency Injection**: All services registered in DI container

## Development Workflow

### 1. Create a Branch

```bash
git checkout -b feature/your-feature-name
```

**Branch Naming**:
- `feature/` - New features
- `bugfix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring

### 2. Make Changes

Follow coding standards and patterns (see below).

### 3. Write Tests

All new code should have corresponding tests.

### 4. Run Tests

```bash
dotnet test
```

### 5. Commit Changes

```bash
git add .
git commit -m "feat: add new feature"
```

**Commit Message Format**:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance

### 6. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Adding New Features

### Adding a New Command

#### Step 1: Define the Command

Create in `PlaneCrazy.Domain/Commands/`:

```csharp
public class YourNewCommand : Command
{
    public required string SomeProperty { get; init; }
    
    public override void Validate()
    {
        // Add validation logic
        if (string.IsNullOrWhiteSpace(SomeProperty))
            throw new ArgumentException("SomeProperty is required");
    }
}
```

#### Step 2: Define the Event

Create in `PlaneCrazy.Domain/Events/`:

```csharp
public class YourEventHappened : DomainEvent
{
    public required string SomeProperty { get; set; }
}
```

#### Step 3: Update Aggregate (if needed)

In `PlaneCrazy.Domain/Aggregates/`:

```csharp
public class YourAggregate : AggregateRoot
{
    private string _someState;
    
    public void HandleCommand(YourNewCommand command)
    {
        // Validate business rules
        if (/* some condition */)
            throw new InvalidOperationException("Cannot do this");
        
        // Generate event
        var @event = new YourEventHappened
        {
            SomeProperty = command.SomeProperty
        };
        
        ApplyChange(@event);
    }
    
    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case YourEventHappened happened:
                _someState = happened.SomeProperty;
                break;
        }
    }
}
```

#### Step 4: Create Command Handler

Create in `PlaneCrazy.Infrastructure/CommandHandlers/`:

```csharp
public class YourNewCommandHandler : ICommandHandler<YourNewCommand>
{
    private readonly IEventStore _eventStore;
    private readonly IEventDispatcher _dispatcher;
    private readonly ILogger<YourNewCommandHandler>? _logger;
    
    public YourNewCommandHandler(
        IEventStore eventStore,
        IEventDispatcher dispatcher,
        ILogger<YourNewCommandHandler>? logger = null)
    {
        _eventStore = eventStore;
        _dispatcher = dispatcher;
        _logger = logger;
    }
    
    public async Task HandleAsync(YourNewCommand command)
    {
        try
        {
            _logger?.LogInformation("Handling YourNewCommand");
            
            // 1. Validate
            command.Validate();
            
            // 2. Load aggregate
            var events = await _eventStore.GetAllAsync();
            var aggregate = new YourAggregate(command.SomeId);
            aggregate.LoadFromHistory(events);
            
            // 3. Execute command
            aggregate.HandleCommand(command);
            
            // 4. Dispatch events
            foreach (var @event in aggregate.GetUncommittedEvents())
            {
                await _dispatcher.DispatchAsync(@event);
            }
            
            _logger?.LogInformation("Successfully handled YourNewCommand");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling YourNewCommand");
            throw;
        }
    }
}
```

#### Step 5: Register in DI

Add to `ServiceCollectionExtensions.cs`:

```csharp
services.AddTransient<ICommandHandler<YourNewCommand>, YourNewCommandHandler>();
```

#### Step 6: Update Event Validation

Add to `EventValidator.cs`:

```csharp
case YourEventHappened yourEvent:
    return ValidateYourEvent(yourEvent);
```

#### Step 7: Update Event Store Deserialization

Add to `JsonFileEventStore.DeserializeEvent()`:

```csharp
nameof(YourEventHappened) => data.Deserialize<YourEventHappened>(options),
```

#### Step 8: Write Tests

Create test file in `PlaneCrazy.Tests/CommandHandlers/`:

```csharp
[TestFixture]
public class YourNewCommandHandlerTests
{
    [Test]
    public async Task HandleAsync_ValidCommand_CreatesEvent()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var dispatcher = new EventDispatcher(eventStore, projections);
        var handler = new YourNewCommandHandler(eventStore, dispatcher);
        
        var command = new YourNewCommand
        {
            SomeProperty = "test"
        };
        
        // Act
        await handler.HandleAsync(command);
        
        // Assert
        var events = await eventStore.GetAllAsync();
        Assert.That(events.Count(), Is.EqualTo(1));
        Assert.That(events.First(), Is.TypeOf<YourEventHappened>());
    }
}
```

### Adding a New Projection

#### Step 1: Create Projection Class

Create in `PlaneCrazy.Infrastructure/Projections/`:

```csharp
public class YourNewProjection : IProjection
{
    private readonly IEventStore _eventStore;
    private readonly YourRepository _repository;
    
    public string ProjectionName => "YourNewProjection";
    
    public YourNewProjection(
        IEventStore eventStore,
        YourRepository repository)
    {
        _eventStore = eventStore;
        _repository = repository;
    }
    
    public async Task<bool> ApplyEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case YourEventHappened happened:
                await HandleYourEventAsync(happened);
                return true;
            default:
                return false;
        }
    }
    
    private async Task HandleYourEventAsync(YourEventHappened @event)
    {
        var entity = new YourEntity
        {
            Id = @event.Id,
            Property = @event.SomeProperty,
            CreatedAt = @event.OccurredAt
        };
        
        await _repository.SaveAsync(entity);
    }
    
    public async Task RebuildAsync()
    {
        // Clear existing data
        var all = await _repository.GetAllAsync();
        foreach (var item in all)
        {
            await _repository.DeleteAsync(item.Id.ToString());
        }
        
        // Replay all events
        var events = await _eventStore.GetAllAsync();
        foreach (var @event in events)
        {
            await ApplyEventAsync(@event);
        }
    }
}
```

#### Step 2: Create Repository

Create in `PlaneCrazy.Infrastructure/Repositories/`:

```csharp
public class YourRepository : JsonFileRepository<YourEntity>
{
    public YourRepository() : base("your_entities.json")
    {
    }
    
    protected override string GetEntityId(YourEntity entity)
    {
        return entity.Id.ToString();
    }
}
```

#### Step 3: Register in DI

```csharp
services.AddSingleton<YourRepository>();
services.AddSingleton<IProjection, YourNewProjection>();
```

### Adding a New Query Service

#### Step 1: Define Interface

Create in `PlaneCrazy.Domain/Interfaces/`:

```csharp
public interface IYourQueryService
{
    Task<YourQueryResult?> GetByIdAsync(string id);
    Task<IEnumerable<YourQueryResult>> GetAllAsync();
}
```

#### Step 2: Define Query Result DTO

Create in `PlaneCrazy.Domain/Queries/QueryResults/`:

```csharp
public class YourQueryResult
{
    public required string Id { get; init; }
    public string? Property { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

#### Step 3: Implement Service

Create in `PlaneCrazy.Infrastructure/QueryServices/`:

```csharp
public class YourQueryService : IYourQueryService
{
    private readonly YourRepository _repository;
    
    public YourQueryService(YourRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<YourQueryResult?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToQueryResult(entity);
    }
    
    public async Task<IEnumerable<YourQueryResult>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToQueryResult);
    }
    
    private YourQueryResult MapToQueryResult(YourEntity entity)
    {
        return new YourQueryResult
        {
            Id = entity.Id.ToString(),
            Property = entity.Property,
            CreatedAt = entity.CreatedAt
        };
    }
}
```

#### Step 4: Register in DI

```csharp
services.AddSingleton<IYourQueryService, YourQueryService>();
```

### Adding a New Entity

#### Step 1: Create Entity Class

Create in `PlaneCrazy.Domain/Entities/`:

```csharp
public class YourEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Property { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

#### Step 2: Create Repository

Follow repository creation steps above.

## Coding Standards

### Naming Conventions

```csharp
// Classes: PascalCase
public class MyClass { }

// Interfaces: IPascalCase
public interface IMyInterface { }

// Methods: PascalCase
public void MyMethod() { }

// Properties: PascalCase
public string MyProperty { get; set; }

// Private fields: _camelCase
private string _myField;

// Parameters: camelCase
public void Method(string myParameter) { }

// Local variables: camelCase
var myVariable = "";
```

### File Organization

- One class per file
- File name matches class name
- Organize using statements alphabetically
- Group related classes in folders

### Code Style

```csharp
// Use var for local variables when type is obvious
var service = new MyService();

// Use explicit types when not obvious
IEnumerable<string> GetNames() { }

// Use expression-bodied members for simple methods
public string FullName => $"{FirstName} {LastName}";

// Prefer async/await
public async Task<string> GetDataAsync()
{
    return await _repository.GetAsync();
}

// Use nullable reference types
public string? OptionalValue { get; set; }

// Use required properties
public required string RequiredValue { get; init; }
```

### Documentation

```csharp
/// <summary>
/// Adds a comment to an aircraft.
/// </summary>
/// <param name="command">The command containing comment details.</param>
/// <returns>A task representing the asynchronous operation.</returns>
/// <exception cref="ValidationException">Thrown when validation fails.</exception>
public async Task HandleAsync(AddCommentCommand command)
{
    // Implementation
}
```

## Testing Guidelines

### Test Structure

```csharp
[TestFixture]
public class MyClassTests
{
    [SetUp]
    public void SetUp()
    {
        // Initialize test dependencies
    }
    
    [Test]
    public void MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var input = "test";
        
        // Act
        var result = MethodUnderTest(input);
        
        // Assert
        Assert.That(result, Is.Not.Null);
    }
}
```

### Test Coverage

- All public methods should have tests
- Test happy paths and error cases
- Test boundary conditions
- Test business rule validation

## Debugging

### Local Debugging

**Visual Studio**:
1. Set breakpoints (F9)
2. Start debugging (F5)
3. Step through code (F10, F11)

**VS Code**:
1. Set breakpoints (click left margin)
2. Press F5 to start debugging
3. Use Debug toolbar to step through

### Debugging Event Flow

```csharp
// Add logging to see event flow
_logger?.LogInformation("Event {EventType} dispatched at {Time}", 
    @event.EventType, DateTime.UtcNow);
```

### Inspecting Event Store

Event files are stored in:
```
%USERPROFILE%\Documents\PlaneCrazy\Events\
```

View raw JSON:
```bash
cat ~/Documents/PlaneCrazy/Events/*.json
```

### Clearing Test Data

```bash
# Windows
rd /s /q "%USERPROFILE%\Documents\PlaneCrazy"

# Linux/Mac
rm -rf ~/Documents/PlaneCrazy
```

## Common Tasks

### Adding a New Event Type

1. Create event class in `Domain/Events/`
2. Add validation in `EventValidator.cs`
3. Add deserialization in `JsonFileEventStore.cs`
4. Update projections to handle new event
5. Write tests

### Modifying Validation Rules

1. Update validator class in `Domain/Validation/Validators/`
2. Update `CommandValidator.cs` or `EventValidator.cs`
3. Update tests in `Domain.Tests/Validation/`
4. Update documentation in `VALIDATION.md`

### Adding External API Integration

1. Define interface in `Domain/Interfaces/`
2. Create implementation in `Infrastructure/Services/`
3. Register in DI container
4. Add tests with mocked HTTP calls

## Troubleshooting

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Test Failures

```bash
# Run specific test
dotnet test --filter "FullyQualifiedName=Namespace.TestClass.TestMethod"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Runtime Errors

Check logs in console output. Enable detailed logging:

```csharp
// In Program.cs
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

## Resources

### Documentation
- [Architecture](ARCHITECTURE.md)
- [Command Handlers](COMMAND_HANDLERS.md)
- [Projections](PROJECTIONS.md)
- [Queries](QUERIES.md)
- [Testing](TESTING.md)
- [Validation](VALIDATION.md)

### External Resources
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [C# Language Reference](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

## Getting Help

- Check existing documentation
- Search existing issues on GitHub
- Ask in team chat/Slack
- Create a GitHub issue for bugs
- Create a discussion for questions

## Code Review Checklist

Before submitting a pull request:

- [ ] Code builds without errors
- [ ] All tests pass
- [ ] New features have tests
- [ ] Code follows naming conventions
- [ ] Public methods have XML documentation
- [ ] No hardcoded values (use configuration)
- [ ] Logging added for important operations
- [ ] Error handling implemented
- [ ] Validation added for inputs
- [ ] README/docs updated if needed

## Release Process

1. Update version in `.csproj` files
2. Update `CHANGELOG.md`
3. Create release branch
4. Run full test suite
5. Create GitHub release
6. Tag release in Git

---

**Happy Coding!**

*Last Updated: January 24, 2026*
