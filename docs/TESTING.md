# PlaneCrazy Testing Guide

## Overview

This document describes the testing strategy, patterns, and best practices for PlaneCrazy. The application uses xUnit for testing with a focus on unit tests, integration tests, and test-driven development practices.

## Testing Strategy

### Testing Pyramid

```
        ┌─────────────┐
        │  Manual     │  (Minimal - smoke testing)
        ├─────────────┤
        │ Integration │  (Command handlers, event store, projections)
        ├─────────────┤
        │   Unit      │  (Domain logic, validation, aggregates)
        └─────────────┘
```

### Test Coverage Goals

- **Domain Layer**: 90%+ coverage (core business logic)
- **Infrastructure Layer**: 70%+ coverage (integration points)
- **Command Handlers**: 80%+ coverage (critical paths)
- **Projections**: 80%+ coverage (event application)
- **Validation**: 95%+ coverage (all validation rules)

## Test Projects

### PlaneCrazy.Tests
**Location**: `src/PlaneCrazy.Tests/`

**Purpose**: Infrastructure and integration tests

**Structure**:
```
PlaneCrazy.Tests/
├── Aggregates/
│   ├── CommentAggregateTests.cs
│   └── FavouriteAggregateTests.cs
├── CommandHandlers/
│   ├── AddCommentCommandHandlerTests.cs
│   ├── EditCommentCommandHandlerTests.cs
│   └── ...
├── EventStore/
│   └── JsonFileEventStoreTests.cs
├── Helpers/
│   └── TestHelpers.cs
└── PlaneCrazy.Tests.csproj
```

### PlaneCrazy.Domain.Tests
**Location**: `tests/PlaneCrazy.Domain.Tests/`

**Purpose**: Pure domain logic tests

**Structure**:
```
PlaneCrazy.Domain.Tests/
├── Validation/
│   ├── IcaoValidatorTests.cs
│   ├── TypeCodeValidatorTests.cs
│   ├── CommandValidatorTests.cs
│   └── EventValidatorTests.cs
├── Aggregates/
└── Commands/
```

## Test Infrastructure

### TestHelpers

**Location**: `src/PlaneCrazy.Tests/Helpers/TestHelpers.cs`

**Purpose**: Factory methods for creating test data

```csharp
public static class TestHelpers
{
    // Event creation helpers
    public static AircraftFavourited CreateAircraftFavouritedEvent(
        string icao24 = "ABC123",
        string? registration = "N12345",
        string? typeCode = "B738")
    {
        return new AircraftFavourited
        {
            Icao24 = icao24,
            Registration = registration,
            TypeCode = typeCode
        };
    }
    
    public static CommentAdded CreateCommentAddedEvent(
        string entityType = "Aircraft",
        string entityId = "ABC123",
        string text = "Test comment")
    {
        return new CommentAdded
        {
            EntityType = entityType,
            EntityId = entityId,
            Text = text,
            User = "TestUser"
        };
    }
    
    // Timestamp helpers
    public static async Task DelayForDistinctTimestamp()
    {
        await Task.Delay(10); // 10ms delay
    }
}
```

### In-Memory Test Repositories

Create test doubles for repositories:

```csharp
public class InMemoryEventStore : IEventStore
{
    private readonly List<DomainEvent> _events = new();
    
    public Task AppendAsync(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
        return Task.CompletedTask;
    }
    
    public Task<IEnumerable<DomainEvent>> GetAllAsync()
    {
        return Task.FromResult(_events.AsEnumerable());
    }
    
    // ... other interface methods
}
```

## Unit Testing

### Testing Aggregates

**Purpose**: Test business logic and state reconstruction

