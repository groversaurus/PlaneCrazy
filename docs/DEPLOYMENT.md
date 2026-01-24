# PlaneCrazy Deployment Guide

## Overview

This guide covers building, deploying, and operating the PlaneCrazy application across different environments.

## System Requirements

### Hardware

**Minimum**:
- CPU: 1 core
- RAM: 512 MB
- Disk: 1 GB free space
- Network: Internet connection

**Recommended**:
- CPU: 2+ cores
- RAM: 2 GB
- Disk: 10 GB free space (for event storage growth)
- Network: Stable broadband connection

### Software

**Required**:
- .NET 10.0 SDK (or runtime for deployment)
- Windows, Linux, or macOS
- Terminal/Command Line access

**Optional**:
- Git (for source control)
- Visual Studio Code or Visual Studio
- Docker (for containerized deployment)

## Building the Application

### Development Build

```powershell
# Clone repository
git clone <repository-url>
cd planecrazy

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/PlaneCrazy.Console
```

### Release Build

```powershell
# Build in Release configuration
dotnet build --configuration Release

# Or build specific project
dotnet build src/PlaneCrazy.Console/PlaneCrazy.Console.csproj --configuration Release
```

### Publish for Deployment

```powershell
# Self-contained deployment (includes .NET runtime)
dotnet publish src/PlaneCrazy.Console/PlaneCrazy.Console.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output ./publish/win-x64

# Framework-dependent deployment (requires .NET runtime on target)
dotnet publish src/PlaneCrazy.Console/PlaneCrazy.Console.csproj `
    --configuration Release `
    --output ./publish/portable
```

### Platform-Specific Builds

**Windows (x64)**:
```powershell
dotnet publish --runtime win-x64 --self-contained true --output ./publish/win-x64
```

**Linux (x64)**:
```powershell
dotnet publish --runtime linux-x64 --self-contained true --output ./publish/linux-x64
```

**macOS (x64)**:
```powershell
dotnet publish --runtime osx-x64 --self-contained true --output ./publish/osx-x64
```

**macOS (ARM64)**:
```powershell
dotnet publish --runtime osx-arm64 --self-contained true --output ./publish/osx-arm64
```

## Deployment Options

### Option 1: Direct Execution

**Best For**: Development, testing, personal use

```powershell
# Navigate to project directory
cd src/PlaneCrazy.Console

# Run directly
dotnet run
```

**Pros**:
- Simple and fast
- No build artifacts to manage
- Easy debugging

**Cons**:
- Requires .NET SDK
- Not suitable for production

---

### Option 2: Published Executable

**Best For**: Production, distribution

```powershell
# Publish application
dotnet publish --configuration Release --output ./publish

# Run published executable
cd publish
./PlaneCrazy.Console.exe  # Windows
./PlaneCrazy.Console       # Linux/macOS
```

**Pros**:
- Optimized for performance
- Can include runtime (self-contained)
- Production-ready

**Cons**:
- Larger file size (self-contained)
- Platform-specific builds

---

### Option 3: Windows Service

**Best For**: Always-on Windows servers

**Step 1**: Install as Windows Service
```powershell
# Using NSSM (Non-Sucking Service Manager)
nssm install PlaneCrazy "C:\path\to\PlaneCrazy.Console.exe"
nssm set PlaneCrazy AppDirectory "C:\path\to"
nssm set PlaneCrazy DisplayName "PlaneCrazy Aircraft Tracker"
nssm set PlaneCrazy Description "Tracks aircraft using ADS-B data"
nssm set PlaneCrazy Start SERVICE_AUTO_START
nssm start PlaneCrazy
```

**Step 2**: Manage Service
```powershell
# Start service
nssm start PlaneCrazy

# Stop service
nssm stop PlaneCrazy

# Restart service
nssm restart PlaneCrazy

# Remove service
nssm remove PlaneCrazy confirm
```

**Pros**:
- Runs on system startup
- Automatic restart on failure
- Runs in background

**Cons**:
- Windows-only
- Requires admin privileges

---

### Option 4: Linux Systemd Service

**Best For**: Linux servers

