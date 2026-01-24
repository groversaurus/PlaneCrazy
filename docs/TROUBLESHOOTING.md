# PlaneCrazy Troubleshooting Guide

## Overview

This guide provides solutions to common issues encountered when running, developing, or deploying PlaneCrazy.

## Common Issues

### Application Won't Start

#### Symptom
Application crashes immediately or shows error during startup

#### Possible Causes & Solutions

**1. .NET Runtime Not Installed**

```powershell
# Check .NET version
dotnet --version
```

**Solution**: Install .NET 10.0 SDK from https://dot.net

---

**2. Missing Dependencies**

```powershell
# Restore NuGet packages
dotnet restore
```

**Solution**: Run `dotnet restore` in solution directory

---

**3. Data Directory Permission Issues**

**Error Message**: `UnauthorizedAccessException: Access to the path '...' is denied`

**Solution (Windows)**:
```powershell
# Grant full permissions to current user
icacls "$env:USERPROFILE\Documents\PlaneCrazy" /grant "${env:USERNAME}:(OI)(CI)F"
```

**Solution (Linux)**:
```bash
# Change ownership
sudo chown -R $USER:$USER ~/Documents/PlaneCrazy

# Set permissions
chmod -R 755 ~/Documents/PlaneCrazy
```

---

**4. Port Already in Use**

**Error Message**: `System.Net.Sockets.SocketException: Only one usage of each socket address`

**Solution**: This is a console application without network listeners, but if modified:
```powershell
# Find process using port
netstat -ano | findstr :5000

# Kill process
taskkill /PID <process-id> /F
```

---

### No Aircraft Data Appearing

#### Symptom
Application runs but no aircraft are fetched or displayed

#### Possible Causes & Solutions

**1. Background Poller Disabled**

**Check**: Look for "Background poller is disabled" in logs

**Solution**: Enable poller in configuration
```csharp
// In Program.cs or appsettings.json
services.Configure<PollerConfiguration>(options =>
{
    options.Enabled = true;
    options.IntervalSeconds = 30;
});
```

---

**2. Network Connectivity Issues**

**Test API Connection**:
```powershell
# Test adsb.fi API
Invoke-RestMethod -Uri "https://api.adsb.fi/v2/lat/35.0/lon/-10.0/lat/70.0/lon/40.0"
```

**Possible Issues**:
- Firewall blocking outbound HTTPS
- Corporate proxy requiring configuration
- Internet connection down
- API endpoint unavailable

**Solution (Proxy)**:
```powershell
# Set proxy (if required)
$env:HTTP_PROXY = "http://proxy.company.com:8080"
$env:HTTPS_PROXY = "http://proxy.company.com:8080"
```

---

**3. No Aircraft in Bounding Box**

**Check**: Area covered by bounding box

**Current Default**: Europe (lat: 35-70, lon: -10 to 40)

**Solution**: Adjust bounding box in `AdsbFiAircraftService.cs`
```csharp
// Example: North America
var lat1 = 25.0;   // Southern US/Mexico
var lon1 = -125.0; // West Coast
var lat2 = 50.0;   // Canada
var lon2 = -65.0;  // East Coast
```

---

**4. API Rate Limited or Unavailable**

**Check Logs**: Look for `HttpRequestException` or `TaskCanceledException`

**Solution**:
- Increase polling interval
- Wait and retry (API may be temporarily down)
- Switch to alternative API provider

---

### Application Crashes During Operation

#### Symptom
Application runs for a while then crashes or freezes

#### Possible Causes & Solutions

**1. Out of Memory**

**Symptoms**:
- Increasing memory usage over time
- `OutOfMemoryException`
- Application slow or unresponsive

**Diagnosis**:
```powershell
# Monitor memory usage
Get-Process PlaneCrazy.Console | Select-Object WorkingSet64
```

**Solution**:
- Reduce polling frequency
- Implement event archiving
- Clear old projection data
- Restart application periodically

