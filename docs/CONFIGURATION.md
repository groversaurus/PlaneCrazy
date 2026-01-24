# PlaneCrazy Configuration Guide

## Overview

This document describes all configuration options available in PlaneCrazy, including environment variables, configuration files, and runtime settings.

## Configuration Sources

PlaneCrazy uses the .NET configuration system with the following hierarchy (later sources override earlier ones):

1. **Hardcoded Defaults** - Built into the application
2. **appsettings.json** - Base configuration file
3. **appsettings.{Environment}.json** - Environment-specific overrides
4. **Environment Variables** - OS-level configuration
5. **Command Line Arguments** - Runtime overrides

## Application Settings

### appsettings.json

**Location**: `src/PlaneCrazy.Console/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "System": "Warning"
    }
  },
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 30
  }
}
```

### Environment-Specific Files

**Development**: `appsettings.Development.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "PollerConfiguration": {
    "IntervalSeconds": 10
  }
}
```

**Production**: `appsettings.Production.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 30
  }
}
```

## Configuration Sections

### Logging Configuration

Controls application logging behavior.

**Schema**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information | Debug | Warning | Error | Critical | None",
      "Microsoft": "...",
      "System": "...",
      "PlaneCrazy": "..."
    }
  }
}
```

**Log Levels**:
- **Trace**: Very detailed diagnostic information
- **Debug**: Internal system events for debugging
- **Information**: General application flow
- **Warning**: Abnormal or unexpected events
- **Error**: Errors and exceptions
- **Critical**: Unrecoverable application/system crashes
- **None**: Disable logging

**Example**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "System": "Warning",
      "PlaneCrazy.Infrastructure.Services": "Debug"
    }
  }
}
```

**Best Practices**:
- **Development**: Use `Debug` or `Trace` for detailed logs
- **Production**: Use `Information` to reduce noise
- **Performance**: Use `Warning` or higher for high-throughput scenarios

---

### Poller Configuration

Controls the background aircraft data polling service.

**Schema**:
```csharp
public class PollerConfiguration
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
}
```

**Configuration**:
```json
{
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 30
  }
}
```

**Properties**:

**`Enabled`** (bool, default: `true`)
- Whether to run the background poller
- Set to `false` to disable automatic aircraft fetching
- Useful for development or testing scenarios

**`IntervalSeconds`** (int, default: `30`)
- How often to poll the API (in seconds)
- Minimum: 5 seconds (to avoid API abuse)
- Recommended: 30-60 seconds
- Higher values reduce API load but decrease data freshness

**Usage**:
```csharp
// In Program.cs
services.Configure<PollerConfiguration>(
    hostContext.Configuration.GetSection(nameof(PollerConfiguration)));
```

**Environment Variable Override**:
```powershell
$env:PollerConfiguration__Enabled = "false"
$env:PollerConfiguration__IntervalSeconds = "60"
```

---

### Data Path Configuration

Controls where PlaneCrazy stores its data files.

**Default Location**:
- **Windows**: `C:\Users\{Username}\Documents\PlaneCrazy\`
- **Linux**: `/home/{username}/Documents/PlaneCrazy/`
- **macOS**: `/Users/{username}/Documents/PlaneCrazy/`

**Override via Environment Variable**:
```powershell
# Windows
$env:PLANECRAZY_DATA_PATH = "C:\PlaneCrazy\Data"

# Linux/macOS
export PLANECRAZY_DATA_PATH="/opt/planecrazy/data"
```

**Override via Code**:
```csharp
// In PlaneCrazyPaths.cs
public static string GetDocumentsPath()
{
    var customPath = Environment.GetEnvironmentVariable("PLANECRAZY_DATA_PATH");
    if (!string.IsNullOrEmpty(customPath))
    {
        return customPath;
    }
    
    // Default path
    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "PlaneCrazy");
}
```

**Directory Structure**:
```
{DataPath}/
├── Events/               # Event store files
│   ├── 20260124_event1.json
│   └── 20260124_event2.json
├── Repositories/         # Entity storage
│   ├── aircraft.json
│   ├── comments.json
│   └── favourites.json
└── Projections/         # Projection state
    ├── aircraft_state.json
    ├── comments.json
    └── favourites.json