**Step 1**: Create service file `/etc/systemd/system/planecrazy.service`
```ini
[Unit]
Description=PlaneCrazy Aircraft Tracker
After=network.target

[Service]
Type=notify
User=planecrazy
WorkingDirectory=/opt/planecrazy
ExecStart=/opt/planecrazy/PlaneCrazy.Console
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=planecrazy
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

**Step 2**: Enable and start service
```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (start on boot)
sudo systemctl enable planecrazy

# Start service
sudo systemctl start planecrazy

# Check status
sudo systemctl status planecrazy

# View logs
sudo journalctl -u planecrazy -f
```

**Pros**:
- Native Linux integration
- Automatic restart
- Log integration

**Cons**:
- Linux-only
- Requires root access

---

### Option 5: Docker Container

**Best For**: Cloud deployments, consistent environments

**Step 1**: Create `Dockerfile`
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY src/PlaneCrazy.Console/PlaneCrazy.Console.csproj src/PlaneCrazy.Console/
COPY src/PlaneCrazy.Domain/PlaneCrazy.Domain.csproj src/PlaneCrazy.Domain/
COPY src/PlaneCrazy.Infrastructure/PlaneCrazy.Infrastructure.csproj src/PlaneCrazy.Infrastructure/
RUN dotnet restore src/PlaneCrazy.Console/PlaneCrazy.Console.csproj

# Copy source code
COPY src/ src/
RUN dotnet publish src/PlaneCrazy.Console/PlaneCrazy.Console.csproj \
    -c Release \
    -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Create volume for data persistence
VOLUME /data
ENV PLANECRAZY_DATA_PATH=/data

ENTRYPOINT ["dotnet", "PlaneCrazy.Console.dll"]
```

**Step 2**: Build and run container
```bash
# Build image
docker build -t planecrazy:latest .

# Run container
docker run -d \
    --name planecrazy \
    --restart unless-stopped \
    -v planecrazy-data:/data \
    planecrazy:latest

# View logs
docker logs -f planecrazy

# Stop container
docker stop planecrazy

# Remove container
docker rm planecrazy
```

**Step 3**: Docker Compose (optional)
```yaml
# docker-compose.yml
version: '3.8'

services:
  planecrazy:
    build: .
    container_name: planecrazy
    restart: unless-stopped
    volumes:
      - planecrazy-data:/data
    environment:
      - DOTNET_ENVIRONMENT=Production
      - PLANECRAZY_DATA_PATH=/data

volumes:
  planecrazy-data:
```

```bash
# Start with Docker Compose
docker-compose up -d

# Stop
docker-compose down
```

**Pros**:
- Consistent environment
- Easy updates
- Portable across platforms

**Cons**:
- Requires Docker knowledge
- Additional overhead

---

## Configuration

### Environment Variables

```powershell
# Set polling interval (seconds)
$env:PLANECRAZY_POLLING_INTERVAL = "30"

# Set data directory
$env:PLANECRAZY_DATA_PATH = "C:\PlaneCrazy\Data"

# Set log level
$env:PLANECRAZY_LOG_LEVEL = "Information"

# Disable background poller
$env:PLANECRAZY_POLLER_ENABLED = "false"
```

### Configuration File

Create `appsettings.Production.json`:
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
    "IntervalSeconds": 30,
    "BoundingBox": {
      "MinLatitude": 35.0,
      "MinLongitude": -10.0,
      "MaxLatitude": 70.0,
      "MaxLongitude": 40.0
    }
  },
  "DataPath": "/opt/planecrazy/data"
}
```

## Data Management

### Data Directory Structure

```
Documents/PlaneCrazy/
├── Events/               # Event store (JSON files)
├── Repositories/         # Entity repositories (JSON files)
└── Projections/         # Projection state (JSON files)
```

### Backup Strategy

**Manual Backup**:
```powershell
# Create backup
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$source = "$env:USERPROFILE\Documents\PlaneCrazy"
$dest = "C:\Backups\PlaneCrazy_$timestamp"
Copy-Item -Path $source -Destination $dest -Recurse