---

**2. Corrupted Event Store**

**Symptoms**:
- `JsonException` during event loading
- Application crashes on startup
- Projection rebuild fails

**Diagnosis**:
```powershell
# Check for malformed JSON files
Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | ForEach-Object {
    try {
        Get-Content $_.FullName | ConvertFrom-Json | Out-Null
    }
    catch {
        Write-Host "Corrupted file: $($_.FullName)"
    }
}
```

**Solution**:
```powershell
# Backup current data
Copy-Item -Path "$env:USERPROFILE\Documents\PlaneCrazy" -Destination "$env:USERPROFILE\Documents\PlaneCrazy_Backup" -Recurse

# Remove corrupted file
Remove-Item "$env:USERPROFILE\Documents\PlaneCrazy\Events\<corrupted-file>.json"

# Rebuild projections
# (Application will do this automatically on next start)
```

---

**3. Unhandled Exception in Background Service**

**Symptoms**:
- Application stops fetching data
- "Background poller stopped" in logs

**Check Logs**: Look for exception stack traces

**Solution**:
- Review exception details
- Check API availability
- Restart application
- Report bug if reproducible

---

### Data Not Persisting

#### Symptom
Favourites, comments, or aircraft data lost after restart

#### Possible Causes & Solutions

**1. Wrong Data Directory**

**Check**: Verify data path
```powershell
# Default location
ls "$env:USERPROFILE\Documents\PlaneCrazy"
```

**Solution**: Set explicit data path
```powershell
$env:PLANECRAZY_DATA_PATH = "C:\PlaneCrazy\Data"
```

---

**2. Insufficient Disk Space**

**Check**:
```powershell
Get-PSDrive C | Select-Object Used, Free
```

**Solution**:
- Free up disk space
- Move data directory to larger drive
- Archive old events

---

**3. File Lock Issues**

**Error**: `IOException: The process cannot access the file because it is being used by another process`

**Solution**:
- Close other instances of application
- Check for file locks: `Handle.exe` (Sysinternals)
- Restart system to clear locks

---

### Performance Issues

#### Symptom
Application slow, high CPU usage, or delayed responses

#### Possible Causes & Solutions

**1. Too Many Events**

**Diagnosis**:
```powershell
# Count events
(Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json").Count

# Check total size
(Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | Measure-Object -Property Length -Sum).Sum / 1MB
```

**Solution**:
- Archive old events (> 1 year)
- Implement event snapshotting
- Increase memory if possible

---

**2. Frequent API Polling**

**Check**: Polling interval in configuration

**Solution**: Increase interval
```csharp
services.Configure<PollerConfiguration>(options =>
{
    options.IntervalSeconds = 60; // Increase from 30 to 60
});
```

---

**3. Large Projection Rebuilds**

**Symptom**: Slow startup or projection updates

**Solution**:
- Use snapshots for projections
- Optimize projection logic
- Consider incremental updates only

---

### Validation Errors

#### Symptom
Commands fail with validation errors

#### Common Validation Issues

**1. Invalid ICAO24**

**Error**: `ICAO24 must be exactly 6 hexadecimal characters`

**Valid Examples**: `ABC123`, `4D2228`, `A1B2C3`  
**Invalid Examples**: `ABCDEF1` (7 chars), `XYZ123` (invalid hex), `abc` (too short)

**Solution**: Ensure ICAO24 is exactly 6 hex characters (0-9, A-F)

---

**2. Invalid Registration**

**Error**: `Registration must be 1-10 alphanumeric characters with hyphens`

**Valid Examples**: `N12345`, `G-ABCD`, `D-AIZT`  
**Invalid Examples**: `N12345X` (too long), `G_ABCD` (underscore not allowed)

**Solution**: Use format like `XX-XXXXX` or `XXXXX`

---

**3. Invalid Type Code**