```csharp
[TestFixture]
public class CommentAggregateTests
{
    [Test]
    public void EditComment_WhenDeleted_ThrowsException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        var addedEvent = new CommentAdded
        {
            Id = commentId,
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Original text",
            User = "TestUser"
        };
        
        var deletedEvent = new CommentDeleted
        {
            CommentId = commentId
        };
        
        aggregate.LoadFromHistory(new[] { addedEvent, deletedEvent });
        
        var editCommand = new EditCommentCommand
        {
            CommentId = commentId,
            Text = "New text"
        };
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            aggregate.EditComment(editCommand));
    }
    
    [Test]
    public void EditComment_WhenValid_GeneratesEvent()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        var addedEvent = new CommentAdded
        {
            Id = commentId,
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Original text",
            User = "TestUser"
        };
        
        aggregate.LoadFromHistory(new[] { addedEvent });
        
        var editCommand = new EditCommentCommand
        {
            CommentId = commentId,
            Text = "Updated text",
            User = "TestUser"
        };
        
        // Act
        aggregate.EditComment(editCommand);
        
        // Assert
        var uncommittedEvents = aggregate.GetUncommittedEvents().ToList();
        Assert.That(uncommittedEvents.Count, Is.EqualTo(1));
        Assert.That(uncommittedEvents[0], Is.TypeOf<CommentEdited>());
        
        var editedEvent = (CommentEdited)uncommittedEvents[0];
        Assert.That(editedEvent.Text, Is.EqualTo("Updated text"));
    }
}
```

### Testing Validation

**Purpose**: Ensure all validation rules work correctly

```csharp
[TestFixture]
public class IcaoValidatorTests
{
    private IcaoValidator _validator;
    
    [SetUp]
    public void SetUp()
    {
        _validator = new IcaoValidator();
    }
    
    [TestCase("ABC123", true)]
    [TestCase("A1B2C3", true)]
    [TestCase("FFFFFF", true)]
    [TestCase("000000", true)]
    public void Validate_ValidIcao_ReturnsSuccess(string icao, bool expected)
    {
        // Act
        var result = _validator.Validate(icao);
        
        // Assert
        Assert.That(result.IsValid, Is.EqualTo(expected));
    }
    
    [TestCase("12345", "ICAO24 must be exactly 6 characters")]
    [TestCase("ABCDEFG", "ICAO24 must be exactly 6 characters")]
    [TestCase("XYZ123", "ICAO24 must contain only hexadecimal characters")]
    [TestCase("", "ICAO24 cannot be empty")]
    [TestCase(null, "ICAO24 cannot be empty")]
    public void Validate_InvalidIcao_ReturnsError(string icao, string expectedError)
    {
        // Act
        var result = _validator.Validate(icao);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain(expectedError));
    }
    
    [Test]
    public void Normalize_ConvertsToUpperCase()
    {
        // Act
        var normalized = IcaoValidator.Normalize("abc123");
        
        // Assert
        Assert.That(normalized, Is.EqualTo("ABC123"));
    }
}
```

### Testing Commands

```csharp
[TestFixture]
public class AddCommentCommandTests
{
    [Test]
    public void Validate_ValidCommand_DoesNotThrow()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Valid comment"
        };
        
        // Act & Assert
        Assert.DoesNotThrow(() => command.Validate());
    }
    
    [Test]
    public void Validate_EmptyText_ThrowsValidationException()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = ""
        };
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => command.Validate());
        Assert.That(ex.Message, Does.Contain("Comment text cannot be empty"));
    }
    
    [Test]
    public void Validate_InvalidEntityType_ThrowsValidationException()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "InvalidType",
            EntityId = "ABC123",
            Text = "Valid comment"
        };
        
        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => command.Validate());
        Assert.That(ex.Message, Does.Contain("Entity type must be one of"));
    }
}
```

## Integration Testing

### Testing Command Handlers

**Purpose**: Test full command execution flow