```

---

### API Configuration

Configuration for external API integration.

**Current Implementation**: Hardcoded in `AdsbFiAircraftService.cs`

**Future Configuration**:
```json
{
  "ApiConfiguration": {
    "BaseUrl": "https://api.adsb.fi/v2",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "BoundingBox": {
      "MinLatitude": 35.0,
      "MinLongitude": -10.0,
      "MaxLatitude": 70.0,
      "MaxLongitude": 40.0
    }
  }
}
```

**Properties**:
- **BaseUrl**: API endpoint base URL
- **TimeoutSeconds**: HTTP request timeout
- **RetryCount**: Number of retry attempts on failure
- **BoundingBox**: Geographic area to query

**Common Bounding Boxes**:

**Europe**:
```json
{
  "MinLatitude": 35.0,
  "MinLongitude": -10.0,
  "MaxLatitude": 70.0,
  "MaxLongitude": 40.0
}
```

**North America**:
```json
{
  "MinLatitude": 25.0,
  "MinLongitude": -125.0,
  "MaxLatitude": 50.0,
  "MaxLongitude": -65.0
}
```

**United Kingdom**:
```json
{
  "MinLatitude": 49.5,
  "MinLongitude": -8.0,
  "MaxLatitude": 61.0,
  "MaxLongitude": 2.0
}
```

---

## Environment Variables

### Standard .NET Variables

**`DOTNET_ENVIRONMENT`**
- Sets the application environment
- Values: `Development`, `Staging`, `Production`
- Determines which `appsettings.{Environment}.json` to load

```powershell
# Development mode
$env:DOTNET_ENVIRONMENT = "Development"

# Production mode
$env:DOTNET_ENVIRONMENT = "Production"
```

**`ASPNETCORE_ENVIRONMENT`**
- Alias for `DOTNET_ENVIRONMENT` (legacy)

---

### PlaneCrazy-Specific Variables

**`PLANECRAZY_DATA_PATH`**
- Custom data directory path
- Overrides default Documents folder

```powershell
$env:PLANECRAZY_DATA_PATH = "C:\CustomPath\PlaneCrazy"
```

**`PollerConfiguration__Enabled`**
- Enable/disable background poller
- Values: `true`, `false`

```powershell
$env:PollerConfiguration__Enabled = "false"
```

**`PollerConfiguration__IntervalSeconds`**
- Polling interval in seconds
- Values: Integer >= 5

```powershell
$env:PollerConfiguration__IntervalSeconds = "60"
```

**`Logging__LogLevel__Default`**
- Default log level
- Values: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

```powershell
$env:Logging__LogLevel__Default = "Debug"
```

---

## Command Line Arguments

Override configuration at runtime.

**Syntax**:
```powershell
dotnet run --project src/PlaneCrazy.Console -- --key=value --section:key=value
```

**Examples**:

**Set log level**:
```powershell
dotnet run -- --Logging:LogLevel:Default=Debug
```

**Disable poller**:
```powershell
dotnet run -- --PollerConfiguration:Enabled=false
```

**Set polling interval**:
```powershell
dotnet run -- --PollerConfiguration:IntervalSeconds=60
```

**Multiple overrides**:
```powershell
dotnet run -- `
  --Logging:LogLevel:Default=Debug `
  --PollerConfiguration:IntervalSeconds=10
```

---

## Docker Configuration

### Environment Variables in Docker

**Docker Run**:
```bash
docker run -d \
  --name planecrazy \
  -e DOTNET_ENVIRONMENT=Production \
  -e PollerConfiguration__IntervalSeconds=60 \
  -e PLANECRAZY_DATA_PATH=/data \
  -v planecrazy-data:/data \
  planecrazy:latest
```

**Docker Compose**:
```yaml
version: '3.8'

services:
  planecrazy:
    image: planecrazy:latest
    environment:
      - DOTNET_ENVIRONMENT=Production
      - PollerConfiguration__Enabled=true
      - PollerConfiguration__IntervalSeconds=30
      - PLANECRAZY_DATA_PATH=/data
      - Logging__LogLevel__Default=Information
    volumes:
      - planecrazy-data:/data