# Compress backup
Compress-Archive -Path $dest -DestinationPath "$dest.zip"
```

**Automated Backup** (Windows Task Scheduler):
```powershell
# Create backup script
@"
`$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
`$source = "`$env:USERPROFILE\Documents\PlaneCrazy"
`$dest = "C:\Backups\PlaneCrazy_`$timestamp"
Copy-Item -Path `$source -Destination `$dest -Recurse
Compress-Archive -Path `$dest -DestinationPath "`$dest.zip"
Remove-Item -Path `$dest -Recurse -Force

# Keep only last 7 backups
Get-ChildItem "C:\Backups\PlaneCrazy_*.zip" | 
    Sort-Object CreationTime -Descending | 
    Select-Object -Skip 7 | 
    Remove-Item -Force
"@ | Out-File -FilePath "C:\Scripts\BackupPlaneCrazy.ps1"

# Schedule task
schtasks /create /tn "PlaneCrazy Backup" /tr "powershell -File C:\Scripts\BackupPlaneCrazy.ps1" /sc daily /st 03:00
```

### Restore from Backup

```powershell
# Stop application
# (Windows Service)
nssm stop PlaneCrazy

# Extract backup
Expand-Archive -Path "C:\Backups\PlaneCrazy_20260124.zip" -DestinationPath "C:\Temp\Restore"

# Replace data directory
Remove-Item -Path "$env:USERPROFILE\Documents\PlaneCrazy" -Recurse -Force
Copy-Item -Path "C:\Temp\Restore\PlaneCrazy" -Destination "$env:USERPROFILE\Documents" -Recurse

# Start application
nssm start PlaneCrazy
```

## Monitoring

### Log Files

**Default Location**: Console output

**Redirect to File**:
```powershell
# Windows
PlaneCrazy.Console.exe > logs.txt 2>&1

# Linux
./PlaneCrazy.Console > logs.txt 2>&1
```

**Systemd Logs** (Linux):
```bash
# View all logs
sudo journalctl -u planecrazy

# Follow logs (live)
sudo journalctl -u planecrazy -f

# Logs from last hour
sudo journalctl -u planecrazy --since "1 hour ago"

# Logs with errors only
sudo journalctl -u planecrazy -p err
```

### Health Checks

**Manual Check**:
```powershell
# Check if process is running
Get-Process PlaneCrazy.Console -ErrorAction SilentlyContinue

# Check recent log entries
Get-Content logs.txt -Tail 50
```

**Automated Monitoring Script**:
```powershell
# monitor.ps1
while ($true) {
    $process = Get-Process PlaneCrazy.Console -ErrorAction SilentlyContinue
    
    if (-not $process) {
        Write-Host "PlaneCrazy is not running! Attempting restart..."
        Start-Process "C:\Path\To\PlaneCrazy.Console.exe"
    }
    else {
        Write-Host "PlaneCrazy is running (PID: $($process.Id))"
    }
    
    Start-Sleep -Seconds 60
}
```

### Performance Metrics

Monitor these key metrics:

1. **CPU Usage**: Should be low (< 5%) between polling cycles
2. **Memory Usage**: Should be stable (< 200 MB typically)
3. **Disk I/O**: Spikes during event storage and projection updates
4. **Network Traffic**: Periodic API calls every polling interval
5. **Event Count**: Growing steadily over time
6. **Projection Update Time**: Should complete within seconds

## Updates

### Update Process

```powershell
# 1. Stop application
nssm stop PlaneCrazy  # Windows Service
# or
docker stop planecrazy  # Docker

# 2. Backup data (optional but recommended)
Copy-Item -Path "$env:USERPROFILE\Documents\PlaneCrazy" -Destination "C:\Backups\PlaneCrazy_BeforeUpdate" -Recurse

# 3. Pull latest code
git pull origin main

# 4. Build new version
dotnet build --configuration Release

# 5. Publish
dotnet publish src/PlaneCrazy.Console/PlaneCrazy.Console.csproj --configuration Release --output ./publish

# 6. Replace binaries
Copy-Item -Path "./publish/*" -Destination "C:\PlaneCrazy" -Recurse -Force

# 7. Start application
nssm start PlaneCrazy
```

### Rolling Back

```powershell
# 1. Stop current version
nssm stop PlaneCrazy

# 2. Restore previous binaries
Copy-Item -Path "C:\Backups\PlaneCrazy_v1.0" -Destination "C:\PlaneCrazy" -Recurse -Force

# 3. Restore data (if needed)
Copy-Item -Path "C:\Backups\PlaneCrazy_Data_BeforeUpdate" -Destination "$env:USERPROFILE\Documents\PlaneCrazy" -Recurse -Force

# 4. Start previous version
nssm start PlaneCrazy
```

