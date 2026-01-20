# ✈️ PlaneCrazy Roadmap

## Milestone 1 — Project Foundation

• Create project structure (Models, Interfaces, Services, EventStore, Projections, Config)
• Implement PlaneCrazyPaths for Documents folder structure
• Add strongly typed models for Aircraft, Position, Snapshot
• Implement ApiClient and IAdsBRepository
• Implement AdsBFiRepository for adsb.fi API
• Add dependency injection setup


---

## Milestone 2 — Event Store & Domain Events

• Define core domain events (CommentAdded, CommentEdited, CommentDeleted, AircraftFavourited, AirportFavourited, TypeFavourited, AircraftPositionUpdated, etc.)
• Implement JSON‑based event store (append‑only, per‑entity folders)
• Add event metadata (timestamp, entityType, entityId, eventId)
• Implement event serialization/deserialization


---

## Milestone 3 — Projections (Read Models)

• Implement Comments projection (per entity)
• Implement Favourites projection (aircraft, types, airports)
• Implement Aircraft state projection (latest known position, metadata)
• Implement Snapshot/history projection
• Add projection rebuild logic (replay events)


---

## Milestone 4 — Command Handlers

• Add command models (AddComment, EditComment, FavouriteAircraft, etc.)
• Implement command handlers that:
  • Load event stream
  • Rebuild state
  • Validate commands
  • Emit new events
  • Update projections

• Add basic error handling and validation


---

## Milestone 5 — Local Data Storage

• Implement JSON repositories for:
  • Aircraft cache
  • Snapshots
  • Favourites
  • Comments

• Add caching strategy for API responses
• Add offline‑friendly behaviour


---

## Milestone 6 — Favourites & Comments Features

• Add support for favouriting:
  • Aircraft (hex/registration)
  • Aircraft types (ICAO codes, cargo/passenger filters)
  • Airports (ICAO/IATA, name, lat/lon)

• Add comment system for any entity
• Add projection‑based queries:
  • Get comments for entity
  • Get favourites
  • Check if entity is favourited



---

## Milestone 7 — Background Processing

• Add periodic ADS‑B fetcher
• Emit events for aircraft updates
• Update projections automatically
• Add throttling and rate‑limit awareness


---

## Milestone 8 — UI Layer (Future)

• Choose mapping engine (Leaflet, Cesium, MapLibre)
• Display aircraft on map
• Highlight favourites
• Show comments in aircraft detail panel
• Add airport markers
• Add filters (favourites, types, airports)


---

## Milestone 9 — Advanced Features (Future)

• Notifications (favourite aircraft airborne, near favourite airports)
• Tagging system (special liveries, VIP, rare visitor)
• Airport dashboards (arrivals, departures, heatmaps)
• Plugin system for extensibility
• Optional cloud sync for favourites/comments