**Error**: `Type code must be 2-10 alphanumeric characters`

**Valid Examples**: `B738`, `A320`, `B77W`  
**Invalid Examples**: `B` (too short), `Boeing737` (too long)

**Solution**: Use ICAO aircraft type codes (2-4 characters typical)

---

**4. Invalid Airport ICAO**

**Error**: `Airport ICAO code must be exactly 4 uppercase letters`

**Valid Examples**: `KJFK`, `EGLL`, `LFPG`  
**Invalid Examples**: `JFK` (too short), `kjfk` (lowercase), `K1FK` (contains digit)

**Solution**: Use 4-letter ICAO airport codes (not IATA codes)

---

**5. Invalid Comment Text**

**Error**: `Comment text must be between 1 and 5000 characters`

**Solution**: Ensure comment is not empty and not too long

---

### Testing Issues

#### Symptom
Tests fail or behave unexpectedly

#### Possible Causes & Solutions

**1. Test Data Conflicts**

**Symptom**: Tests pass individually but fail when run together

**Solution**: Ensure test isolation
```csharp
[SetUp]
public void Setup()
{
    // Clear test data before each test
    _eventStore = new JsonFileEventStore(testEventStoreDirectory);
    // Clean test directory
}

[TearDown]
public void TearDown()
{
    // Clean up after test
    if (Directory.Exists(testEventStoreDirectory))
    {
        Directory.Delete(testEventStoreDirectory, true);
    }
}
```

---

**2. Asynchronous Test Issues**

**Symptom**: Intermittent test failures, race conditions

**Solution**: Properly await async operations
```csharp
[Test]
public async Task TestAsync()
{
    // ❌ Wrong
    var result = handler.Handle(command);
    
    // ✅ Correct
    var result = await handler.Handle(command);
}
```

---

**3. Mock Configuration Issues**

**Symptom**: NullReferenceException in tests

**Solution**: Properly configure mocks
```csharp
var mockService = new Mock<IAircraftDataService>();
mockService.Setup(s => s.FetchAircraftAsync())
    .ReturnsAsync(new List<Aircraft> { /* test data */ });
```

---

### Build Errors

#### Common Build Issues

**1. Package Restore Failed**

```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

---

**2. SDK Version Mismatch**

**Error**: `The current .NET SDK does not support targeting .NET 10.0`

**Solution**: Install .NET 10.0 SDK or downgrade target framework

---

**3. Reference Errors**

**Error**: `The type or namespace name 'X' could not be found`

**Solution**:
```powershell
# Clean solution
dotnet clean

# Rebuild
dotnet build
```

---

## Diagnostic Commands

### Check Application Health

```powershell
# Check if running
Get-Process PlaneCrazy.Console -ErrorAction SilentlyContinue

# Check resource usage
Get-Process PlaneCrazy.Console | Select-Object CPU, WorkingSet64

# View recent events
Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 10 Name, LastWriteTime
```

### Analyze Event Store

```powershell
# Count events by type
Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | ForEach-Object {
    $event = Get-Content $_.FullName | ConvertFrom-Json
    $event.eventType
} | Group-Object | Select-Object Name, Count

# Find oldest event
Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | 
    Sort-Object CreationTime | 
    Select-Object -First 1 Name, CreationTime