## Troubleshooting Deployment

### Application Won't Start

**Check**:
1. .NET runtime installed: `dotnet --version`
2. Executable permissions (Linux): `chmod +x PlaneCrazy.Console`
3. Required dependencies present
4. Configuration files valid JSON

### Data Directory Errors

**Error**: "Access denied to data directory"

**Solution**:
```powershell
# Grant permissions (Windows)
icacls "C:\PlaneCrazy\Data" /grant "Users:(OI)(CI)F"

# Grant permissions (Linux)
sudo chown -R planecrazy:planecrazy /opt/planecrazy/data
sudo chmod -R 755 /opt/planecrazy/data
```

### Network Connectivity Issues

**Test API Connection**:
```powershell
# Test adsb.fi API
Invoke-RestMethod -Uri "https://api.adsb.fi/v2/lat/35.0/lon/-10.0/lat/70.0/lon/40.0"
```

**Firewall Rules**:
```powershell
# Allow outbound HTTPS (Windows Firewall)
New-NetFirewallRule -DisplayName "PlaneCrazy HTTPS" -Direction Outbound -Protocol TCP -RemotePort 443 -Action Allow
```

### Service Won't Auto-Start

**Windows Service**:
```powershell
# Check service status
nssm status PlaneCrazy

# Set to auto-start
nssm set PlaneCrazy Start SERVICE_AUTO_START
```

**Linux Systemd**:
```bash
# Enable service
sudo systemctl enable planecrazy

# Check if enabled
sudo systemctl is-enabled planecrazy
```

## Security Considerations

### File Permissions

**Windows**:
```powershell
# Restrict data directory to current user
icacls "C:\PlaneCrazy\Data" /inheritance:r
icacls "C:\PlaneCrazy\Data" /grant:r "$env:USERNAME:(OI)(CI)F"
```

**Linux**:
```bash
# Restrict to service user
sudo chown -R planecrazy:planecrazy /opt/planecrazy
sudo chmod 700 /opt/planecrazy/data
```

### Network Security

- Application makes **outbound HTTPS only**
- No incoming network connections required
- No authentication credentials stored
- API keys not required (public API)

### Data Privacy

- No personally identifiable information (PII) collected
- Aircraft tracking data is public domain
- Comments and favourites stored locally only

## Production Checklist

- [ ] .NET runtime/SDK installed
- [ ] Application published in Release configuration
- [ ] Data directory configured with proper permissions
- [ ] Service/daemon configured for auto-start
- [ ] Backup strategy implemented and tested
- [ ] Monitoring and logging configured
- [ ] Firewall rules configured (if necessary)
- [ ] Update procedure documented
- [ ] Rollback plan tested
- [ ] Health checks automated
- [ ] Documentation accessible to operators

## Cloud Deployment

### Azure App Service

```bash
# Login to Azure
az login

# Create resource group
az group create --name PlaneCrazyRG --location eastus

# Create App Service plan
az appservice plan create --name PlaneCrazyPlan --resource-group PlaneCrazyRG --sku B1 --is-linux

# Create web app
az webapp create --name planecrazy --resource-group PlaneCrazyRG --plan PlaneCrazyPlan --runtime "DOTNETCORE:10.0"

# Deploy
az webapp deploy --name planecrazy --resource-group PlaneCrazyRG --src-path ./publish.zip
```

### AWS EC2

```bash
# Launch EC2 instance
aws ec2 run-instances --image-id ami-xxxxxx --instance-type t2.micro --key-name MyKeyPair

# SSH to instance
ssh -i MyKeyPair.pem ec2-user@<instance-ip>

# Install .NET
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Upload and run application
scp -i MyKeyPair.pem publish.zip ec2-user@<instance-ip>:~/
ssh -i MyKeyPair.pem ec2-user@<instance-ip>
unzip publish.zip
./PlaneCrazy.Console
```

### Google Cloud Run

```bash
# Build container
docker build -t gcr.io/my-project/planecrazy .

# Push to registry
docker push gcr.io/my-project/planecrazy

# Deploy
gcloud run deploy planecrazy --image gcr.io/my-project/planecrazy --platform managed --region us-central1
```

---

*Last Updated: January 24, 2026*