```

---

## Configuration Best Practices

### 1. Use Environment-Specific Files

**Development**:
- Verbose logging
- Shorter polling intervals
- Debug mode enabled

**Production**:
- Minimal logging
- Optimized intervals
- Release mode

### 2. Secure Sensitive Data

**Don't**:
```json
{
  "ApiKey": "super-secret-key"  // ❌ Don't commit secrets
}
```

**Do**:
```json
{
  "ApiKey": ""  // ✅ Override with environment variable
}
```

```powershell
$env:ApiKey = "super-secret-key"
```

### 3. Document Configuration

Always document custom configuration options in this file.

### 4. Validate Configuration

Add validation at startup:
```csharp
var pollerConfig = configuration.GetSection("PollerConfiguration")
    .Get<PollerConfiguration>();

if (pollerConfig.IntervalSeconds < 5)
{
    throw new InvalidOperationException(
        "PollerConfiguration.IntervalSeconds must be at least 5");
}
```

### 5. Use Strongly-Typed Configuration

**Prefer**:
```csharp
services.Configure<PollerConfiguration>(
    configuration.GetSection(nameof(PollerConfiguration)));

// Usage
public class BackgroundAdsBPoller
{
    private readonly PollerConfiguration _config;
    
    public BackgroundAdsBPoller(IOptions<PollerConfiguration> options)
    {
        _config = options.Value;
    }
}
```

**Over**:
```csharp
// ❌ Avoid string-based configuration access
var interval = configuration["PollerConfiguration:IntervalSeconds"];
```

---

## Configuration Templates

### Minimal Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 30
  }
}
```

### Development Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "System": "Information"
    }
  },
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 10
  }
}
```

### Production Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 30
  }
}
```

### High-Frequency Monitoring

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "PollerConfiguration": {
    "Enabled": true,
    "IntervalSeconds": 5
  }
}
```

### Background Service Disabled

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "PollerConfiguration": {
    "Enabled": false,
    "IntervalSeconds": 30
  }
}
```

---

## Troubleshooting Configuration

### Configuration Not Loading

**Check**:
1. File exists in correct location
2. File is valid JSON
3. File is included in publish output

**Solution**:
```xml
<!-- In .csproj -->
<ItemGroup>
  <None Update="appsettings*.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Environment Variables Not Applied

**Check**:
1. Variable name format: `Section__Property`
2. Environment is set: `DOTNET_ENVIRONMENT`
3. Process has been restarted after setting variable

**Verify**:
```powershell
# List all environment variables
Get-ChildItem Env: | Where-Object { $_.Name -like "*PLANECRAZY*" }
```

### Configuration Precedence Issues

**Remember**: Later sources override earlier ones
1. appsettings.json
2. appsettings.{Environment}.json
3. Environment variables
4. Command line arguments

**Debug**:
```csharp
// Add logging to see final configuration
var config = serviceProvider.GetRequiredService<IConfiguration>();
Console.WriteLine($"Enabled: {config["PollerConfiguration:Enabled"]}");
Console.WriteLine($"Interval: {config["PollerConfiguration:IntervalSeconds"]}");
```

---

## Future Configuration Enhancements

1. **User Preferences**: Per-user configuration file
2. **Configuration UI**: Visual configuration editor
3. **Hot Reload**: Change configuration without restart
4. **Configuration Validation**: Schema validation at startup
5. **Configuration Profiles**: Predefined configuration sets
6. **Remote Configuration**: Load from cloud/database
7. **Configuration History**: Track configuration changes
8. **Configuration Secrets**: Integration with secret managers

---

## Configuration Schema

### JSON Schema (Draft)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "PlaneCrazy Configuration",
  "type": "object",
  "properties": {
    "Logging": {
      "type": "object",
      "properties": {
        "LogLevel": {
          "type": "object",
          "additionalProperties": {
            "type": "string",
            "enum": ["Trace", "Debug", "Information", "Warning", "Error", "Critical", "None"]
          }
        }
      }
    },
    "PollerConfiguration": {
      "type": "object",
      "properties": {
        "Enabled": {
          "type": "boolean",
          "default": true
        },
        "IntervalSeconds": {
          "type": "integer",
          "minimum": 5,
          "default": 30
        }
      },
      "required": ["Enabled", "IntervalSeconds"]
    }
  }
}
```

---

*Last Updated: January 24, 2026*
