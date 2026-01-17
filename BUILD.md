# Build and Test Commands

## Quick Start

### Build Everything
```bash
dotnet build
```

### Run All Tests
```bash
dotnet test
```

### Run Console Application
```bash
dotnet run --project src/PlaneCrazy.Console
```

### Run Blazor Application
```bash
dotnet run --project src/PlaneCrazy.Blazor
```

## Detailed Commands

### Building

Build the entire solution:
```bash
dotnet build PlaneCrazy.sln
```

Build a specific project:
```bash
dotnet build src/PlaneCrazy.Core
dotnet build src/PlaneCrazy.Console
dotnet build src/PlaneCrazy.Blazor
dotnet build tests/PlaneCrazy.Core.Tests
```

Clean and rebuild:
```bash
dotnet clean
dotnet build
```

### Testing

Run all tests:
```bash
dotnet test
```

Run tests with detailed output:
```bash
dotnet test --verbosity detailed
```

Run tests with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run specific test project:
```bash
dotnet test tests/PlaneCrazy.Core.Tests
```

Run tests in watch mode (for TDD):
```bash
dotnet watch test --project tests/PlaneCrazy.Core.Tests
```

### Running Applications

#### Console Application

Run with default settings:
```bash
dotnet run --project src/PlaneCrazy.Console
```

Run in release mode:
```bash
dotnet run --project src/PlaneCrazy.Console --configuration Release
```

#### Blazor Application

Run with default settings (will open browser):
```bash
dotnet run --project src/PlaneCrazy.Blazor
```

Run on specific port:
```bash
dotnet run --project src/PlaneCrazy.Blazor --urls "http://localhost:5050"
```

Run in watch mode (hot reload):
```bash
dotnet watch --project src/PlaneCrazy.Blazor
```

### Development Workflow

Watch for changes and rebuild:
```bash
dotnet watch --project src/PlaneCrazy.Core
```

Watch tests (runs tests on file changes):
```bash
dotnet watch test --project tests/PlaneCrazy.Core.Tests
```

### Package Management

Add a package to a project:
```bash
dotnet add src/PlaneCrazy.Core package <PackageName>
```

Remove a package:
```bash
dotnet remove src/PlaneCrazy.Core package <PackageName>
```

List packages:
```bash
dotnet list package
```

Restore all packages:
```bash
dotnet restore
```

### Project Management

Add a new project to solution:
```bash
dotnet sln add <path-to-csproj>
```

List projects in solution:
```bash
dotnet sln list
```

Add project reference:
```bash
dotnet add <project> reference <reference-project>
```

### Publishing

Publish console app:
```bash
dotnet publish src/PlaneCrazy.Console -c Release -o ./publish/console
```

Publish Blazor app:
```bash
dotnet publish src/PlaneCrazy.Blazor -c Release -o ./publish/blazor
```

Create self-contained executable:
```bash
dotnet publish src/PlaneCrazy.Console -c Release -r win-x64 --self-contained
dotnet publish src/PlaneCrazy.Console -c Release -r linux-x64 --self-contained
dotnet publish src/PlaneCrazy.Console -c Release -r osx-x64 --self-contained
```

## Troubleshooting

### Clear build artifacts
```bash
dotnet clean
rm -rf **/bin **/obj
```

### Restore with force
```bash
dotnet restore --force --no-cache
```

### Check .NET version
```bash
dotnet --version
dotnet --info
```

### List installed SDKs
```bash
dotnet --list-sdks
```

### List installed runtimes
```bash
dotnet --list-runtimes
```

## Continuous Integration

Example commands for CI/CD pipelines:

```bash
# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release --no-restore

# Run tests
dotnet test --configuration Release --no-build --verbosity normal

# Publish applications
dotnet publish src/PlaneCrazy.Console --configuration Release --output ./artifacts/console
dotnet publish src/PlaneCrazy.Blazor --configuration Release --output ./artifacts/blazor
```

## Performance Testing

Build in Release mode for performance testing:
```bash
dotnet build --configuration Release
dotnet run --project src/PlaneCrazy.Console --configuration Release
dotnet run --project src/PlaneCrazy.Blazor --configuration Release
```

## Code Analysis

Run code analyzers:
```bash
dotnet build /p:RunAnalyzers=true
```

Format code:
```bash
dotnet format
```

Check code style:
```bash
dotnet format --verify-no-changes
```