# Calculate total size
$totalSize = (Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | 
    Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Total event store size: $totalSize MB"
```

### Test API Connectivity

```powershell
# Test adsb.fi API
try {
    $response = Invoke-RestMethod -Uri "https://api.adsb.fi/v2/lat/35.0/lon/-10.0/lat/70.0/lon/40.0" -TimeoutSec 10
    Write-Host "API is accessible. Aircraft count: $($response.aircraft.Count)"
}
catch {
    Write-Host "API error: $_"
}
```

### Check Configuration

```csharp
// Add diagnostic output to Program.cs
var pollerConfig = serviceProvider.GetRequiredService<IOptions<PollerConfiguration>>().Value;
Console.WriteLine($"Poller Enabled: {pollerConfig.Enabled}");
Console.WriteLine($"Interval: {pollerConfig.IntervalSeconds} seconds");

var dataPath = PlaneCrazyPaths.GetDocumentsPath();
Console.WriteLine($"Data Path: {dataPath}");
```

## Logging Best Practices

### Enable Detailed Logging

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "System": "Information"
    }
  }
}
```

### Redirect Logs to File

```powershell
# Redirect all output
.\PlaneCrazy.Console.exe > logs.txt 2>&1

# Or use PowerShell redirection
.\PlaneCrazy.Console.exe *> logs.txt
```

### Analyze Logs

```powershell
# Search for errors
Select-String -Path logs.txt -Pattern "ERROR|Exception" -Context 2, 5

# Count warnings
(Select-String -Path logs.txt -Pattern "WARN").Count

# View recent entries
Get-Content logs.txt -Tail 50
```

## When to Seek Help

Contact developers or file an issue if:

1. **Bug is reproducible** with clear steps
2. **Data corruption** that can't be fixed
3. **Critical security issue** discovered
4. **Performance degradation** that can't be resolved
5. **Feature request** for troubleshooting tools

### Information to Provide

When reporting issues, include:

1. **Environment**: OS, .NET version, deployment method
2. **Steps to Reproduce**: Exact sequence of actions
3. **Expected Behavior**: What should happen
4. **Actual Behavior**: What actually happens
5. **Logs**: Relevant log excerpts
6. **Data Statistics**: Event count, file sizes
7. **Configuration**: Non-sensitive config values

### Example Issue Report

```markdown
## Bug Report

**Environment**:
- OS: Windows 11
- .NET Version: 10.0.1
- Deployment: Windows Service (NSSM)

**Issue**: Application crashes when processing large number of aircraft

**Steps to Reproduce**:
1. Start application
2. Wait for 10 minutes during busy time (2000+ aircraft)
3. Application crashes with OutOfMemoryException

**Expected**: Application should handle any number of aircraft

**Actual**: Crashes after ~2000 aircraft

**Logs**:
```
[ERROR] OutOfMemoryException at AircraftStateProjection.Apply()
Stack trace: ...
```

**Data Statistics**:
- Event Count: 125,487
- Event Store Size: 450 MB
- Memory Usage Before Crash: 1.8 GB
```

## Recovery Procedures

### Complete Reset

**Warning**: This deletes all data

```powershell
# Stop application
Stop-Process -Name PlaneCrazy.Console -Force

# Delete data directory
Remove-Item -Path "$env:USERPROFILE\Documents\PlaneCrazy" -Recurse -Force

# Start application (will create fresh data directory)
Start-Process "$PWD\PlaneCrazy.Console.exe"
```

### Partial Reset

**Reset projections only** (keep events):
```powershell
# Stop application
Stop-Process -Name PlaneCrazy.Console -Force

# Delete projection files
Remove-Item "$env:USERPROFILE\Documents\PlaneCrazy\Projections\*.json" -Force

# Projections will rebuild from events on next start
```

**Reset repositories only** (keep events and projections):
```powershell
Remove-Item "$env:USERPROFILE\Documents\PlaneCrazy\Repositories\*.json" -Force
```

### Event Store Repair

```powershell
# Find and remove corrupted events
Get-ChildItem "$env:USERPROFILE\Documents\PlaneCrazy\Events\*.json" | ForEach-Object {
    try {
        Get-Content $_.FullName | ConvertFrom-Json | Out-Null
    }
    catch {
        Write-Host "Removing corrupted file: $($_.Name)"
        Move-Item $_.FullName "$env:USERPROFILE\Documents\PlaneCrazy\Corrupted_$($_.Name)"
    }
}
```

---

*Last Updated: January 24, 2026*