```csharp
[TestFixture]
public class AddCommentCommandHandlerTests
{
    private IEventStore _eventStore;
    private CommentRepository _commentRepository;
    private CommentProjection _commentProjection;
    private AddCommentCommandHandler _handler;
    
    [SetUp]
    public void SetUp()
    {
        _eventStore = new InMemoryEventStore();
        _commentRepository = new InMemoryCommentRepository();
        _commentProjection = new CommentProjection(_eventStore, _commentRepository);
        _handler = new AddCommentCommandHandler(
            _eventStore, 
            _commentProjection);
    }
    
    [Test]
    public async Task HandleAsync_ValidCommand_AddsCommentToProjection()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Great aircraft!",
            User = "TestUser"
        };
        
        // Act
        await _handler.HandleAsync(command);
        
        // Assert
        var comments = await _commentRepository.GetAllAsync();
        Assert.That(comments.Count(), Is.EqualTo(1));
        
        var comment = comments.First();
        Assert.That(comment.EntityType, Is.EqualTo("Aircraft"));
        Assert.That(comment.EntityId, Is.EqualTo("ABC123"));
        Assert.That(comment.Text, Is.EqualTo("Great aircraft!"));
        Assert.That(comment.CreatedBy, Is.EqualTo("TestUser"));
    }
    
    [Test]
    public async Task HandleAsync_ValidCommand_AppendsEventToStore()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Great aircraft!",
            User = "TestUser"
        };
        
        // Act
        await _handler.HandleAsync(command);
        
        // Assert
        var events = await _eventStore.GetAllAsync();
        Assert.That(events.Count(), Is.EqualTo(1));
        Assert.That(events.First(), Is.TypeOf<CommentAdded>());
        
        var commentAdded = (CommentAdded)events.First();
        Assert.That(commentAdded.Text, Is.EqualTo("Great aircraft!"));
    }
    
    [Test]
    public void HandleAsync_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "" // Empty text
        };
        
        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(async () => 
            await _handler.HandleAsync(command));
    }
}
```

### Testing Event Store

```csharp
[TestFixture]
public class JsonFileEventStoreTests
{
    private string _testDirectory;
    private JsonFileEventStore _eventStore;
    
    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(), 
            $"PlaneCrazyTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Override PlaneCrazyPaths.EventsPath for testing
        _eventStore = new JsonFileEventStore(testPath: _testDirectory);
    }
    
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
    
    [Test]
    public async Task AppendAsync_ValidEvent_CreatesFile()
    {
        // Arrange
        var @event = new CommentAdded
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Test comment",
            User = "TestUser"
        };
        
        // Act
        await _eventStore.AppendAsync(@event);
        
        // Assert
        var files = Directory.GetFiles(_testDirectory, "*.json");
        Assert.That(files.Length, Is.EqualTo(1));
    }
    
    [Test]
    public async Task GetAllAsync_MultipleEvents_ReturnsInOrder()
    {
        // Arrange
        var event1 = TestHelpers.CreateCommentAddedEvent(text: "First");
        var event2 = TestHelpers.CreateCommentAddedEvent(text: "Second");
        var event3 = TestHelpers.CreateCommentAddedEvent(text: "Third");
        
        await _eventStore.AppendAsync(event1);
        await TestHelpers.DelayForDistinctTimestamp();
        await _eventStore.AppendAsync(event2);
        await TestHelpers.DelayForDistinctTimestamp();
        await _eventStore.AppendAsync(event3);
        
        // Act
        var events = await _eventStore.GetAllAsync();
        var commentEvents = events.OfType<CommentAdded>().ToList();
        
        // Assert
        Assert.That(commentEvents.Count, Is.EqualTo(3));
        Assert.That(commentEvents[0].Text, Is.EqualTo("First"));
        Assert.That(commentEvents[1].Text, Is.EqualTo("Second"));
        Assert.That(commentEvents[2].Text, Is.EqualTo("Third"));
    }
}
```

### Testing Projections

