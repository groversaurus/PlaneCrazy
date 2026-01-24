# PlaneCrazy Security Guide

## Overview

This document covers security considerations, best practices, and potential vulnerabilities in the PlaneCrazy application.

## Security Model

### Current Architecture

**Application Type**: Desktop console application  
**Network Exposure**: Outbound HTTPS only (no inbound connections)  
**Data Storage**: Local file system (JSON files)  
**User Authentication**: None (single-user application)  
**External Dependencies**: adsb.fi API (public, no authentication)

### Threat Surface

**Low Risk**:
- No web interface
- No network listeners
- No user authentication needed
- No sensitive credentials
- Public data only

**Moderate Risk**:
- Local file system access
- External API dependency
- Data injection via API responses

---

## Data Security

### Data Classification

**Public Data**:
- ✅ Aircraft positions (publicly broadcast via ADS-B)
- ✅ Aircraft identifiers (ICAO24, registration, type)
- ✅ Airport codes and names

**User Data**:
- ⚠️ Favourites (personal preferences)
- ⚠️ Comments (user-generated content)

**No Sensitive Data**:
- ❌ No passwords
- ❌ No API keys
- ❌ No personal information (PII)
- ❌ No financial data

### Data Storage Security

#### File System Permissions

**Windows**:
```powershell
# Restrict data directory to current user only
$dataPath = "$env:USERPROFILE\Documents\PlaneCrazy"
icacls $dataPath /inheritance:r
icacls $dataPath /grant:r "$env:USERNAME:(OI)(CI)F"
```

**Linux/macOS**:
```bash
# Restrict to user only
chmod 700 ~/Documents/PlaneCrazy
```

**Why**: Prevent other users on the same system from accessing favourites/comments

#### Encryption at Rest

**Current**: No encryption (data is not sensitive)

**Future Enhancement**:
```csharp
public class EncryptedFileStorage
{
    private readonly byte[] _key;
    
    public async Task WriteEncryptedAsync(string path, string data)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        using var fs = File.Create(path);
        
        // Write IV first
        await fs.WriteAsync(aes.IV);
        
        // Write encrypted data
        using var cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);
        await writer.WriteAsync(data);
    }
}
```

---

## Network Security

### API Communication

**Protocol**: HTTPS only  
**Authentication**: None required (public API)  
**Encryption**: TLS 1.2+ (enforced by .NET)

**Best Practices**:

**1. Validate SSL Certificates** (default in .NET):
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // ❌ Never do this!
};

// ✅ Use default (validates certificates)
var client = new HttpClient(); // Validates by default
```

**2. Timeout Configuration**:
```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30) // Prevent indefinite hangs
};
```

**3. Deny List for Malicious URLs**:
```csharp
public class SafeApiClient : IApiClient
{
    private readonly HashSet<string> _blockedDomains = new()
    {
        "malicious-site.com",
        "known-bad-actor.net"
    };
    
    public async Task<T?> GetAsync<T>(string url)
    {
        var uri = new Uri(url);
        if (_blockedDomains.Contains(uri.Host))
        {
            throw new SecurityException($"Blocked domain: {uri.Host}");
        }
        
        return await _httpClient.GetFromJsonAsync<T>(url);
    }
}
```

### Firewall Configuration

**Outbound Rules**:
- Allow: HTTPS (port 443) to `api.adsb.fi`
- Block: All inbound connections (none needed)

**Windows Firewall**:
```powershell
# Allow outbound HTTPS
New-NetFirewallRule `
    -DisplayName "PlaneCrazy HTTPS" `
    -Direction Outbound `
    -Protocol TCP `
    -RemotePort 443 `
    -Action Allow `
    -Program "C:\Path\To\PlaneCrazy.Console.exe"
