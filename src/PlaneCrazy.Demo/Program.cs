using PlaneCrazy.EventStore;
using PlaneCrazy.EventStore.Events;

Console.WriteLine("PlaneCrazy Event Store Demo");
Console.WriteLine("============================\n");

// Create an instance of the event store
var eventStore = new JsonFileEventStore("./DemoEvents");

Console.WriteLine("Simulating aircraft tracking events...\n");

// Simulate detecting a new aircraft
var aircraftDetected = new AircraftDetectedEvent
{
    EntityType = "Aircraft",
    EntityId = "ABC123",
    IcaoAddress = "ABC123",
    Callsign = "UAL123",
    Latitude = 40.7128,
    Longitude = -74.0060,
    Altitude = 35000
};

await eventStore.SaveEventAsync(aircraftDetected);
Console.WriteLine($"✓ Aircraft detected: {aircraftDetected.Callsign} at {aircraftDetected.Latitude}, {aircraftDetected.Longitude}");

// Simulate position updates
await Task.Delay(100); // Small delay to ensure different timestamps

var positionUpdate1 = new PositionUpdatedEvent
{
    EntityType = "Aircraft",
    EntityId = "ABC123",
    Latitude = 40.8128,
    Longitude = -74.1060,
    Altitude = 36000,
    GroundSpeed = 450.5,
    Track = 90.0,
    VerticalRate = 1000
};

await eventStore.SaveEventAsync(positionUpdate1);
Console.WriteLine($"✓ Position updated: {positionUpdate1.Latitude}, {positionUpdate1.Longitude}, {positionUpdate1.Altitude}ft");

await Task.Delay(100);

var positionUpdate2 = new PositionUpdatedEvent
{
    EntityType = "Aircraft",
    EntityId = "ABC123",
    Latitude = 40.9128,
    Longitude = -74.2060,
    Altitude = 37000,
    GroundSpeed = 455.0,
    Track = 92.0,
    VerticalRate = 500
};

await eventStore.SaveEventAsync(positionUpdate2);
Console.WriteLine($"✓ Position updated: {positionUpdate2.Latitude}, {positionUpdate2.Longitude}, {positionUpdate2.Altitude}ft");

// Simulate another aircraft
await Task.Delay(100);

var aircraftDetected2 = new AircraftDetectedEvent
{
    EntityType = "Aircraft",
    EntityId = "XYZ789",
    IcaoAddress = "XYZ789",
    Callsign = "DAL456",
    Latitude = 41.8781,
    Longitude = -87.6298,
    Altitude = 30000
};

await eventStore.SaveEventAsync(aircraftDetected2);
Console.WriteLine($"✓ Aircraft detected: {aircraftDetected2.Callsign} at {aircraftDetected2.Latitude}, {aircraftDetected2.Longitude}");

// Simulate squawk code change (emergency)
await Task.Delay(100);

var squawkChanged = new SquawkChangedEvent
{
    EntityType = "Aircraft",
    EntityId = "ABC123",
    PreviousSquawk = "1200",
    NewSquawk = "7700" // Emergency squawk code
};

await eventStore.SaveEventAsync(squawkChanged);
Console.WriteLine($"✓ Squawk changed: {squawkChanged.PreviousSquawk} → {squawkChanged.NewSquawk} (EMERGENCY!)");

Console.WriteLine("\n--- Retrieving Events ---\n");

// Get all events for ABC123
var abc123Events = await eventStore.GetEventsAsync("Aircraft", "ABC123");
Console.WriteLine($"Events for aircraft ABC123: {abc123Events.Count}");
foreach (var evt in abc123Events)
{
    Console.WriteLine($"  - {evt.EventType} at {evt.Timestamp:HH:mm:ss.fff}");
}

Console.WriteLine();

// Get all events for XYZ789
var xyz789Events = await eventStore.GetEventsAsync("Aircraft", "XYZ789");
Console.WriteLine($"Events for aircraft XYZ789: {xyz789Events.Count}");
foreach (var evt in xyz789Events)
{
    Console.WriteLine($"  - {evt.EventType} at {evt.Timestamp:HH:mm:ss.fff}");
}

Console.WriteLine();

// Get all aircraft events
var allAircraftEvents = await eventStore.GetEventsByEntityTypeAsync("Aircraft");
Console.WriteLine($"Total events for all aircraft: {allAircraftEvents.Count}");

Console.WriteLine("\n--- File Structure ---\n");
Console.WriteLine("Events are stored in the following structure:");
Console.WriteLine("DemoEvents/");
Console.WriteLine("  └── Aircraft/");
Console.WriteLine("      ├── ABC123/");
Console.WriteLine("      │   ├── 001.json");
Console.WriteLine("      │   ├── 002.json");
Console.WriteLine("      │   ├── 003.json");
Console.WriteLine("      │   └── 004.json");
Console.WriteLine("      └── XYZ789/");
Console.WriteLine("          └── 001.json");

Console.WriteLine("\nDemo completed! Check the DemoEvents directory to see the JSON files.");