```csharp
[TestFixture]
public class FavouriteProjectionTests
{
    private IEventStore _eventStore;
    private FavouriteRepository _repository;
    private FavouriteProjection _projection;
    
    [SetUp]
    public void SetUp()
    {
        _eventStore = new InMemoryEventStore();
        _repository = new InMemoryFavouriteRepository();
        _projection = new FavouriteProjection(_eventStore, _repository);
    }
    
    [Test]
    public async Task ApplyEventAsync_AircraftFavourited_AddsToRepository()
    {
        // Arrange
        var @event = TestHelpers.CreateAircraftFavouritedEvent(
            icao24: "ABC123",
            registration: "N12345",
            typeCode: "B738");
        
        // Act
        var handled = await _projection.ApplyEventAsync(@event);
        
        // Assert
        Assert.That(handled, Is.True);
        
        var favourites = await _repository.GetAllAsync();
        Assert.That(favourites.Count(), Is.EqualTo(1));
        
        var favourite = favourites.First();
        Assert.That(favourite.EntityType, Is.EqualTo("Aircraft"));
        Assert.That(favourite.EntityId, Is.EqualTo("ABC123"));
        Assert.That(favourite.Metadata["Registration"], Is.EqualTo("N12345"));
    }
    
    [Test]
    public async Task RebuildAsync_WithEvents_RebuildsCorrectly()
    {
        // Arrange
        await _eventStore.AppendAsync(
            TestHelpers.CreateAircraftFavouritedEvent("A1"));
        await _eventStore.AppendAsync(
            TestHelpers.CreateAircraftFavouritedEvent("A2"));
        await _eventStore.AppendAsync(
            TestHelpers.CreateAircraftUnfavouritedEvent("A1"));
        
        // Act
        await _projection.RebuildAsync();
        
        // Assert
        var favourites = await _repository.GetAllAsync();
        Assert.That(favourites.Count(), Is.EqualTo(1));
        Assert.That(favourites.First().EntityId, Is.EqualTo("A2"));
    }
}
```

## Test Patterns

### Arrange-Act-Assert (AAA)

```csharp
[Test]
public async Task TestMethod()
{
    // Arrange - Setup test data and dependencies
    var command = new AddCommentCommand { ... };
    var handler = new AddCommentCommandHandler(...);
    
    // Act - Execute the code being tested
    await handler.HandleAsync(command);
    
    // Assert - Verify the results
    Assert.That(result, Is.EqualTo(expected));
}
```

### Builder Pattern for Test Data

```csharp
public class AircraftBuilder
{
    private string _icao24 = "ABC123";
    private string _registration = "N12345";
    
    public AircraftBuilder WithIcao(string icao24)
    {
        _icao24 = icao24;
        return this;
    }
    
    public AircraftBuilder WithRegistration(string registration)
    {
        _registration = registration;
        return this;
    }
    
    public Aircraft Build()
    {
        return new Aircraft
        {
            Icao24 = _icao24,
            Registration = _registration,
            FirstSeen = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow
        };
    }
}

// Usage
var aircraft = new AircraftBuilder()
    .WithIcao("XYZ789")
    .WithRegistration("G-ABCD")
    .Build();
```

### Test Fixtures for Shared Setup

```csharp
[TestFixture]
public class CommandHandlerTestFixture
{
    protected IEventStore EventStore;
    protected CommentRepository CommentRepository;
    protected CommentProjection CommentProjection;
    
    [SetUp]
    public void BaseSetUp()
    {
        EventStore = new InMemoryEventStore();
        CommentRepository = new InMemoryCommentRepository();
        CommentProjection = new CommentProjection(EventStore, CommentRepository);
    }
}

[TestFixture]
public class AddCommentCommandHandlerTests : CommandHandlerTestFixture
{
    [Test]
    public async Task TestMethod()
    {
        // EventStore, CommentRepository available from base class
    }
}
```

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run specific project
dotnet test src/PlaneCrazy.Tests/PlaneCrazy.Tests.csproj

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests and generate coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName=PlaneCrazy.Tests.AddCommentCommandHandlerTests.HandleAsync_ValidCommand_AddsComment"

# Run tests in category
dotnet test --filter "Category=Integration"
```

### Visual Studio

1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" or select specific tests
3. View results in Test Explorer window

### VS Code

1. Install C# Dev Kit extension
2. Tests appear in Testing sidebar
3. Click play button to run tests

## Test Organization

### Naming Conventions

```csharp
// Method name pattern: MethodName_Scenario_ExpectedResult

[Test]
public void Validate_EmptyIcao_ReturnsError() { }

[Test]
public async Task HandleAsync_ValidCommand_AddsComment() { }

[Test]
public void EditComment_WhenDeleted_ThrowsException() { }
```

### Test Categories

```csharp
[TestFixture]
[Category("Unit")]
public class ValidatorTests { }