```

---

## Input Validation

### Validating User Input

**Current Validation**: Using `FluentValidation` library

**Command Validation**:
```csharp
public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .Must(BeValidEntityType)
            .WithMessage("Entity type must be Aircraft, Type, or Airport");
        
        RuleFor(x => x.Text)
            .NotEmpty()
            .Length(1, 5000)
            .WithMessage("Comment must be 1-5000 characters");
        
        // Prevent injection attacks
        RuleFor(x => x.Text)
            .Must(NotContainDangerousContent)
            .WithMessage("Comment contains prohibited content");
    }
    
    private bool NotContainDangerousContent(string text)
    {
        // Check for common injection patterns
        var dangerousPatterns = new[]
        {
            "<script", "javascript:", "onerror=", 
            "onclick=", "onload=", "../", "..\\"
        };
        
        return !dangerousPatterns.Any(p => 
            text.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
```

### Validating API Responses

**Example**: Validate aircraft data from API

```csharp
public class AircraftValidator
{
    public bool IsValid(Aircraft aircraft)
    {
        // Validate ICAO24 format
        if (!IsValidIcao24(aircraft.Icao24))
            return false;
        
        // Validate coordinate ranges
        if (aircraft.Latitude < -90 || aircraft.Latitude > 90)
            return false;
        
        if (aircraft.Longitude < -180 || aircraft.Longitude > 180)
            return false;
        
        // Validate altitude range
        if (aircraft.Altitude < -1000 || aircraft.Altitude > 60000)
            return false;
        
        // Validate speed range
        if (aircraft.Velocity < 0 || aircraft.Velocity > 1000)
            return false;
        
        return true;
    }
    
    private bool IsValidIcao24(string icao24)
    {
        return !string.IsNullOrEmpty(icao24) &&
               icao24.Length == 6 &&
               icao24.All(c => char.IsDigit(c) || 
                              (c >= 'A' && c <= 'F') || 
                              (c >= 'a' && c <= 'f'));
    }
}
```

### Sanitizing Data

**Example**: Sanitize comment text

```csharp
public class CommentSanitizer
{
    public string Sanitize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        
        // Remove control characters
        text = RemoveControlCharacters(text);
        
        // Normalize whitespace
        text = NormalizeWhitespace(text);
        
        // Limit length
        if (text.Length > 5000)
            text = text.Substring(0, 5000);
        
        return text;
    }
    
    private string RemoveControlCharacters(string text)
    {
        return new string(text.Where(c => 
            !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
            .ToArray());
    }
    
    private string NormalizeWhitespace(string text)
    {
        // Replace multiple spaces with single space
        return Regex.Replace(text.Trim(), @"\s+", " ");
    }
}
```

---

## Dependency Security

### NuGet Package Security

**Best Practices**:

1. **Use Package Signatures**:
```xml
<PropertyGroup>
  <NuGetPackageSignatureVerification>true</NuGetPackageSignatureVerification>
</PropertyGroup>
```

2. **Keep Dependencies Updated**:
```powershell
# Check for outdated packages
dotnet list package --outdated

# Update packages
dotnet add package <PackageName> --version <LatestVersion>
```

3. **Scan for Vulnerabilities**:
```powershell
# Using dotnet CLI
dotnet list package --vulnerable

# Or use third-party tools
# npm audit (for JavaScript dependencies)
# OWASP Dependency-Check
```

### Current Dependencies

**Core Dependencies** (as of project creation):
- `Microsoft.Extensions.Hosting` - Microsoft (trusted)
- `Microsoft.Extensions.Http` - Microsoft (trusted)
- `FluentValidation` - Community (well-maintained)
- `System.Text.Json` - Microsoft (built-in)

**Security Assessment**: ✅ All dependencies from trusted sources

---

## Code Security

### Avoiding Common Vulnerabilities

#### 1. SQL Injection

**Not Applicable**: No SQL database in use

#### 2. Path Traversal

**Risk**: File operations with user-controlled paths

**Mitigation**:
```csharp
public class SafeFileAccess
{
    private readonly string _baseDirectory;
    
    public SafeFileAccess()
    {
        _baseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PlaneCrazy");
    }
    
    public string GetSafePath(string filename)
    {
        // Remove path traversal attempts
        filename = Path.GetFileName(filename);
        
        // Construct full path
        var fullPath = Path.Combine(_baseDirectory, filename);
        
        // Verify path is within base directory
        if (!fullPath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException("Path traversal attempt detected");
        }
        
        return fullPath;
    }
}
```

#### 3. Deserialization Vulnerabilities

**Risk**: Malicious JSON payloads

**Mitigation**:
```csharp
public class SecureJsonDeserializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        MaxDepth = 64, // Prevent stack overflow
        PropertyNameCaseInsensitive = true
    };
    
    public T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON detected");
            return default;
        }
    }
}
```

#### 4. Command Injection

**Not Applicable**: No shell command execution in current implementation

**If Added**: Use parameterized commands
```csharp
// ❌ Never do this:
Process.Start("cmd.exe", $"/c {userInput}");

