# PlaneCrazy Models

This library provides data models for handling ADS-B (Automatic Dependent Surveillance-Broadcast) aircraft tracking data based on the adsb.fi API JSON structure.

## Models

### Position
Represents geographic position data:
- `Latitude` - Decimal degrees (-90 to 90)
- `Longitude` - Decimal degrees (-180 to 180)
- `Altitude` - Altitude in feet (nullable)
- `GroundAltitude` - Ground level altitude in feet (nullable)

### Aircraft
Represents aircraft identification and flight information:
- `Hex` - ICAO 24-bit address (unique aircraft identifier)
- `Flight` - Flight callsign or registration
- `Registration` - Aircraft registration (tail number)
- `Type` - ICAO aircraft type designator (e.g., "B738" for Boeing 737-800)
- `Description` - Aircraft manufacturer and model description
- `Category` - EmitterCategory enum value
- `GroundSpeed` - Speed in knots
- `Track` - True track/heading in degrees (0-359)
- `VerticalRate` - Climb/descent rate in feet per minute
- `Squawk` - Transponder code
- `OnGround` - Whether aircraft is on the ground
- `Emergency` - Emergency status indicator
- `Spi` - Special Position Identification pulse indicator

### Snapshot
Represents a timestamped snapshot of aircraft state:
- `Timestamp` - UTC timestamp
- `Aircraft` - Aircraft data
- `Position` - Position data
- `SeenPos` - Unix timestamp of last position update
- `Seen` - Unix timestamp of last message
- `Messages` - Number of messages received
- `Rssi` - Received Signal Strength Indicator (dBFS)

### EmitterCategory (Enum)
Aircraft/vehicle type classifications:
- None, Light, Small, Large, Heavy
- HighVortexLarge, HighPerformance
- Rotorcraft, Glider, LighterThanAir
- Parachutist, Ultralight, UAV
- Space, Surface vehicles, Obstacles
- Reserved

## Usage

```csharp
using PlaneCrazy.Models;

var snapshot = new Snapshot
{
    Timestamp = DateTime.UtcNow,
    Aircraft = new Aircraft
    {
        Hex = "a1b2c3",
        Flight = "UAL123",
        Registration = "N12345",
        Type = "B738",
        Category = EmitterCategory.Large,
        GroundSpeed = 450.5,
        Track = 270.0
    },
    Position = new Position
    {
        Latitude = 40.7128,
        Longitude = -74.0060,
        Altitude = 35000
    }
};
```

## Testing

Run tests with:
```bash
dotnet test
```

All models include comprehensive unit tests validating functionality.