[TestFixture]
[Category("Integration")]
public class CommandHandlerTests { }

[TestFixture]
[Category("Slow")]
public class EventStoreTests { }
```

## Mocking

### Using Moq (if added)

```csharp
[Test]
public async Task TestWithMock()
{
    // Arrange
    var mockEventStore = new Mock<IEventStore>();
    mockEventStore
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(new List<DomainEvent>());
    
    var projection = new FavouriteProjection(
        mockEventStore.Object, 
        repository);
    
    // Act
    await projection.RebuildAsync();
    
    // Assert
    mockEventStore.Verify(x => x.GetAllAsync(), Times.Once);
}
```

### Without Mocking (Pure Test Doubles)

```csharp
public class FakeEventStore : IEventStore
{
    public bool AppendCalled { get; private set; }
    public DomainEvent LastEvent { get; private set; }
    
    public Task AppendAsync(DomainEvent domainEvent)
    {
        AppendCalled = true;
        LastEvent = domainEvent;
        return Task.CompletedTask;
    }
}

// Usage
var fakeEventStore = new FakeEventStore();
// ... use in test
Assert.That(fakeEventStore.AppendCalled, Is.True);
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run tests
      run: dotnet test --no-build --verbosity normal
    
    - name: Generate coverage
      run: dotnet test /p:CollectCoverage=true
```

## Best Practices

1. **Test Independence**: Each test should be independent and run in isolation
2. **Fast Tests**: Unit tests should run in milliseconds
3. **Descriptive Names**: Test names should clearly describe what is being tested
4. **One Assert Per Test**: Focus each test on a single behavior (when practical)
5. **Avoid Logic in Tests**: Tests should be simple and straightforward
6. **Use Test Helpers**: Extract common setup to helper methods
7. **Clean Up**: Dispose resources, delete temp files in teardown
8. **Test Edge Cases**: Test boundary conditions and error paths
9. **Keep Tests Maintainable**: Refactor tests as you refactor code
10. **Run Tests Often**: Run tests before committing code

## Common Testing Scenarios

### Testing Event Replay

```csharp
[Test]
public async Task Aggregate_ReplayMultipleEvents_RebuildsState()
{
    // Arrange
    var commentId = Guid.NewGuid();
    var aggregate = new CommentAggregate(commentId);
    
    var events = new DomainEvent[]
    {
        new CommentAdded { Id = commentId, Text = "Original" },
        new CommentEdited { CommentId = commentId, Text = "Edited 1" },
        new CommentEdited { CommentId = commentId, Text = "Edited 2" }
    };
    
    // Act
    aggregate.LoadFromHistory(events);
    
    // Assert
    Assert.That(aggregate.Version, Is.EqualTo(3));
    // Additional state assertions
}
```

### Testing Async Operations

```csharp
[Test]
public async Task AsyncMethod_CompletesSuccessfully()
{
    // Arrange
    var service = new MyAsyncService();
    
    // Act
    var result = await service.DoSomethingAsync();
    
    // Assert
    Assert.That(result, Is.Not.Null);
}
```

### Testing Exceptions

```csharp
[Test]
public void Method_InvalidInput_ThrowsException()
{
    // Arrange
    var validator = new IcaoValidator();
    
    // Act & Assert
    var ex = Assert.Throws<ValidationException>(() => 
        validator.ValidateRequired(null));
    Assert.That(ex.Message, Does.Contain("cannot be empty"));
}
```

## Troubleshooting Tests

### Tests Fail Intermittently

**Cause**: Race conditions, timing issues

**Solution**:
- Add small delays between operations
- Use async/await consistently
- Ensure proper test isolation

### Tests Run Slowly

**Cause**: Too many integration tests, file I/O

**Solution**:
- Use in-memory test doubles
- Mock external dependencies
- Run integration tests separately

### Tests Pass Locally But Fail in CI

**Cause**: Environment differences, file paths

**Solution**:
- Use relative paths
- Clean up test data in teardown
- Check for timezone/culture differences

---

*Last Updated: January 24, 2026*