// ✅ Do this instead:
var startInfo = new ProcessStartInfo
{
    FileName = "safe-executable.exe",
    Arguments = escapedUserInput,
    UseShellExecute = false
};
Process.Start(startInfo);
```

#### 5. XML External Entity (XXE) Injection

**Not Applicable**: No XML parsing in current implementation

---

## Logging and Auditing

### Secure Logging

**Best Practices**:

1. **Don't Log Sensitive Data**:
```csharp
// ❌ Bad: Logs sensitive info
_logger.LogInformation("User password: {Password}", password);

// ✅ Good: Logs only non-sensitive info
_logger.LogInformation("User login attempt: {Username}", username);
```

2. **Sanitize Log Output**:
```csharp
public class SecureLogger
{
    private readonly ILogger _logger;
    
    public void LogUserInput(string input)
    {
        // Remove control characters that could break log format
        var sanitized = Regex.Replace(input, @"[\r\n\t]", " ");
        
        // Limit length to prevent log injection
        if (sanitized.Length > 200)
            sanitized = sanitized.Substring(0, 200) + "...";
        
        _logger.LogInformation("User input: {Input}", sanitized);
    }
}
```

3. **Log Security Events**:
```csharp
public class SecurityEventLogger
{
    public void LogValidationFailure(string entityType, string reason)
    {
        _logger.LogWarning(
            "Validation failure for {EntityType}: {Reason}",
            entityType, reason);
    }
    
    public void LogSuspiciousActivity(string activity)
    {
        _logger.LogError(
            "Suspicious activity detected: {Activity}",
            activity);
    }
}
```

### Audit Trail

Event sourcing provides built-in audit trail:

```csharp
// All events are immutable and timestamped
public abstract class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

// Query audit history
public async Task<IEnumerable<DomainEvent>> GetAuditTrailAsync(
    string entityType, 
    string entityId)
{
    var events = await _eventStore.GetAllAsync();
    return events.Where(e => 
        e is CommentAdded ca && 
        ca.EntityType == entityType && 
        ca.EntityId == entityId)
        .OrderBy(e => e.OccurredAt);
}
```

---

## Error Handling

### Secure Error Messages

**Don't Expose Internal Details**:

```csharp
// ❌ Bad: Exposes internal paths
catch (FileNotFoundException ex)
{
    throw new Exception($"File not found: {ex.FileName}");
}

