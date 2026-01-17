# PlaneCrazy

PlaneCrazy is a file‑based, event‑sourced aircraft tracking application that consumes ADS‑B data from the adsb.fi open API. The goal is to build a transparent, inspectable, extensible system using C#, JSON storage, and modern architectural patterns such as Event Sourcing and Event Modeling.

## Core Concepts

• Event‑sourced architecture: All domain changes are captured as append‑only events stored as JSON files.
• Event Modeling: User workflows, commands, events, and projections are designed explicitly.
• File‑based storage: No SQL Server. All data is stored under the user's Documents folder:
  • Documents/PlaneCrazy/Data
  • Documents/PlaneCrazy/Config
  • Documents/PlaneCrazy/Events

## ADS‑B Integration

• Uses adsb.fi's open REST API to fetch aircraft data.
• A C# repository pattern abstracts API access.
• Strongly typed models for aircraft, positions, snapshots, etc.

## Event Store

• JSON‑based, append‑only event store.
• Folder structure per entity type (Aircraft, Airport, etc.).
• Each event is a standalone JSON file with timestamp, type, and payload.
• Supports replay to rebuild projections.

## Projections (Read Models)

• Aircraft state projection
• Favourite aircraft/types/airports projection
• Comments projection
• Snapshot/history projection

Projections are rebuilt by replaying events and stored as JSON for fast UI access.

## Favourites System

Users can favourite:

• Individual aircraft (by hex or registration)
• Aircraft types (ICAO type codes, cargo/passenger filters)
• Airports (ICAO/IATA, name, lat/lon)

Favourites are stored in Config/favourites.json and updated via events:

• AircraftFavourited
• AircraftUnfavourited
• TypeFavourited
• AirportFavourited
…etc.

## Comments System

Users can add comments to any entity (aircraft, airports, etc.).
Events include:

• CommentAdded
• CommentEdited
• CommentDeleted

Comments are projected into per‑entity JSON files for easy display.

## Local Data Storage

• Aircraft data cached as JSON under Data/aircraft/{hex}.json
• Snapshots stored under Data/snapshots/
• Config stored under Config/
• Events stored under Events/{EntityType}/{EntityId}/

## Architecture

• C# repository pattern for API and local storage
• Event Store + Command Handlers + Projections
• Dependency Injection for clean composition
• Transparent, inspectable JSON files for all data
• Designed for extensibility (plugins, notifications, dashboards)

## Future Directions

• Map UI using Leaflet, Cesium, or MapLibre
• Background fetcher for periodic ADS‑B updates
• Notifications for favourite aircraft or airports
• Airport dashboards and traffic analytics
• Tagging system for special liveries, VIP flights, etc.