// ✅ Good: Generic message for user
catch (FileNotFoundException ex)
{
    _logger.LogError(ex, "File not found: {FileName}", ex.FileName);
    throw new Exception("Unable to load data. Please contact support.");
}
```

### Fail Securely

```csharp
public async Task<Aircraft?> GetAircraftAsync(string icao24)
{
    try
    {
        return await _repository.GetByIdAsync(icao24);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading aircraft {Icao24}", icao24);
        
        // Fail securely: return null instead of exposing error
        return null;
    }
}
```

---

## Secure Development Practices

### Code Review Checklist

- [ ] All user input is validated
- [ ] No sensitive data in logs
- [ ] File paths are sanitized
- [ ] API responses are validated
- [ ] Errors handled securely
- [ ] No hardcoded secrets
- [ ] Dependencies are up to date
- [ ] No SQL/command injection vectors
- [ ] Data sanitized before storage
- [ ] TLS/HTTPS used for external calls

### Security Testing

```csharp
[Test]
public void CommentValidator_RejectsScriptInjection()
{
    // Arrange
    var validator = new AddCommentCommandValidator();
    var command = new AddCommentCommand
    {
        EntityType = "Aircraft",
        EntityId = "ABC123",
        Text = "<script>alert('xss')</script>"
    };
    
    // Act
    var result = validator.Validate(command);
    
    // Assert
    Assert.That(result.IsValid, Is.False);
}

[Test]
public void FileAccess_RejectsPathTraversal()
{
    // Arrange
    var fileAccess = new SafeFileAccess();
    
    // Act & Assert
    Assert.Throws<SecurityException>(() => 
        fileAccess.GetSafePath("../../etc/passwd"));
}
```

---

## Compliance and Privacy

### GDPR Compliance

**Personal Data Collected**: None (or minimal)

**User Rights**:
- ✅ **Right to Access**: Users can view their data (favourites, comments)
- ✅ **Right to Erasure**: Users can delete data directory
- ✅ **Right to Portability**: Data stored in JSON (portable format)
- ✅ **Right to Rectification**: Users can edit comments

**Implementation**:
```csharp
public class UserDataExporter
{
    public async Task ExportUserDataAsync(string exportPath)
    {
        var userData = new
        {
            Favourites = await _favouriteRepo.GetAllAsync(),
            Comments = await _commentRepo.GetAllAsync(),
            ExportedAt = DateTime.UtcNow
        };
        
        var json = JsonSerializer.Serialize(userData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        await File.WriteAllTextAsync(exportPath, json);
    }
}
```

### Data Retention

**Current**: Indefinite retention

**Recommended**:
```csharp
public class DataRetentionPolicy
{
    public async Task EnforceRetentionPolicyAsync()
    {
        // Delete events older than 1 year
        var cutoffDate = DateTime.UtcNow.AddYears(-1);
        var oldEvents = await _eventStore.GetEventsBeforeAsync(cutoffDate);
        
        // Archive before deletion
        await ArchiveEventsAsync(oldEvents);
        
        // Delete old events
        await _eventStore.DeleteAsync(oldEvents);
    }
}
```

---

## Incident Response

### Security Incident Plan

**1. Detection**:
- Monitor logs for suspicious activity
- Track validation failures
- Watch for unusual API responses

**2. Response**:
- Stop application if compromise suspected
- Backup current state
- Review logs for scope of incident

**3. Recovery**:
- Restore from backup if needed
- Update application if vulnerability found
- Notify users if data breach occurred

**4. Post-Incident**:
- Document incident
- Update security measures
- Conduct security review

---

## Future Security Enhancements

1. **Multi-User Support**: Add authentication and authorization
2. **Encryption**: Encrypt sensitive data at rest
3. **API Key Management**: Secure storage of API keys
4. **Rate Limiting**: Prevent API abuse
5. **Content Security Policy**: If web interface added
6. **Security Scanning**: Automated vulnerability scanning
7. **Penetration Testing**: Professional security assessment
8. **Security Audit**: Third-party security audit
9. **Bug Bounty Program**: Incentivize security research

---

## Security Contacts

**Reporting Security Issues**:
- Email: [security@planecrazy.example]
- Encrypted: [PGP key fingerprint]
- Response Time: Within 48 hours

**Disclosure Policy**:
- Report privately first
- Wait for fix before public disclosure
- Coordinated disclosure timeline

---

*Last Updated: January 24, 2026*
